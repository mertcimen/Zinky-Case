using _Main.Scripts.GamePlay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		[SerializeField, InlineEditor]
		private LevelPrefabDataSO levelData;
		
		public LevelPrefabDataSO LevelData => levelData;
		
		public bool IsLevelHard => levelData.LevelHardness == LevelHardness._1Hard;
		public bool IsLevelExtreme => levelData.LevelHardness == LevelHardness._2Extreme;

		public virtual void Load()
		{
			gameObject.SetActive(true);
			// TimeManager.Instance.Initialize(46);
		}

		public virtual void Play()
		{
		}

		
	}
}
