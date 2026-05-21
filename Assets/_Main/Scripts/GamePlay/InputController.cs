using _Main.Scripts.BallSystem;
using _Main.Scripts.GridSystem;
using Base_Systems.Scripts.Managers;
using Base_Systems.Scripts.Utilities.Singletons;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Main.Scripts.GamePlay
{
	public class InputController : SingletonPersistent<InputController>
	{
		private const float DefaultRayDistance = 100f;

		[SerializeField] private LayerMask selectableLayerMask = Physics.DefaultRaycastLayers;
		[SerializeField] private float rayDistance = DefaultRayDistance;

		private GridManager activeGridManager;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void CreateControllerIfMissing()
		{
			if (Instance != null)
				return;

			GameObject controllerObject = new("Input Controller");
			controllerObject.AddComponent<InputController>();
		}

		private void OnEnable()
		{
			LevelManager.OnLevelUnload += ClearGridManager;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelUnload -= ClearGridManager;
		}

		public void SetGridManager(GridManager gridManager)
		{
			activeGridManager = gridManager;
		}

		private void Update()
		{
			if (!IsSelectionInputStarted())
				return;

			if (IsPointerOverUI())
				return;

			BallController selectedBall = TryGetSelectedBall();
			if (selectedBall == null)
				return;

			if (!BallPathChecker.CanReachBottom(activeGridManager, selectedBall))
				return;

			selectedBall.TriggerFallSequence();
		}

		private static bool IsSelectionInputStarted()
		{
			if (Input.GetMouseButtonDown(0))
				return true;

			if (Input.touchCount == 0)
				return false;

			return Input.GetTouch(0).phase == TouchPhase.Began;
		}

		private static bool IsPointerOverUI()
		{
			if (EventSystem.current == null)
				return false;

			if (Input.touchCount > 0)
				return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

			return EventSystem.current.IsPointerOverGameObject();
		}

		private BallController TryGetSelectedBall()
		{
			Camera mainCamera = Camera.main;
			if (mainCamera == null)
				return null;

			Vector3 screenPosition = Input.touchCount > 0
				? (Vector3)Input.GetTouch(0).position
				: Input.mousePosition;

			Ray ray = mainCamera.ScreenPointToRay(screenPosition);
			if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, selectableLayerMask))
				return null;

			return hit.collider.GetComponentInParent<BallController>();
		}

		private void ClearGridManager()
		{
			activeGridManager = null;
		}
	}
}
