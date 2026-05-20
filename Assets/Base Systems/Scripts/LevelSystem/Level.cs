using _Main.Scripts.Datas;
using _Main.Scripts.GridSystem;
using UnityEngine;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		[SerializeField] private LevelDataSO levelDataSO;
		[SerializeField] private GridManager gridManager;

		public virtual void Load()
		{
			gameObject.SetActive(true);
			gridManager.Initialize(levelDataSO);
		}

		public virtual void Play()
		{
		}
	}
}
