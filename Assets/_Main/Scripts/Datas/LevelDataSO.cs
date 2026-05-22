using System.Collections.Generic;
using _Main.Scripts.Containers;
using UnityEngine;

namespace _Main.Scripts.Datas
{
	[CreateAssetMenu(fileName = "LevelData", menuName = "Data/Level Data")]
	public class LevelDataSO : ScriptableObject
	{
		private const int DEFAULT_COLUMN_COUNT = 5;
		private const int DEFAULT_ROW_COUNT = 4;
		private const int MIN_GRID_SIZE = 1;
		private const int MIN_ROPE_COUNT = 1;

		[SerializeField] private int columnCount = DEFAULT_COLUMN_COUNT;
		[SerializeField] private int rowCount = DEFAULT_ROW_COUNT;
		[SerializeField] private List<ColorType> cellColors = new();
		[SerializeField] private List<RopeLevelData> ropeDatas = new();

		public int ColumnCount => columnCount;
		public int RowCount => rowCount;
		public int RopeCount => ropeDatas.Count;

		private void OnValidate()
		{
			EnsureGridData();
			EnsureRopeData();
		}

		public void SetGridSize(int columns, int rows)
		{
			columnCount = Mathf.Max(MIN_GRID_SIZE, columns);
			rowCount = Mathf.Max(MIN_GRID_SIZE, rows);
			EnsureGridData();
		}

		public ColorType GetCellColor(int row, int column)
		{
			int index = ToIndex(row, column);
			if (index < 0 || index >= cellColors.Count)
				return ColorType.None;

			return cellColors[index];
		}

		public bool SetCellColor(int row, int column, ColorType colorType)
		{
			int index = ToIndex(row, column);
			if (index < 0 || index >= cellColors.Count)
				return false;

			if (cellColors[index] == colorType)
				return false;

			cellColors[index] = colorType;
			return true;
		}

		public int GetInitialBallCount()
		{
			EnsureGridData();

			int ballCount = 0;
			for (int i = 0; i < cellColors.Count; i++)
			{
				if (cellColors[i] != ColorType.None)
					ballCount++;
			}

			return ballCount;
		}

		public bool EnsureGridData()
		{
			bool isChanged = false;

			int clampedColumns = Mathf.Max(MIN_GRID_SIZE, columnCount);
			if (clampedColumns != columnCount)
			{
				columnCount = clampedColumns;
				isChanged = true;
			}

			int clampedRows = Mathf.Max(MIN_GRID_SIZE, rowCount);
			if (clampedRows != rowCount)
			{
				rowCount = clampedRows;
				isChanged = true;
			}


			int targetCount = columnCount * rowCount;
			if (cellColors == null)
			{
				cellColors = new List<ColorType>(targetCount);
				isChanged = true;
			}

			while (cellColors.Count < targetCount)
			{
				cellColors.Add(ColorType.None);
				isChanged = true;
			}

			if (cellColors.Count > targetCount)
			{
				cellColors.RemoveRange(targetCount, cellColors.Count - targetCount);
				isChanged = true;
			}

			return isChanged;
		}

		public void SetRopeCount(int ropeCount)
		{
			int clampedRopeCount = Mathf.Max(MIN_ROPE_COUNT, ropeCount);
			EnsureRopeData();

			while (ropeDatas.Count < clampedRopeCount)
				ropeDatas.Add(new RopeLevelData());

			if (ropeDatas.Count > clampedRopeCount)
				ropeDatas.RemoveRange(clampedRopeCount, ropeDatas.Count - clampedRopeCount);
		}

		public int GetRopeCapacity(int ropeIndex)
		{
			if (ropeIndex < 0 || ropeIndex >= ropeDatas.Count)
				return 1;

			return ropeDatas[ropeIndex].MaxCapacity;
		}

		public bool SetRopeCapacity(int ropeIndex, int capacity)
		{
			if (ropeIndex < 0 || ropeIndex >= ropeDatas.Count)
				return false;

			return ropeDatas[ropeIndex].SetMaxCapacity(capacity);
		}

		public bool EnsureRopeData()
		{
			bool isChanged = false;

			if (ropeDatas == null)
			{
				ropeDatas = new List<RopeLevelData>();
				isChanged = true;
			}

			while (ropeDatas.Count < MIN_ROPE_COUNT)
			{
				ropeDatas.Add(new RopeLevelData());
				isChanged = true;
			}

			for (int i = 0; i < ropeDatas.Count; i++)
			{
				if (ropeDatas[i] == null)
				{
					ropeDatas[i] = new RopeLevelData();
					isChanged = true;
					continue;
				}

				if (ropeDatas[i].EnsureData())
					isChanged = true;
			}

			return isChanged;
		}

		private int ToIndex(int row, int column)
		{
			if (row < 0 || row >= rowCount || column < 0 || column >= columnCount)
				return -1;

			return row * columnCount + column;
		}
	}
}
