using TMPro;
using UnityEngine;

namespace _Main.Scripts.RopeSystem
{
	public class RopeLaneCapacityTextController : MonoBehaviour
	{
		private const string DefaultSeparator = "/";

		[SerializeField] private TMP_Text capacityText;
		[SerializeField] private string separator = DefaultSeparator;
		[SerializeField] private bool findInChildrenIfMissing = true;

		public void UpdateCapacityText(int currentCount, int maxCapacity)
		{
			ResolveTextIfNeeded();
			if (capacityText == null)
				return;

			int clampedMaxCapacity = Mathf.Max(1, maxCapacity);
			int clampedCurrentCount = Mathf.Clamp(currentCount, 0, clampedMaxCapacity);
			capacityText.text = $"{clampedCurrentCount}{separator}{clampedMaxCapacity}";
		}

		private void ResolveTextIfNeeded()
		{
			if (capacityText != null)
				return;

			if (!findInChildrenIfMissing)
				return;

			capacityText = GetComponentInChildren<TMP_Text>(true);
		}
	}
}
