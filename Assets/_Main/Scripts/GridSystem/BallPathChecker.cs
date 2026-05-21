using _Main.Scripts.BallSystem;
using UnityEngine;

namespace _Main.Scripts.GridSystem
{
	public static class BallPathChecker
	{
		public static bool CanReachBottom(GridManager gridManager, BallController selectedBall)
		{
			if (gridManager == null || selectedBall == null || selectedBall.IsInFreeFall)
				return false;

			GridCell selectedCell = selectedBall.GridCell;
			if (selectedCell == null)
				return false;

			Vector2Int startCoordinates = selectedCell.Coordinates;
			if (!gridManager.IsInsideGrid(startCoordinates))
				return false;

			if (startCoordinates.y == 0)
				return true;

			for (int row = startCoordinates.y - 1; row >= 0; row--)
			{
				Vector2Int checkCoordinates = new(startCoordinates.x, row);
				if (gridManager.HasBallAt(checkCoordinates))
					return false;
			}

			return true;
		}
	}
}
