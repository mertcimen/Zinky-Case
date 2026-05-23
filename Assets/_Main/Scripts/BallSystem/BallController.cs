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
	public class BallController : MonoBehaviour, ITapInteractable, ITapRejectedFeedback
	{
		[Header("Components")]
		[SerializeField] private Rigidbody rb;
		[SerializeField] private Collider collider;
		[SerializeField] private BallRendererController ballRendererController;

		[Header("Invalid Tap Feedback")]
		[SerializeField] private float invalidTapFeedbackDuration = 0.18f;
		[SerializeField] private Vector3 invalidTapPositionPunch = new Vector3(0.18f, 0.06f, 0f);
		[SerializeField] private float invalidTapScalePunch = 0.14f;
		[SerializeField] private int invalidTapVibrato = 10;
		[SerializeField] private float invalidTapElasticity = 0.75f;

		private const RigidbodyConstraints InitialConstraints = RigidbodyConstraints.FreezePositionX |
		                                                        RigidbodyConstraints.FreezePositionY |
		                                                        RigidbodyConstraints.FreezePositionZ;
		private const RigidbodyConstraints FreeFallConstraints = RigidbodyConstraints.FreezePositionZ |
		                                                         RigidbodyConstraints.FreezeRotation;

		private ColorType colorType;
		private GridCell gridCell;
		private bool isInFreeFall;
		private float dropSequenceBackMoveDuration = 0.2f;
		private Tween invalidTapFeedbackTween;
		private bool isInvalidTapFeedbackPlaying;

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

		public void HandleTapRejected(TapInputContext inputContext)
		{
			if (isInFreeFall)
				return;

			PlayInvalidTapFeedback();
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

			StopInvalidTapFeedback();
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

		private void PlayInvalidTapFeedback()
		{
			if (isInvalidTapFeedbackPlaying)
				return;

			isInvalidTapFeedbackPlaying = true;
			Vector3 startLocalPosition = transform.localPosition;
			Vector3 startLocalScale = transform.localScale;

			float duration = Mathf.Max(0.01f, invalidTapFeedbackDuration);
			Sequence sequence = DOTween.Sequence();
			sequence.Join(transform.DOPunchPosition(invalidTapPositionPunch, duration, invalidTapVibrato,
				invalidTapElasticity, false));
			sequence.Join(transform.DOPunchScale(Vector3.one * invalidTapScalePunch, duration, invalidTapVibrato,
				invalidTapElasticity));
			sequence.OnKill(() =>
			{
				transform.localPosition = startLocalPosition;
				transform.localScale = startLocalScale;
				isInvalidTapFeedbackPlaying = false;
				invalidTapFeedbackTween = null;
			});
			sequence.OnComplete(() =>
			{
				transform.localPosition = startLocalPosition;
				transform.localScale = startLocalScale;
				isInvalidTapFeedbackPlaying = false;
				invalidTapFeedbackTween = null;
			});
			invalidTapFeedbackTween = sequence;
		}

		private void StopInvalidTapFeedback()
		{
			if (invalidTapFeedbackTween != null && invalidTapFeedbackTween.IsActive())
				invalidTapFeedbackTween.Kill();

			isInvalidTapFeedbackPlaying = false;
			invalidTapFeedbackTween = null;
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

		private void OnDisable()
		{
			StopInvalidTapFeedback();
		}
	}
}
