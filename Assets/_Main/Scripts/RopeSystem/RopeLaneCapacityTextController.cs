using TMPro;
using DG.Tweening;
using UnityEngine;

namespace _Main.Scripts.RopeSystem
{
	public class RopeLaneCapacityTextController : MonoBehaviour
	{
		private const string DefaultSeparator = "/";
		private const int WarningRemainingSlotCount = 1;
		private const float DefaultWarningFlashDuration = 0.08f;

		[SerializeField] private TMP_Text capacityText;
		[SerializeField] private GameObject capacityTextRootObject;
		[SerializeField] private string separator = DefaultSeparator;
		[SerializeField] private bool findInChildrenIfMissing = true;
		[SerializeField] private Color warningColor = Color.red;
		[SerializeField] private float warningFlashDuration = DefaultWarningFlashDuration;

		private int lastRemainingCapacity = int.MinValue;
		private bool hasCachedDefaultColor;
		private Color defaultTextColor = Color.white;
		private Tween warningTween;

		public void UpdateCapacityText(int currentCount, int maxCapacity)
		{
			ResolveTextIfNeeded();
			if (capacityText == null)
				return;

			CacheDefaultColorIfNeeded();

			int clampedMaxCapacity = Mathf.Max(1, maxCapacity);
			int clampedCurrentCount = Mathf.Clamp(currentCount, 0, clampedMaxCapacity);
			capacityText.text = $"{clampedCurrentCount}{separator}{clampedMaxCapacity}";

			int remainingCapacity = clampedMaxCapacity - clampedCurrentCount;
			if (remainingCapacity == WarningRemainingSlotCount && lastRemainingCapacity != WarningRemainingSlotCount)
				PlayLastSlotWarning();

			lastRemainingCapacity = remainingCapacity;
		}

		public void SetTextVisibility(bool isVisible)
		{
			ResolveTextRootObjectIfNeeded();
			if (capacityTextRootObject == null)
				return;

			if (capacityTextRootObject.activeSelf == isVisible)
				return;

			capacityTextRootObject.SetActive(isVisible);
		}

		private void ResolveTextIfNeeded()
		{
			if (capacityText != null)
				return;

			if (!findInChildrenIfMissing)
				return;

			capacityText = GetComponentInChildren<TMP_Text>(true);
		}

		private void ResolveTextRootObjectIfNeeded()
		{
			if (capacityTextRootObject != null)
				return;

			ResolveTextIfNeeded();
			if (capacityText != null)
				capacityTextRootObject = capacityText.gameObject;
		}

		private void CacheDefaultColorIfNeeded()
		{
			if (hasCachedDefaultColor || capacityText == null)
				return;

			defaultTextColor = capacityText.color;
			hasCachedDefaultColor = true;
		}

		private void PlayLastSlotWarning()
		{
			if (capacityText == null)
				return;

			warningTween?.Kill();
			capacityText.color = defaultTextColor;

			float duration = Mathf.Max(0.01f, warningFlashDuration);
			warningTween = capacityText.DOColor(warningColor, duration).SetEase(Ease.OutQuad)
				.SetLoops(2, LoopType.Yoyo).OnComplete(() =>
				{
					capacityText.color = defaultTextColor;
					warningTween = null;
				});
		}

		private void OnDisable()
		{
			warningTween?.Kill();
			warningTween = null;

			if (capacityText != null && hasCachedDefaultColor)
				capacityText.color = defaultTextColor;
		}
	}
}
