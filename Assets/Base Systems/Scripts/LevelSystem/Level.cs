using _Main.Scripts.Datas;
using _Main.Scripts.GamePlay;
using _Main.Scripts.GridSystem;
using _Main.Scripts.MergeSystem;
using _Main.Scripts.RopeSystem;
using Base_Systems.Scripts.Managers;
using UnityEngine;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		[SerializeField] private LevelDataSO levelDataSO;
		[SerializeField] private GridManager gridManager;
		[SerializeField] private RopeController ropeController;
		[SerializeField] private MergeController mergeController;

		private int remainingBallCount;
		private bool hasWinTriggered;

		public int RemainingBallCount => remainingBallCount;

		public virtual void Load()
		{
			gameObject.SetActive(true);
			EnsureMergeController();
			remainingBallCount = levelDataSO.GetInitialBallCount();
			hasWinTriggered = false;
			gridManager.Initialize(levelDataSO);
			ropeController.Initialize(levelDataSO, mergeController);
			InputController.Instance.SetGridManager(gridManager);
		}

		public virtual void Play()
		{
		}

		private void EnsureMergeController()
		{
			if (mergeController == null)
				mergeController = GetComponent<MergeController>();

			if (mergeController == null)
				mergeController = gameObject.AddComponent<MergeController>();
		}

		public void DecreaseRemainingBallCount(int mergedBallCount)
		{
			if (mergedBallCount <= 0)
				return;

			remainingBallCount = Mathf.Max(0, remainingBallCount - mergedBallCount);
			if (hasWinTriggered || remainingBallCount > 0)
				return;

			hasWinTriggered = true;
			LevelManager.Instance.Win();
		}
	}
}
