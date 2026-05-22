using _Main.Scripts.GridSystem;
using UnityEngine;

namespace _Main.Scripts.GamePlay
{
	public readonly struct TapInputContext
	{
		public TapInputContext(
			InputController inputController,
			Camera camera,
			Vector3 screenPosition,
			Ray ray,
			RaycastHit hit,
			bool isTouchInput)
		{
			InputController = inputController;
			Camera = camera;
			ScreenPosition = screenPosition;
			Ray = ray;
			Hit = hit;
			IsTouchInput = isTouchInput;
		}

		public InputController InputController { get; }
		public Camera Camera { get; }
		public Vector3 ScreenPosition { get; }
		public Ray Ray { get; }
		public RaycastHit Hit { get; }
		public bool IsTouchInput { get; }
		public GridManager ActiveGridManager => InputController.ActiveGridManager;
	}
}
