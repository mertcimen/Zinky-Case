using System.Collections.Generic;
using _Main.Scripts.BallSystem;
using _Main.Scripts.Containers;
using _Main.Scripts.Datas;
using UnityEngine;

namespace _Main.Scripts.GridSystem
{
	public class GridManager : MonoBehaviour
	{
		private const float DEFAULT_CELL_SIZE = 1f;

		[SerializeField] private float cellSize = DEFAULT_CELL_SIZE;

		private readonly List<GridCell> gridCells = new();
		private readonly Dictionary<Vector2Int, GridCell> gridCellLookup = new();
		private int columnCount;
		private int rowCount;

		public IReadOnlyList<GridCell> GridCells => gridCells;
		public int ColumnCount => columnCount;
		public int RowCount => rowCount;

		public void Initialize(LevelDataSO levelDataSO)
		{
			ClearGrid();

			levelDataSO.EnsureGridData();

			columnCount = levelDataSO.ColumnCount;
			rowCount = levelDataSO.RowCount;

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
					gridCell.Initialize(coordinates);
					gridCellLookup.Add(coordinates, gridCell);

					ColorType ballColorType = levelDataSO.GetCellColor(row, column);
					if (ballColorType != ColorType.None)
						CreateBall(gridCell, ballColorType);

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

		public bool IsInsideGrid(Vector2Int coordinates)
		{
			return coordinates.x >= 0 &&
				coordinates.x < columnCount &&
				coordinates.y >= 0 &&
				coordinates.y < rowCount;
		}

		public bool TryGetGridCell(Vector2Int coordinates, out GridCell gridCell)
		{
			return gridCellLookup.TryGetValue(coordinates, out gridCell);
		}

		public bool HasBallAt(Vector2Int coordinates)
		{
			if (!TryGetGridCell(coordinates, out GridCell gridCell))
				return false;

			return gridCell.GetComponentInChildren<BallController>() != null;
		}

		private BallController CreateBall(GridCell gridCell, ColorType colorType)
		{
			BallController ballController = Instantiate(ReferenceManagerSO.Instance.BallControllerPrefab);
			ballController.transform.SetParent(gridCell.transform, false);
			ballController.transform.localPosition = Vector3.zero;
			ballController.Initialize(colorType, gridCell);
			return ballController;
		}

		private void ClearGrid()
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
				Destroy(transform.GetChild(i).gameObject);

			gridCells.Clear();
			gridCellLookup.Clear();
			columnCount = 0;
			rowCount = 0;
		}
	}
}
