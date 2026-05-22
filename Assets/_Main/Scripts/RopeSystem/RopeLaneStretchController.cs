using System.Collections;
using Obi;
using UnityEngine;

namespace _Main.Scripts.RopeSystem
{
	public class RopeLaneStretchController : MonoBehaviour
	{
		private const float DefaultStretchStepPerBall = 0.1f;
		private const float DefaultStretchIncreaseDuration = 1.5f;
		private const float DefaultStretchDecreaseDuration = 1.5f;
		private const float DefaultMinStretchingScale = 0.1f;
		private const float DefaultMaxStretchingScale = 5f;

		[SerializeField] private ObiRope obiRope;
		[SerializeField] private float stretchStepPerBall = DefaultStretchStepPerBall;
		[SerializeField] private float stretchIncreaseDuration = DefaultStretchIncreaseDuration;
		[SerializeField] private float stretchDecreaseDuration = DefaultStretchDecreaseDuration;
		[SerializeField] private float minStretchingScale = DefaultMinStretchingScale;
		[SerializeField] private float maxStretchingScale = DefaultMaxStretchingScale;

		private float baseStretchingScale;
		private Coroutine stretchScaleRoutine;

		public void Initialize(ObiRope rope)
		{
			if (rope != null)
				obiRope = rope;

			if (obiRope == null)
				obiRope = GetComponentInChildren<ObiRope>();

			if (obiRope == null)
				return;

			baseStretchingScale = obiRope.stretchingScale;
			StopAnimation();
		}

		public void UpdateStretchForBallCount(int ballCount, bool canAnimate)
		{
			if (obiRope == null || !canAnimate)
				return;

			float targetStretchingScale = Mathf.Clamp(
				baseStretchingScale + ballCount * stretchStepPerBall,
				minStretchingScale,
				maxStretchingScale
			);

			float currentStretchingScale = obiRope.stretchingScale;
			bool isIncreasing = targetStretchingScale > currentStretchingScale;
			float duration = isIncreasing ? stretchIncreaseDuration : stretchDecreaseDuration;
			if (duration <= 0f)
			{
				obiRope.stretchingScale = targetStretchingScale;
				return;
			}

			StopAnimation();
			stretchScaleRoutine = StartCoroutine(AnimateStretchingScale(currentStretchingScale, targetStretchingScale, duration));
		}

		public void StopAnimation()
		{
			if (stretchScaleRoutine == null)
				return;

			StopCoroutine(stretchScaleRoutine);
			stretchScaleRoutine = null;
		}

		private void OnDisable()
		{
			StopAnimation();
		}

		private IEnumerator AnimateStretchingScale(float startValue, float endValue, float duration)
		{
			float elapsedTime = 0f;

			while (elapsedTime < duration)
			{
				elapsedTime += Time.deltaTime;
				float t = Mathf.Clamp01(elapsedTime / duration);
				obiRope.stretchingScale = Mathf.Lerp(startValue, endValue, t);
				yield return null;
			}

			obiRope.stretchingScale = endValue;
			stretchScaleRoutine = null;
		}
	}
}
