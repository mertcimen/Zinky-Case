using UnityEngine;

namespace _Main.Scripts.Datas
{
	[System.Serializable]
	public class RopeLevelData
	{
		private const int MIN_CAPACITY = 1;
		private const int DEFAULT_CAPACITY = 6;

		[SerializeField] private int maxCapacity = DEFAULT_CAPACITY;

		public int MaxCapacity => maxCapacity;

		public bool SetMaxCapacity(int capacity)
		{
			int clampedCapacity = Mathf.Max(MIN_CAPACITY, capacity);
			if (clampedCapacity == maxCapacity)
				return false;

			maxCapacity = clampedCapacity;
			return true;
		}

		public bool EnsureData()
		{
			int clampedCapacity = Mathf.Max(MIN_CAPACITY, maxCapacity);
			if (clampedCapacity == maxCapacity)
				return false;

			maxCapacity = clampedCapacity;
			return true;
		}
	}
}
