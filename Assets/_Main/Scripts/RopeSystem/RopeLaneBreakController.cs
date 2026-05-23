using System.Collections;
using Obi;
using UnityEngine;

namespace _Main.Scripts.RopeSystem
{
	public class RopeLaneBreakController : MonoBehaviour
	{
		private const float DefaultBrokenThicknessFadeStartDelay = 0.7f;
		private const float DefaultBrokenThicknessFadeDuration = 0.25f;

		[SerializeField] private ObiRope obiRope;
		[SerializeField] private ObiRopeExtrudedRenderer ropeExtrudedRenderer;
		[SerializeField] private float brokenThicknessFadeStartDelay = DefaultBrokenThicknessFadeStartDelay;
		[SerializeField] private float brokenThicknessFadeDuration = DefaultBrokenThicknessFadeDuration;

		private Coroutine brokenThicknessFadeRoutine;
		private bool hasCachedCollisionFlags;
		private bool hasCachedInitialThicknessScale;
		private bool initialSurfaceCollisions;
		private bool initialSelfCollisions;
		private float initialRopeThicknessScale;

		public void Initialize(ObiRope rope)
		{
			if (rope != null)
				obiRope = rope;

			ResolveRopeIfNeeded();
			if (obiRope != null)
				obiRope.enabled = true;
			ResolveRopeRendererIfNeeded();
			CacheRopeCollisionFlagsIfNeeded();
			RestoreRopeCollisionFlags();
			RestoreRopeRendererThickness();
			StopEffects();
		}

		public void HandleRopeBreak(int ropeIndex)
		{
			ResolveRopeIfNeeded();
			ResolveRopeRendererIfNeeded();
			TryCutRopeAtMiddle(ropeIndex);
			DisableRopeSurfaceCollisions();
			AnimateBrokenRopeThicknessToZero();
		}

		public void StopEffects()
		{
			if (brokenThicknessFadeRoutine == null)
				return;

			StopCoroutine(brokenThicknessFadeRoutine);
			brokenThicknessFadeRoutine = null;
		}

		private void OnDisable()
		{
			StopEffects();
		}

		private void ResolveRopeIfNeeded()
		{
			if (obiRope == null)
				obiRope = GetComponentInChildren<ObiRope>();
		}

		private void ResolveRopeRendererIfNeeded()
		{
			if (ropeExtrudedRenderer == null)
				ropeExtrudedRenderer = GetComponent<ObiRopeExtrudedRenderer>();

			if (ropeExtrudedRenderer == null && obiRope != null)
				ropeExtrudedRenderer = obiRope.GetComponent<ObiRopeExtrudedRenderer>();

			if (ropeExtrudedRenderer == null || hasCachedInitialThicknessScale)
				return;

			initialRopeThicknessScale = ropeExtrudedRenderer.thicknessScale;
			hasCachedInitialThicknessScale = true;
		}

		private void CacheRopeCollisionFlagsIfNeeded()
		{
			if (obiRope == null || hasCachedCollisionFlags)
				return;

			initialSurfaceCollisions = obiRope.surfaceCollisions;
			initialSelfCollisions = obiRope.selfCollisions;
			hasCachedCollisionFlags = true;
		}

		private void RestoreRopeCollisionFlags()
		{
			if (obiRope == null || !hasCachedCollisionFlags)
				return;

			obiRope.surfaceCollisions = initialSurfaceCollisions;
			obiRope.selfCollisions = initialSelfCollisions;
		}

		private void RestoreRopeRendererThickness()
		{
			if (ropeExtrudedRenderer == null || !hasCachedInitialThicknessScale)
				return;

			SetRopeRendererThickness(initialRopeThicknessScale);
		}

		private void TryCutRopeAtMiddle(int ropeIndex)
		{
			if (obiRope == null)
				return;

			if (obiRope.elements == null || obiRope.elements.Count == 0)
				return;

			int centerIndex = obiRope.elements.Count / 2;
			bool torn = TryTearAtOrNearIndex(centerIndex);
			if (!torn)
			{
				Debug.LogWarning($"[{nameof(RopeLaneBreakController)}] Rope tear failed at lane {ropeIndex}. " +
				                 "Check rope blueprint spare particles / particle groups.");
				return;
			}

			obiRope.RebuildConstraintsFromElements();
		}

		private bool TryTearAtOrNearIndex(int centerIndex)
		{
			int elementCount = obiRope.elements.Count;
			for (int offset = 0; offset < elementCount; offset++)
			{
				int rightIndex = centerIndex + offset;
				if (rightIndex < elementCount && TryTearAtIndex(rightIndex))
					return true;

				int leftIndex = centerIndex - offset;
				if (leftIndex >= 0 && leftIndex != rightIndex && TryTearAtIndex(leftIndex))
					return true;
			}

			return false;
		}

		private bool TryTearAtIndex(int elementIndex)
		{
			if (elementIndex < 0 || elementIndex >= obiRope.elements.Count)
				return false;

			var element = obiRope.elements[elementIndex];
			return element != null && obiRope.Tear(element);
		}

		private void DisableRopeSurfaceCollisions()
		{
			if (obiRope == null)
				return;

			obiRope.surfaceCollisions = false;
			obiRope.selfCollisions = false;
		}

		private void AnimateBrokenRopeThicknessToZero()
		{
			if (ropeExtrudedRenderer == null)
			{
				DisableRopeComponent();
				return;
			}

			StopEffects();
			brokenThicknessFadeRoutine = StartCoroutine(FadeBrokenRopeThicknessToZero());
		}

		private IEnumerator FadeBrokenRopeThicknessToZero()
		{
			if (brokenThicknessFadeStartDelay > 0f)
				yield return new WaitForSeconds(brokenThicknessFadeStartDelay);

			if (ropeExtrudedRenderer == null)
			{
				DisableRopeComponent();
				brokenThicknessFadeRoutine = null;
				yield break;
			}

			float startThickness = ropeExtrudedRenderer.thicknessScale;
			if (brokenThicknessFadeDuration <= 0f)
			{
				SetRopeRendererThickness(0f);
				DisableRopeComponent();
				brokenThicknessFadeRoutine = null;
				yield break;
			}

			float elapsedTime = 0f;
			while (elapsedTime < brokenThicknessFadeDuration)
			{
				elapsedTime += Time.deltaTime;
				float t = Mathf.Clamp01(elapsedTime / brokenThicknessFadeDuration);
				SetRopeRendererThickness(Mathf.Lerp(startThickness, 0f, t));
				yield return null;
			}

			SetRopeRendererThickness(0f);
			DisableRopeComponent();
			brokenThicknessFadeRoutine = null;
		}

		private void SetRopeRendererThickness(float thickness)
		{
			if (ropeExtrudedRenderer == null)
				return;

			ropeExtrudedRenderer.thicknessScale = Mathf.Max(0f, thickness);
			((ObiActorRenderer<ObiRopeExtrudedRenderer>)ropeExtrudedRenderer).SetRendererDirty(Oni.RenderingSystemType.AllSmoothedRopes);
		}

		private void DisableRopeComponent()
		{
			if (obiRope == null)
				return;

			obiRope.enabled = false;
		}
	}
}
