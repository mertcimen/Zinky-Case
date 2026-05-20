using System.Collections.Generic;
using _Main.Scripts.Data;
using _Main.Scripts.LevelConfig;
using _Main.Scripts.Utilities;
using Base_Systems.AudioSystem.Scripts;
using Base_Systems.Scripts.Utilities;
using Base_Systems.Scripts.Utilities.Singletons;
using DG.Tweening;
using Fiber.LevelSystem;
using Fiber.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Base_Systems.Scripts.Managers
{
	[DefaultExecutionOrder(-2)]
	public class LevelManager : Singleton<LevelManager>
	{
#if UNITY_EDITOR
		[FoldoutGroup("Developer")]
		[SerializeField] private bool isActiveTestLevel;
		[FoldoutGroup("Developer")]
		[ShowIf(nameof(isActiveTestLevel))]
		[SerializeField] private Level testLevel;
#endif
		[FoldoutGroup("Developer")]
		[SerializeField] private bool enableLevelDebugLogs = true;
		[Space]
		

		private List<int> loopingLevelIndices = new();
		private const string LOOP_LEVEL_KEY = "LoopingLevels";

		private const string LAST_KNOWN_LEVEL_COUNT_KEY = "LastKnownLevelCount";
		private const string PENDING_NEW_LEVELS_KEY = "PendingNewLevels";

		private const string LAST_KNOWN_PLAYORDER_COUNT_KEY = "LastKnownPlayOrderCount";
		private const string PENDING_REMOTE_LEVELS_KEY = "PendingRemoteLevels";

		// [ReadOnly, SerializeField]
		private List<int> pendingNewLevelIndices = new();

		// [ReadOnly, SerializeField]
		private List<int> pendingRemoteLevelIndices = new();

		private int realLevelNo;

		public int LevelNo
		{
			get => PlayerPrefs.GetInt(PlayerPrefsNames.LEVEL_NO, 1);
			set => PlayerPrefs.SetInt(PlayerPrefsNames.LEVEL_NO, value);
		}
		public int RealLevelNo => realLevelNo;

		[Tooltip("Randomizes levels after all levels are played.\nIf this is unchecked, levels will be played again in the same order.")]
		private bool randomizeAfterRotation = true;
		[InlineEditor]
		[SerializeField] private LevelsSO levelsSO;

		public LevelsSO LevelsSO => levelsSO;
		public Level CurrentLevel { get; private set; }

		private int currentLevelIndex;
		public int CurrentLevelIndex => currentLevelIndex;

		public static event UnityAction OnLevelLoad;
		public static event UnityAction OnLevelUnload;
		public static event UnityAction OnLevelStart;
		public static event UnityAction OnLevelRestart;

		public static event UnityAction OnLevelWin;
		public static event UnityAction<int> OnLevelWinWithMoveCount;
		public static event UnityAction OnLevelLose;

		private void Awake()
		{
			if (levelsSO is null || levelsSO.Levels.Count.Equals(0))
			{
				Debug.LogWarning(name + ": There isn't any level added to the script!", this);
			}
		}

		private void Start()
		{
#if UNITY_EDITOR
			var levels = FindObjectsByType<Level>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (var level in levels)
				level.gameObject.SetActive(false);
#endif
			LoadPendingNewLevels();
			UpdatePendingNewLevelsIfNeeded();

			LoadPendingRemoteLevels();

			LoadLoopingLevels();
			SanitizeLoopingList(levelsSO.Levels.Count);
			LoadCurrentLevel(true);
		}

		public void LoadCurrentLevel(bool isStart)
		{
			StateManager.Instance.CurrentState = GameState.OnStart;
			int totalLevels = levelsSO.Levels.Count;
			int requestedLevelIndex = LevelNo - 1;
			realLevelNo = LevelNo;

			var playOrder = LevelConfigManager.Instance.GetPlayOrder;
			UpdatePendingRemoteFromPlayOrder(playOrder, totalLevels);

			if (pendingNewLevelIndices.Count > 0 && requestedLevelIndex >= totalLevels)
			{
				int pendingIndex = pendingNewLevelIndices[0];

				if (pendingIndex >= 0 && pendingIndex < totalLevels)
				{
					currentLevelIndex = pendingIndex;
					pendingNewLevelIndices.RemoveAt(0);
					SavePendingNewLevels();
					if (enableLevelDebugLogs)
						Debug.Log($"[LEVEL] Pending new build level played -> Index:{currentLevelIndex} LevelNo:{LevelNo}");
					LoadLevel(currentLevelIndex);
					SaveLoopingLevels();
					return;
				}

				pendingNewLevelIndices.RemoveAt(0);
				SavePendingNewLevels();
			}

			if (pendingRemoteLevelIndices.Count > 0)
			{
				int pendingRemote = pendingRemoteLevelIndices[0];

				if (pendingRemote >= 0 && pendingRemote < totalLevels)
				{
					currentLevelIndex = pendingRemote;
					pendingRemoteLevelIndices.RemoveAt(0);
					SavePendingRemoteLevels();
					if (enableLevelDebugLogs)
						Debug.Log($"[LEVEL] Pending remote playOrder level played -> Index:{currentLevelIndex} LevelNo:{LevelNo}");
					LoadLevel(currentLevelIndex);
					SaveLoopingLevels();
					return;
				}

				pendingRemoteLevelIndices.RemoveAt(0);
				SavePendingRemoteLevels();
			}

			bool fallbackToLoop = false;

			if (playOrder.Count != 0 && requestedLevelIndex < playOrder.Count)
			{
				int remoteIndex = playOrder[LevelNo - 1] - 1;
				if (remoteIndex < 0 || remoteIndex >= totalLevels)
				{
					fallbackToLoop = true;
					if (enableLevelDebugLogs)
						Debug.Log($"[LEVEL] Remote index invalid -> Fallback to loop | Remote:{remoteIndex} Total:{totalLevels}");
				}
				else
				{
					currentLevelIndex = remoteIndex;
					realLevelNo = requestedLevelIndex;
					if (enableLevelDebugLogs)
						Debug.Log($"[LEVEL] Remote level played -> Index:{currentLevelIndex} LevelNo:{LevelNo}");
				}
			}
			else if (playOrder.Count == 0 && requestedLevelIndex < totalLevels)
			{
				currentLevelIndex = requestedLevelIndex;
				realLevelNo = requestedLevelIndex;
				if (enableLevelDebugLogs)
					Debug.Log($"[LEVEL] Normal level played -> Index:{currentLevelIndex} LevelNo:{LevelNo}");
			}
			else
			{
				fallbackToLoop = true;
				if (enableLevelDebugLogs)
					Debug.Log($"[LEVEL] No valid remote/local -> Fallback to loop | LevelNo:{LevelNo}");
			}

			if (fallbackToLoop)
			{
				if (!isStart && loopingLevelIndices.Count != 0)
					loopingLevelIndices.RemoveAt(0);

				if (loopingLevelIndices.Count == 0)
					PopulateLoopingLevelIndices();
				else
				{
					if (loopingLevelIndices[0] >= totalLevels || loopingLevelIndices[0] < 0)
						PopulateLoopingLevelIndices();
				}

				SanitizeLoopingList(totalLevels);

				while (loopingLevelIndices.Count > 0 && (loopingLevelIndices[0] < 0 || loopingLevelIndices[0] >= totalLevels))
					loopingLevelIndices.RemoveAt(0);

				if (loopingLevelIndices.Count == 0)
				{
					PopulateLoopingLevelIndices();
					SanitizeLoopingList(totalLevels);

					while (loopingLevelIndices.Count > 0 && (loopingLevelIndices[0] < 0 || loopingLevelIndices[0] >= totalLevels))
						loopingLevelIndices.RemoveAt(0);
				}

				if (loopingLevelIndices.Count == 0)
				{
					Debug.LogError("No looping levels found!");
					return;
				}

				currentLevelIndex = loopingLevelIndices[0];
				if (enableLevelDebugLogs)
					Debug.Log($"[LEVEL] Looping level played -> Index:{currentLevelIndex} LevelNo:{LevelNo}");
			}

			LoadLevel(currentLevelIndex);
			SaveLoopingLevels();
		}

		private void UpdatePendingRemoteFromPlayOrder(List<int> playOrder, int totalLevels)
		{
			if (playOrder == null || playOrder.Count == 0) return;

			int lastKnown = PlayerPrefs.GetInt(LAST_KNOWN_PLAYORDER_COUNT_KEY, playOrder.Count);

			if (playOrder.Count > lastKnown)
			{
				for (int i = lastKnown; i < playOrder.Count; i++)
				{
					int idx = playOrder[i] - 1;
					if (idx >= 0 && idx < totalLevels && !pendingRemoteLevelIndices.Contains(idx))
						pendingRemoteLevelIndices.Add(idx);
				}

				SavePendingRemoteLevels();

				if (enableLevelDebugLogs && pendingRemoteLevelIndices.Count > 0)
					Debug.Log($"[LEVEL] Remote playOrder expanded -> PendingRemote [{string.Join(",", pendingRemoteLevelIndices)}]");
			}

			PlayerPrefs.SetInt(LAST_KNOWN_PLAYORDER_COUNT_KEY, playOrder.Count);
			PlayerPrefs.Save();
		}

		private void SavePendingRemoteLevels()
		{
			PlayerPrefs.SetString(PENDING_REMOTE_LEVELS_KEY, string.Join(",", pendingRemoteLevelIndices));
			PlayerPrefs.Save();
		}

		private void LoadPendingRemoteLevels()
		{
			pendingRemoteLevelIndices.Clear();
			string data = PlayerPrefs.GetString(PENDING_REMOTE_LEVELS_KEY, "");
			if (!string.IsNullOrEmpty(data))
			{
				var values = data.Split(',');
				foreach (var v in values)
				{
					if (int.TryParse(v, out int index))
						pendingRemoteLevelIndices.Add(index);
				}
			}

			if (enableLevelDebugLogs && pendingRemoteLevelIndices.Count > 0)
				Debug.Log($"[LEVEL] Pending remote levels loaded -> [{string.Join(",", pendingRemoteLevelIndices)}]");
		}

		private void SanitizeLoopingList(int totalLevels)
		{
			if (loopingLevelIndices == null) return;

			for (int i = loopingLevelIndices.Count - 1; i >= 0; i--)
			{
				int idx = loopingLevelIndices[i];
				if (idx < 0 || idx >= totalLevels)
					loopingLevelIndices.RemoveAt(i);
			}

			if (enableLevelDebugLogs && loopingLevelIndices.Count > 0)
				Debug.Log($"[LEVEL] Looping list sanitized -> [{string.Join(",", loopingLevelIndices)}]");
		}

		private void PopulateLoopingLevelIndices()
		{
			loopingLevelIndices.Clear();
			var loopingLevels = LevelConfigManager.Instance.GetLoopingLevels;
			if (loopingLevels.Count == 0)
			{
				for (int i = 0; i < levelsSO.Levels.Count; i++)
				{
					if (i == 0 && currentLevelIndex != 0 && currentLevelIndex == i)
						continue;

					if (levelsSO.Levels[i].IsLoopingLevel)
						loopingLevelIndices.Add(i);
				}
			}
			else
			{
				for (int i = 0; i < loopingLevels.Count; i++)
				{
					int idx = loopingLevels[i] - 1;
					if (i == 0 && currentLevelIndex != 0 && currentLevelIndex == idx)
						continue;
					if (idx >= 0 && idx < levelsSO.Levels.Count)
						loopingLevelIndices.Add(idx);
				}
			}

			loopingLevelIndices.Shuffle();

			if (enableLevelDebugLogs)
				Debug.Log($"[LEVEL] Looping list populated -> [{string.Join(",", loopingLevelIndices)}]");
		}

		private void SaveLoopingLevels()
		{
			string data = string.Join(",", loopingLevelIndices);
			PlayerPrefs.SetString(LOOP_LEVEL_KEY, data);
			PlayerPrefs.Save();
		}

		private void LoadLoopingLevels()
		{
			loopingLevelIndices.Clear();
			string data = PlayerPrefs.GetString(LOOP_LEVEL_KEY, "");
			if (!string.IsNullOrEmpty(data))
			{
				var values = data.Split(',');
				foreach (var v in values)
				{
					if (int.TryParse(v, out int index))
						loopingLevelIndices.Add(index);
				}
			}

			if (enableLevelDebugLogs && loopingLevelIndices.Count > 0)
				Debug.Log($"[LEVEL] Looping list loaded -> [{string.Join(",", loopingLevelIndices)}]");
		}

		private void UpdatePendingNewLevelsIfNeeded()
		{
			int currentCount = levelsSO.Levels.Count;
			int lastKnown = PlayerPrefs.GetInt(LAST_KNOWN_LEVEL_COUNT_KEY, currentCount);

			if (currentCount > lastKnown)
			{
				for (int i = lastKnown; i < currentCount; i++)
				{
					if (!pendingNewLevelIndices.Contains(i))
						pendingNewLevelIndices.Add(i);
				}
				SavePendingNewLevels();

				if (enableLevelDebugLogs)
					Debug.Log($"[LEVEL] New build levels detected -> PendingBuild [{string.Join(",", pendingNewLevelIndices)}]");
			}

			PlayerPrefs.SetInt(LAST_KNOWN_LEVEL_COUNT_KEY, currentCount);
			PlayerPrefs.Save();
		}

		private void SavePendingNewLevels()
		{
			string data = string.Join(",", pendingNewLevelIndices);
			PlayerPrefs.SetString(PENDING_NEW_LEVELS_KEY, data);
			PlayerPrefs.Save();
		}

		private void LoadPendingNewLevels()
		{
			pendingNewLevelIndices.Clear();
			string data = PlayerPrefs.GetString(PENDING_NEW_LEVELS_KEY, "");
			if (!string.IsNullOrEmpty(data))
			{
				var values = data.Split(',');
				foreach (var v in values)
				{
					if (int.TryParse(v, out int index))
						pendingNewLevelIndices.Add(index);
				}
			}

			if (enableLevelDebugLogs && pendingNewLevelIndices.Count > 0)
				Debug.Log($"[LEVEL] Pending build levels loaded -> [{string.Join(",", pendingNewLevelIndices)}]");
		}

		private void LoadLevel(int index)
		{
			if (index < 0 || index >= levelsSO.Levels.Count)
			{
				if (enableLevelDebugLogs)
					Debug.Log($"[LEVEL] Invalid index requested -> {index}, Total:{levelsSO.Levels.Count} | Forcing loop fallback");
				PopulateLoopingLevelIndices();
				SanitizeLoopingList(levelsSO.Levels.Count);
				if (loopingLevelIndices.Count == 0)
				{
					Debug.LogError($"Invalid level index: {index}");
					return;
				}
				index = loopingLevelIndices[0];
				currentLevelIndex = index;
				if (enableLevelDebugLogs)
					Debug.Log($"[LEVEL] Forced loop level played -> Index:{currentLevelIndex} LevelNo:{LevelNo}");
			}

			LevelData levelData = levelsSO.Levels[index];
			Level levelPrefab = levelData.Level;

#if UNITY_EDITOR
			CurrentLevel = Instantiate(isActiveTestLevel ? testLevel : levelPrefab).GetComponent<Level>();
#else
            CurrentLevel = Instantiate(levelPrefab).GetComponent<Level>();
#endif

			CurrentLevel.Load();
			OnLevelLoad?.Invoke();
			StartLevel();
		}

		public void StartLevel()
		{
			CurrentLevel.Play();
			OnLevelStart?.Invoke();
			EnableHardOrExtremeLevelAnimation();
		}

#if UNITY_EDITOR
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
				RetryLevel();
		}
#endif

		public void RetryLevel()
		{
			UnloadLevel();
			LoadLevel(currentLevelIndex);
		}

		public void RestartFromFirstLevel()
		{
			currentLevelIndex = 0;
			OnLevelRestart?.Invoke();
			RetryLevel();
		}

		public void RestartLevel()
		{
			OnLevelRestart?.Invoke();
			RetryLevel();
		}

		public void LoadNextLevel()
		{
			UnloadLevel();
			LevelNo++;
			LoadCurrentLevel(false);
		}
		public void LoadTargetLevel(int levelNo)
		{
			UnloadLevel();
			LevelNo = levelNo;
			LoadCurrentLevel(false);
		}

		public void LoadBackLevel()
		{
			UnloadLevel();
			LevelNo--;
			LoadCurrentLevel(false);
		}

		private void UnloadLevel()
		{
			OnLevelUnload?.Invoke();
			Destroy(CurrentLevel.gameObject);
		}

		[Button, FoldoutGroup("Developer")]
		public void Win()
		{
			if (StateManager.Instance.CurrentState != GameState.OnStart) return;

			AudioManager.Instance.PlayAudio(AudioName.LevelWin);
			OnLevelWin?.Invoke();
		}

		public void Win(int moveCount)
		{
			if (StateManager.Instance.CurrentState != GameState.OnStart) return;

			AudioManager.Instance.PlayAudio(AudioName.LevelWin);
			OnLevelWinWithMoveCount?.Invoke(moveCount);
		}
		[Button("LOSE"),FoldoutGroup("Developer")]
		private void LoseEditor()
		{
			Lose("NO SPACE!!");
		}
		public void Lose(string loseText)
		{
			if (StateManager.Instance.CurrentState != GameState.OnStart) return;
			UIManager.Instance.SetLosePanelText(loseText);
			AudioManager.Instance.PlayAudio(AudioName.LevelLose);
			OnLevelLose?.Invoke();
		}

		public void StartHardLevelAnimation()
		{
			GameObject hardLevelCanvas = ReferenceManagerSO.Instance.HardLevelCanvas;
			hardLevelCanvas.SetActive(true);
			hardLevelCanvas.GetComponent<Animator>().Play("Empty", 0, 0f);

			DOVirtual.DelayedCall(2.5f, () =>
			{
				hardLevelCanvas.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).onComplete = () =>
				{
					hardLevelCanvas.SetActive(false);
				};
			});
		}

		public void StartExtremeLevelAnimation()
		{
			GameObject extremeLevelCanvas = ReferenceManagerSO.Instance.ExtremeLevelCanvas;
			extremeLevelCanvas.SetActive(true);
			extremeLevelCanvas.GetComponent<Animator>().Play("Empty", 0, 0f);

			DOVirtual.DelayedCall(2.5f, () =>
			{
				extremeLevelCanvas.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).onComplete = () =>
				{
					extremeLevelCanvas.SetActive(false);
				};
			});
		}

		public void EnableHardOrExtremeLevelAnimation()
		{
			if (CurrentLevel.IsLevelHard)
			{
				StartHardLevelAnimation();
				//PlayerPrefs.SetInt(LevelNo.ToString(), 1);
			}
			else if (CurrentLevel.IsLevelExtreme)
			{
				StartExtremeLevelAnimation();
				//PlayerPrefs.SetInt(LevelNo.ToString(), 1);
			}
		}

#if UNITY_EDITOR
		[Button]
		private void AddLevelAssetsToList()
		{
			const string levelPath = "Assets/_Main/Prefabs/Levels/Lev-Des";
			var levels = EditorUtilities.LoadAllAssetsFromPath<Level>(levelPath);
			levelsSO.Levels.Clear();

			foreach (var level in levels)
			{
				if (level.name.ToLower().Contains("test")) continue;
				if (level.name.ToLower().Contains("_base")) continue;
				levelsSO.AddLevel(level);
			}
		}
#endif
	}
}
