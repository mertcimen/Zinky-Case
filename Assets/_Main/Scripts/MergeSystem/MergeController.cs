using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.BallSystem;
using Base_Systems.AudioSystem.Scripts;
using Base_Systems.Scripts.Managers;
using Base_Systems.Scripts.Utilities;
using DG.Tweening;
using UnityEngine;

namespace _Main.Scripts.MergeSystem
{
	public class MergeController : MonoBehaviour
	{
		private const float DefaultStartStaggerDelay = 0.1f;
		private const float DefaultBackwardDistance = 1f;
		private const float DefaultBackwardMoveDuration = 0.2f;
		private const float DefaultCenterAlignDuration = 0.25f;
		private const float DefaultAbsorbDuration = 0.2f;
		private const float DefaultFocusPulseDuration = 0.15f;
		private const float DefaultFocusScaleMultiplier = 1.2f;

		[SerializeField] private float focusFoamDuration = 0.4f;
		[SerializeField] private float focusFoamTargetValue = 0f;

		[SerializeField] private float startStaggerDelay = DefaultStartStaggerDelay;
		[SerializeField] private float backwardDistance = DefaultBackwardDistance;
		[SerializeField] private float backwardMoveDuration = DefaultBackwardMoveDuration;
		[SerializeField] private float centerAlignDuration = DefaultCenterAlignDuration;
		[SerializeField] private float absorbDuration = DefaultAbsorbDuration;
		[SerializeField] private float focusPulseDuration = DefaultFocusPulseDuration;
		[SerializeField] private float focusScaleMultiplier = DefaultFocusScaleMultiplier;
		[SerializeField] private string mergeParticlePoolTag = "Water";

		public IEnumerator PlayMergeSequence(IReadOnlyList<BallController> candidateBalls)
		{
			List<BallController> mergeBalls = CollectValidBalls(candidateBalls);
			if (mergeBalls.Count == 0)
				yield break;

			PrepareBallsForMerge(mergeBalls);

			yield return MoveBallsBackwardInStagger(mergeBalls);
			BallController focusBall = GetClosestBallToCenterX(mergeBalls);
			yield return AlignBallsToCenterXAndFocusY(mergeBalls, focusBall);
			yield return AbsorbBallsIntoFocus(mergeBalls, focusBall);
			yield return PulseFocusBall(focusBall);

			DestroyBalls(mergeBalls, focusBall);
		}

		private List<BallController> CollectValidBalls(IReadOnlyList<BallController> sourceBalls)
		{
			List<BallController> validBalls = new();
			for (int i = 0; i < sourceBalls.Count; i++)
			{
				BallController ballController = sourceBalls[i];
				if (ballController != null)
					validBalls.Add(ballController);
			}

			return validBalls;
		}

		private void PrepareBallsForMerge(IReadOnlyList<BallController> mergeBalls)
		{
			for (int i = 0; i < mergeBalls.Count; i++)
			{
				BallController ballController = mergeBalls[i];
				ballController.PrepareForMerge();
				ballController.transform.DOKill();
			}
		}

		private IEnumerator MoveBallsBackwardInStagger(IReadOnlyList<BallController> mergeBalls)
		{
			int finishedCount = 0;
			int ballCount = mergeBalls.Count;

			for (int i = 0; i < ballCount; i++)
			{
				Transform ballTransform = mergeBalls[i].transform;
				Vector3 targetPosition = ballTransform.position + Vector3.back * backwardDistance;
				ballTransform.DOMove(targetPosition, backwardMoveDuration).SetEase(Ease.InOutSine)
					.SetDelay(i * startStaggerDelay).OnComplete(() => { finishedCount++; });
			}

			yield return new WaitUntil(() => finishedCount >= ballCount);
		}

		private IEnumerator PlayFocusFoamEffect(BallController focusBall)
		{
			if (focusBall == null || focusBall.BallRendererController == null)
				yield break;

			bool isFinished = false;

			focusBall.BallRendererController.DoFoamWidth(focusFoamTargetValue, focusFoamDuration).SetEase(Ease.OutQuad)
				.OnComplete(() => isFinished = true);

			yield return new WaitUntil(() => isFinished);
		}

		private IEnumerator AlignBallsToCenterXAndFocusY(IReadOnlyList<BallController> mergeBalls,
			BallController focusBall)
		{
			int finishedCount = 0;
			int ballCount = mergeBalls.Count;
			float targetY = focusBall != null ? focusBall.transform.position.y : 0f;

			for (int i = 0; i < ballCount; i++)
			{
				Transform ballTransform = mergeBalls[i].transform;
				Vector3 position = ballTransform.position;
				Vector3 targetPosition = new Vector3(0f, targetY, position.z);
				ballTransform.DOMove(targetPosition, centerAlignDuration).SetEase(Ease.InOutSine)
					.OnComplete(() => { finishedCount++; });
			}

			yield return new WaitUntil(() => finishedCount >= ballCount);
		}

		private BallController GetClosestBallToCenterX(IReadOnlyList<BallController> mergeBalls)
		{
			BallController closestBall = mergeBalls[0];
			float closestDistance = Mathf.Abs(closestBall.transform.position.x);

			for (int i = 1; i < mergeBalls.Count; i++)
			{
				BallController ballController = mergeBalls[i];
				float distance = Mathf.Abs(ballController.transform.position.x);
				if (distance >= closestDistance)
					continue;

				closestDistance = distance;
				closestBall = ballController;
			}

			return closestBall;
		}

		private IEnumerator PulseFocusBall(BallController focusBall)
		{
			if (focusBall == null)
				yield break;

			bool isFinished = false;

			Transform focusTransform = focusBall.transform;
			Vector3 initialScale = focusTransform.localScale;

			Sequence sequence = DOTween.Sequence();

			sequence.Join(focusTransform.DOScale(initialScale * focusScaleMultiplier, focusPulseDuration)
				.SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo));

			if (focusBall.BallRendererController != null)
			{
				sequence.Join(focusBall.BallRendererController.DoFoamWidth(focusFoamTargetValue, focusFoamDuration)
					.SetEase(Ease.OutQuad));
			}

			sequence.OnComplete(() => isFinished = true);

			yield return new WaitUntil(() => isFinished);
		}

		private IEnumerator AbsorbBallsIntoFocus(IReadOnlyList<BallController> mergeBalls, BallController focusBall)
		{
			if (focusBall == null)
				yield break;

			int finishedCount = 0;
			int movingBallCount = 0;
			Vector3 focusPosition = focusBall.transform.position;

			for (int i = 0; i < mergeBalls.Count; i++)
			{
				BallController ballController = mergeBalls[i];
				if (ballController == focusBall)
					continue;

				movingBallCount++;
				ballController.transform.DOMove(focusPosition, absorbDuration).SetEase(Ease.InQuad).OnComplete(() =>
				{
					finishedCount++;
					ballController.gameObject.SetActive(false);
				});
			}

			yield return new WaitUntil(() => finishedCount >= movingBallCount);
		}

		private void DestroyBalls(IReadOnlyList<BallController> mergeBalls, BallController focusBall)
		{
			PlayMergeParticle(focusBall);

			int destroyedBallCount = 0;
			for (int i = 0; i < mergeBalls.Count; i++)
			{
				BallController ballController = mergeBalls[i];
				if (ballController == null)
					continue;

				destroyedBallCount++;
				Destroy(ballController.gameObject);
			}

			AudioManager.Instance.PlayAudio(AudioName.Plop1);
			LevelManager.Instance.CurrentLevel.DecreaseRemainingBallCount(destroyedBallCount);
		}

		private void PlayMergeParticle(BallController focusBall)
		{
			if (focusBall == null)
				return;

			if (ParticlePooler.Instance == null)
				return;

			ParticlePooler.Instance.Spawn(mergeParticlePoolTag, focusBall.transform.position, focusBall.ColorType);
		}
	}
}