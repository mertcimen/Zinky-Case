using System.Collections.Generic;
using _Main.Scripts.Datas;
using UnityEngine;

namespace _Main.Scripts.GridSystem
{
	public class GridManager : MonoBehaviour
	{
		private const float DEFAULT_CELL_SIZE = 1f;

		[SerializeField] private float cellSize = DEFAULT_CELL_SIZE;

		private readonly List<GridCell> gridCells = new();

		public IReadOnlyList<GridCell> GridCells => gridCells;

		public void Initialize(LevelDataSO levelDataSO)
		{
			ClearGrid();

			levelDataSO.EnsureGridData();

			int columnCount = levelDataSO.ColumnCount;
			int rowCount = levelDataSO.RowCount;

			float xCenterOffset = (columnCount - 1) * 0.5f;
			float yCenterOffset = (rowCount - 1) * 0.5f;

			for (int row = 0; row < rowCount; row++)
			{
				for (int column = 0; column < columnCount; column++)
				{
					Vector2Int coordinates = new(column, row);
					GridCell gridCell = CreateGridCell(coordinates);
					Vector3 localPosition = new(
						(column - xCenterOffset) * cellSize,
						(row - yCenterOffset) * cellSize,
						0f
					);
					gridCell.transform.localPosition = localPosition;
					gridCell.Initialize(coordinates, levelDataSO.GetCellColor(row, column));
					gridCells.Add(gridCell);
				}
			}
		}

		private GridCell CreateGridCell(Vector2Int coordinates)
		{
			GridCell gridCell = Instantiate(ReferenceManagerSO.Instance.GridCellPrefab);
			gridCell.transform.SetParent(transform, false);
			gridCell.gameObject.name = $"GridCell_{coordinates.x}_{coordinates.y}";
			return gridCell;
		}

		private void ClearGrid()
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
				Destroy(transform.GetChild(i).gameObject);

			gridCells.Clear();
		}
	}
}
