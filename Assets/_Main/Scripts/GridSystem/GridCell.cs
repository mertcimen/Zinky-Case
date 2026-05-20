using _Main.Scripts.Containers;
using UnityEngine;

namespace _Main.Scripts.GridSystem
{
	public class GridCell : MonoBehaviour
	{
		[SerializeField] private Vector2Int coordinates;
		[SerializeField] private ColorType colorType;

		public Vector2Int Coordinates => coordinates;
		public ColorType ColorType => colorType;

		public void Initialize(Vector2Int gridCoordinates, ColorType cellColorType)
		{
			coordinates = gridCoordinates;
			colorType = cellColorType;
			gameObject.name = $"GridCell_{coordinates.x}_{coordinates.y}";
		}
	}
}
