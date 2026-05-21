using UnityEngine;

namespace _Main.Scripts.GridSystem
{
	public class GridCell : MonoBehaviour
	{
		[SerializeField] private Vector2Int coordinates;

		public Vector2Int Coordinates => coordinates;

		public void Initialize(Vector2Int gridCoordinates)
		{
			coordinates = gridCoordinates;
			gameObject.name = $"GridCell_{coordinates.x}_{coordinates.y}";
		}
	}
}
