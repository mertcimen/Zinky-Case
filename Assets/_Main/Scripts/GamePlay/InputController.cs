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
		private const int DefaultRaycastHitBufferSize = 16;

		[SerializeField] private LayerMask selectableLayerMask = Physics.DefaultRaycastLayers;
		[SerializeField] private float rayDistance = DefaultRayDistance;
		[SerializeField] private int raycastHitBufferSize = DefaultRaycastHitBufferSize;

		private GridManager activeGridManager;
		private RaycastHit[] raycastHitsBuffer;
		private Camera mainCamera;
		public GridManager ActiveGridManager => activeGridManager;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private void Start()
		{
			mainCamera = Camera.main;
		}

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
			EnsureRaycastHitBuffer();
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

			if (TryGetTapInteractable(out ITapInteractable tapInteractable, out TapInputContext tapInputContext,
				    out ITapRejectedFeedback rejectedFeedback, out TapInputContext rejectedTapInputContext))
			{
				tapInteractable.HandleTap(tapInputContext);
				return;
			}

			rejectedFeedback?.HandleTapRejected(rejectedTapInputContext);
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

		private bool TryGetTapInteractable(out ITapInteractable tapInteractable, out TapInputContext tapInputContext,
			out ITapRejectedFeedback rejectedFeedback, out TapInputContext rejectedTapInputContext)
		{
			tapInteractable = null;
			tapInputContext = default;
			rejectedFeedback = null;
			rejectedTapInputContext = default;

			if (mainCamera == null)
				return false;

			Vector3 screenPosition = Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
			bool isTouchInput = Input.touchCount > 0;

			Ray ray = mainCamera.ScreenPointToRay(screenPosition);
			EnsureRaycastHitBuffer();
			int hitCount = Physics.RaycastNonAlloc(ray, raycastHitsBuffer, rayDistance, selectableLayerMask);
			if (hitCount <= 0)
				return false;

			float closestDistance = float.MaxValue;
			float closestRejectedDistance = float.MaxValue;
			for (int i = 0; i < hitCount; i++)
			{
				RaycastHit hit = raycastHitsBuffer[i];
				Collider hitCollider = hit.collider;
				if (hitCollider == null)
					continue;

				ITapInteractable candidateTapInteractable = hitCollider.GetComponentInParent<ITapInteractable>();
				if (candidateTapInteractable == null)
					continue;

				TapInputContext candidateContext =
					new TapInputContext(this, mainCamera, screenPosition, ray, hit, isTouchInput);
				if (!candidateTapInteractable.CanHandleTap(candidateContext))
				{
					if (candidateTapInteractable is ITapRejectedFeedback candidateRejectedFeedback &&
					    hit.distance < closestRejectedDistance)
					{
						closestRejectedDistance = hit.distance;
						rejectedFeedback = candidateRejectedFeedback;
						rejectedTapInputContext = candidateContext;
					}

					continue;
				}

				if (hit.distance >= closestDistance)
					continue;

				closestDistance = hit.distance;
				tapInteractable = candidateTapInteractable;
				tapInputContext = candidateContext;
			}

			return tapInteractable != null;
		}

		private void ClearGridManager()
		{
			activeGridManager = null;
		}

		private void EnsureRaycastHitBuffer()
		{
			int clampedBufferSize = Mathf.Max(1, raycastHitBufferSize);
			if (raycastHitsBuffer != null && raycastHitsBuffer.Length == clampedBufferSize)
				return;

			raycastHitsBuffer = new RaycastHit[clampedBufferSize];
		}
	}
}
