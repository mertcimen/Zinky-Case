using System.Collections;
using _Main.Scripts.Containers;
using _Main.Scripts.GridSystem;
using Base_Systems.Scripts.Managers;
using DG.Tweening;
using UnityEngine;

namespace _Main.Scripts.BallSystem
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(BallRendererController))]
	public class BallController : MonoBehaviour
	{
		[Header("Components")]
		[SerializeField] private Rigidbody rb;
		[SerializeField] private BallRendererController ballRendererController;

		private const RigidbodyConstraints InitialConstraints = RigidbodyConstraints.FreezePositionX |
		                                                        RigidbodyConstraints.FreezePositionY |
		                                                        RigidbodyConstraints.FreezePositionZ;
		private const RigidbodyConstraints FreeFallConstraints = RigidbodyConstraints.FreezePositionZ |
		                                                         RigidbodyConstraints.FreezeRotation;

		private ColorType colorType;
		private GridCell gridCell;
		private bool isInFreeFall;
		private float dropSequenceBackMoveDuration = 0.5f;

		public ColorType ColorType => colorType;
		public GridCell GridCell => gridCell;
		public bool IsInFreeFall => isInFreeFall;

		public void Initialize(ColorType colorType, GridCell ownerGridCell)
		{
			if (ballRendererController == null)
			{
				if (TryGetComponent<BallRendererController>(out var rendererController))
				{
					ballRendererController = rendererController;
				}
			}

			ballRendererController.Initialize(ColorType);
			SetColorType(colorType);
			gridCell = ownerGridCell;
			isInFreeFall = false;
			rb.isKinematic = true;
			rb.constraints = InitialConstraints;
		}

		public void TriggerFallSequence()
		{
			if (isInFreeFall)
				return;

			isInFreeFall = true;
			gridCell = null;
			transform.SetParent(LevelManager.Instance.CurrentLevel.transform, true);
			StartCoroutine(FallSequence());
		}

		private IEnumerator FallSequence()
		{
			transform.DOMove(transform.position + Vector3.back * 1.5f, dropSequenceBackMoveDuration);
			yield return new WaitForSeconds(dropSequenceBackMoveDuration);
			EnterFreeFall();
		}

		private bool EnterFreeFall()
		{
			rb.isKinematic = false;
			rb.constraints = FreeFallConstraints;
			rb.WakeUp();
			return true;
		}

		private void SetColorType(ColorType colorType)
		{
			this.colorType = colorType;
		}
	}
}