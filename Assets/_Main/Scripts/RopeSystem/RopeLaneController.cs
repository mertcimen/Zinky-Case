using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.BallSystem;
using _Main.Scripts.Containers;
using _Main.Scripts.MergeSystem;
using Obi;
using UnityEngine;

namespace _Main.Scripts.RopeSystem
{
	public class RopeLaneController : MonoBehaviour
	{
		private const int MatchCount = 3;
		private const float DefaultSettleVelocityThreshold = 0.2f;
		private const float DefaultSettleTimeout = 2f;
		private const float DefaultMergeStepDelay = 0.1f;

		[SerializeField] private int ropeIndex;
		[SerializeField] private int maxCapacity;
		[SerializeField] private ObiRope obiRope;
		[SerializeField] private RopeLaneBreakController ropeLaneBreakController;
		[SerializeField] private RopeLaneStretchController ropeLaneStretchController;
		[SerializeField] private ObiParticleAttachment leftEdgeAttachment;
		[SerializeField] private ObiParticleAttachment rightEdgeAttachment;
		[SerializeField] private Collider landingTrigger;
		[SerializeField] private float settleVelocityThreshold = DefaultSettleVelocityThreshold;
		[SerializeField] private float settleTimeout = DefaultSettleTimeout;
		[SerializeField] private float mergeStepDelay = DefaultMergeStepDelay;
		[SerializeField] private Vector3 triggerCenter = new Vector3(0f, 1.8f, 0f);
		[SerializeField] private Vector3 triggerSize = new Vector3(12f, 3f, 1.5f);

		[SerializeField] private List<BallController> ballsOnRope = new();
		private readonly HashSet<BallController> pendingLandingBalls = new();
		private Coroutine mergeSequenceCoroutine;
		private Coroutine capacityResolutionCoroutine;
		private bool isLaneActive;
		private bool isBroken;
		private int totalMergedBallCount;
		private MergeController mergeController;
		private RopeController ropeController;

		public int RopeIndex => ropeIndex;
		public int MaxCapacity => maxCapacity;
		public int BallCount => ballsOnRope.Count;
		public IReadOnlyList<BallController> BallsOnRope => ballsOnRope;
		public ObiRope ObiRope => obiRope;

		public void Initialize(int laneIndex, int capacity, MergeController mergeController, RopeController ropeController)
		{
			ropeIndex = laneIndex;
			maxCapacity = Mathf.Max(1, capacity);
			this.mergeController = mergeController;
			this.ropeController = ropeController;
			if (obiRope == null)
				obiRope = GetComponentInChildren<ObiRope>();

			ResolveBreakControllerIfNeeded();
			ResolveStretchControllerIfNeeded();
			ropeLaneBreakController?.Initialize(obiRope);
			ropeLaneStretchController?.Initialize(obiRope);
			ResolveEdgeAttachmentsIfNeeded();
			EnsureLandingTrigger();
			isBroken = false;
			totalMergedBallCount = 0;
			pendingLandingBalls.Clear();
			CleanupNullBalls();
			UpdateStretching();
			gameObject.name = $"Rope_{laneIndex + 1}";
		}

		public void SetLaneActive(bool isActive)
		{
			if (isBroken)
				isActive = false;

			isLaneActive = isActive;
			EnsureLandingTrigger();
			landingTrigger.enabled = isLaneActive;
		}

		public void SetAttachmentTargets(Transform leftTarget, Transform rightTarget)
		{
			ResolveEdgeAttachmentsIfNeeded();
			leftEdgeAttachment.target = leftTarget;
			rightEdgeAttachment.target = rightTarget;
		}

		public bool TryRegisterBall(BallController ballController)
		{
			if (ballController == null || isBroken)
				return false;

			CleanupNullBalls();
			if (ballsOnRope.Count >= maxCapacity)
				return false;

			if (ballsOnRope.Contains(ballController))
				return false;

			ballsOnRope.Add(ballController);
			UpdateStretching();
			TriggerMergeCheck();
			if (ballsOnRope.Count >= maxCapacity)
				TriggerCapacityResolution();
			return true;
		}

		public void UnregisterBall(BallController ballController)
		{
			if (ballController == null)
				return;

			if (!ballsOnRope.Remove(ballController))
				return;

			UpdateStretching();
		}

		public int GetColorMatchCount(ColorType colorType)
		{
			int count = 0;
			for (int i = 0; i < ballsOnRope.Count; i++)
			{
				BallController ballController = ballsOnRope[i];
				if (ballController == null)
					continue;

				if (ballController.ColorType == colorType)
					count++;
			}

			return count;
		}

		private void CleanupNullBalls()
		{
			for (int i = ballsOnRope.Count - 1; i >= 0; i--)
			{
				if (ballsOnRope[i] == null)
					ballsOnRope.RemoveAt(i);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!isLaneActive || isBroken)
				return;

			BallController ballController = other.GetComponentInParent<BallController>();
			if (ballController == null || !ballController.IsInFreeFall)
				return;

			CleanupNullBalls();
			if (ballsOnRope.Contains(ballController) || pendingLandingBalls.Contains(ballController))
				return;

			pendingLandingBalls.Add(ballController);
			StartCoroutine(WaitForLandingAndRegister(ballController));
		}

		private void OnTriggerExit(Collider other)
		{
			BallController ballController = other.GetComponentInParent<BallController>();
			if (ballController == null)
				return;

			pendingLandingBalls.Remove(ballController);
		}

		private IEnumerator WaitForLandingAndRegister(BallController ballController)
		{
			float elapsedTime = 0f;
			float squaredVelocityThreshold = settleVelocityThreshold * settleVelocityThreshold;
			bool isRegistered = false;

			while (elapsedTime < settleTimeout)
			{
				if (ballController == null)
					break;

				if (!isLaneActive || !pendingLandingBalls.Contains(ballController))
					break;

				Rigidbody ballRigidbody = ballController.Rigidbody;
				if (ballRigidbody.velocity.sqrMagnitude <= squaredVelocityThreshold)
				{
					TryRegisterBall(ballController);
					isRegistered = true;
					break;
				}

				elapsedTime += Time.deltaTime;
				yield return null;
			}

			if (!isRegistered && ballController != null && isLaneActive && pendingLandingBalls.Contains(ballController))
				TryRegisterBall(ballController);

			pendingLandingBalls.Remove(ballController);
		}

		private void ResolveEdgeAttachmentsIfNeeded()
		{
			if (leftEdgeAttachment != null && rightEdgeAttachment != null)
				return;

			ObiParticleAttachment[] attachments = GetComponents<ObiParticleAttachment>();
			for (int i = 0; i < attachments.Length; i++)
			{
				ObiParticleAttachment attachment = attachments[i];
				if (!attachment.enabled || attachment.attachmentType != ObiParticleAttachment.AttachmentType.Static)
					continue;

				if (leftEdgeAttachment == null)
				{
					leftEdgeAttachment = attachment;
					continue;
				}

				if (rightEdgeAttachment == null && attachment != leftEdgeAttachment)
				{
					rightEdgeAttachment = attachment;
					return;
				}
			}
		}

		private void ResolveBreakControllerIfNeeded()
		{
			if (ropeLaneBreakController == null)
				ropeLaneBreakController = GetComponent<RopeLaneBreakController>();

			if (ropeLaneBreakController == null)
				ropeLaneBreakController = gameObject.AddComponent<RopeLaneBreakController>();
		}

		private void ResolveStretchControllerIfNeeded()
		{
			if (ropeLaneStretchController == null)
				ropeLaneStretchController = GetComponent<RopeLaneStretchController>();

			if (ropeLaneStretchController == null)
				ropeLaneStretchController = gameObject.AddComponent<RopeLaneStretchController>();
		}

		private void EnsureLandingTrigger()
		{
			if (landingTrigger == null)
				landingTrigger = GetComponent<Collider>();

			if (landingTrigger == null)
			{
				BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
				boxCollider.isTrigger = true;
				boxCollider.center = triggerCenter;
				boxCollider.size = triggerSize;
				landingTrigger = boxCollider;
			}

			if (landingTrigger is BoxCollider landingBoxCollider)
			{
				landingBoxCollider.isTrigger = true;
				landingBoxCollider.center = triggerCenter;
				landingBoxCollider.size = triggerSize;
			}
		}

		private void OnDisable()
		{
			if (capacityResolutionCoroutine != null)
			{
				StopCoroutine(capacityResolutionCoroutine);
				capacityResolutionCoroutine = null;
			}

			ropeLaneStretchController?.StopAnimation();
			ropeLaneBreakController?.StopEffects();
		}

		private void TriggerMergeCheck()
		{
			if (mergeSequenceCoroutine != null)
				return;

			mergeSequenceCoroutine = StartCoroutine(RunMergeSequence());
		}

		private IEnumerator RunMergeSequence()
		{
			yield return null;

			while (TryGetMatchedColor(out ColorType matchedColorType))
			{
				List<BallController> matchedBalls = CollectMatchedBalls(matchedColorType, MatchCount);
				if (matchedBalls.Count < MatchCount)
					break;

				totalMergedBallCount += matchedBalls.Count;

				for (int i = 0; i < matchedBalls.Count; i++)
				{
					BallController ballController = matchedBalls[i];
					if (ballController == null)
						continue;

					UnregisterBall(ballController);
					pendingLandingBalls.Remove(ballController);
				}

				if (mergeController != null)
					yield return mergeController.PlayMergeSequence(matchedBalls);
				else
					DestroyMatchedBallsImmediately(matchedBalls);

				yield return new WaitForSeconds(mergeStepDelay);
			}

			mergeSequenceCoroutine = null;
		}

		private void TriggerCapacityResolution()
		{
			if (capacityResolutionCoroutine != null || isBroken)
				return;

			int mergedBallCountBeforeFull = totalMergedBallCount;
			capacityResolutionCoroutine = StartCoroutine(ResolveCapacityReached(mergedBallCountBeforeFull));
		}

		private IEnumerator ResolveCapacityReached(int mergedBallCountBeforeFull)
		{
			yield return null;

			while (mergeSequenceCoroutine != null)
				yield return null;

			bool hasMergedAfterFull = totalMergedBallCount > mergedBallCountBeforeFull;
			ropeController?.HandleLaneCapacityReached(this, hasMergedAfterFull);
			capacityResolutionCoroutine = null;
		}

		private bool TryGetMatchedColor(out ColorType matchedColorType)
		{
			matchedColorType = ColorType.None;
			Dictionary<ColorType, int> colorCounts = new();

			for (int i = 0; i < ballsOnRope.Count; i++)
			{
				BallController ballController = ballsOnRope[i];
				if (ballController == null)
					continue;

				ColorType colorType = ballController.ColorType;
				if (colorType == ColorType.None)
					continue;

				if (!colorCounts.TryGetValue(colorType, out int currentCount))
					currentCount = 0;

				currentCount++;
				colorCounts[colorType] = currentCount;

				if (currentCount >= MatchCount)
				{
					matchedColorType = colorType;
					return true;
				}
			}

			return false;
		}

		private List<BallController> CollectMatchedBalls(ColorType colorType, int count)
		{
			List<BallController> matchedBalls = new(count);

			for (int i = 0; i < ballsOnRope.Count; i++)
			{
				BallController ballController = ballsOnRope[i];
				if (ballController == null || ballController.ColorType != colorType)
					continue;

				matchedBalls.Add(ballController);
				if (matchedBalls.Count == count)
					return matchedBalls;
			}

			return matchedBalls;
		}

		public List<BallController> BreakLaneAndCollectRemainingBalls()
		{
			isBroken = true;
			isLaneActive = false;

			if (landingTrigger != null)
				landingTrigger.enabled = false;

			pendingLandingBalls.Clear();
			if (capacityResolutionCoroutine != null)
			{
				StopCoroutine(capacityResolutionCoroutine);
				capacityResolutionCoroutine = null;
			}

			if (mergeSequenceCoroutine != null)
			{
				StopCoroutine(mergeSequenceCoroutine);
				mergeSequenceCoroutine = null;
			}

			ropeLaneStretchController?.StopAnimation();

			ropeLaneBreakController?.HandleRopeBreak(ropeIndex);
			CleanupNullBalls();

			List<BallController> remainingBalls = new(ballsOnRope.Count);
			for (int i = 0; i < ballsOnRope.Count; i++)
			{
				BallController ballController = ballsOnRope[i];
				if (ballController != null)
					remainingBalls.Add(ballController);
			}

			ballsOnRope.Clear();
			return remainingBalls;
		}

		private void DestroyMatchedBallsImmediately(IReadOnlyList<BallController> matchedBalls)
		{
			for (int i = 0; i < matchedBalls.Count; i++)
			{
				BallController ballController = matchedBalls[i];
				if (ballController != null)
					Destroy(ballController.gameObject);
			}
		}

		private void UpdateStretching()
		{
			ropeLaneStretchController?.UpdateStretchForBallCount(ballsOnRope.Count, !isBroken);
		}
	}
}
