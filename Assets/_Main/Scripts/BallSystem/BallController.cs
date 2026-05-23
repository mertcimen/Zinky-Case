using System.Collections;
using _Main.Scripts.Containers;
using _Main.Scripts.GamePlay;
using _Main.Scripts.GridSystem;
using Base_Systems.Scripts.Managers;
using DG.Tweening;
using UnityEngine;

namespace _Main.Scripts.BallSystem
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(BallRendererController))]
	public class BallController : MonoBehaviour, ITapInteractable
	{
		[Header("Components")]
		[SerializeField] private Rigidbody rb;
		[SerializeField] private Collider collider;
		[SerializeField] private BallRendererController ballRendererController;

		private const RigidbodyConstraints InitialConstraints = RigidbodyConstraints.FreezePositionX |
		                                                        RigidbodyConstraints.FreezePositionY |
		                                                        RigidbodyConstraints.FreezePositionZ;
		private const RigidbodyConstraints FreeFallConstraints = RigidbodyConstraints.FreezePositionZ |
		                                                         RigidbodyConstraints.FreezeRotation;

		private ColorType colorType;
		private GridCell gridCell;
		private bool isInFreeFall;
		private float dropSequenceBackMoveDuration = 0.2f;

		public BallRendererController BallRendererController => ballRendererController;
		public ColorType ColorType => colorType;
		public GridCell GridCell => gridCell;
		public bool IsInFreeFall => isInFreeFall;
		public Rigidbody Rigidbody => rb;

		public void Initialize(ColorType colorType, GridCell ownerGridCell)
		{
			if (ballRendererController == null)
			{
				if (TryGetComponent<BallRendererController>(out var rendererController))
				{
					ballRendererController = rendererController;
				}
			}

			SetColorType(colorType);
			ballRendererController.Initialize(ColorType);
			gridCell = ownerGridCell;
			isInFreeFall = false;
			rb.isKinematic = true;
			rb.constraints = InitialConstraints;
		}

		public bool CanHandleTap(TapInputContext inputContext)
		{
			if (isInFreeFall)
				return false;

			return BallPathChecker.CanReachBottom(inputContext.ActiveGridManager, this);
		}

		public void HandleTap(TapInputContext inputContext)
		{
			TriggerFallSequence();
		}

		private IEnumerator FallSequence()
		{
			transform.DOScale(Vector3.one * 1.2f, dropSequenceBackMoveDuration).OnComplete((() =>
			{
				transform.DOScale(Vector3.one, dropSequenceBackMoveDuration / 2f);
			}));

			transform.DOMove(transform.position + Vector3.back * 1.5f, dropSequenceBackMoveDuration);
			yield return new WaitForSeconds(dropSequenceBackMoveDuration);
			EnterFreeFall();
		}

		private void TriggerFallSequence()
		{
			if (isInFreeFall)
				return;

			isInFreeFall = true;
			gridCell = null;
			transform.SetParent(LevelManager.Instance.CurrentLevel.transform, true);
			StartCoroutine(FallSequence());
		}

		private bool EnterFreeFall()
		{
			rb.isKinematic = false;
			rb.constraints = FreeFallConstraints;
			rb.WakeUp();
			rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

			return true;
		}

		public void PrepareForMerge()
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.isKinematic = true;
			rb.constraints = InitialConstraints;

			if (collider != null)
				collider.enabled = false;
		}

		public void ReleaseFromBrokenRope()
		{
			transform.DOKill();
			transform.SetParent(LevelManager.Instance.CurrentLevel.transform, true);

			if (collider != null)
				collider.enabled = true;

			isInFreeFall = true;
			gridCell = null;
			rb.isKinematic = false;
			rb.constraints = FreeFallConstraints;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.WakeUp();
		}

		private void SetColorType(ColorType colorType)
		{
			this.colorType = colorType;
		}
	}
}