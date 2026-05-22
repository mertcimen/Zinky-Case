using System.Collections.Generic;
using _Main.Scripts.BallSystem;
using _Main.Scripts.Datas;
using _Main.Scripts.MergeSystem;
using Base_Systems.Scripts.Managers;
using UnityEngine;

namespace _Main.Scripts.RopeSystem
{
	public class RopeController : MonoBehaviour
	{
		private const float DEFAULT_ROPE_VERTICAL_OFFSET = 2f;
		private const string DEFAULT_FAIL_TEXT = "NO SPACE!!";
		private const string LEFT_ANCHOR_NAME = "LeftAnchor";
		private const string RIGHT_ANCHOR_NAME = "RightAnchor";

		[SerializeField] private List<RopeLaneController> ropeLanes = new();
		[SerializeField] private Vector3 firstRopeLocalPosition = Vector3.zero;
		[SerializeField] private float ropeVerticalOffset = DEFAULT_ROPE_VERTICAL_OFFSET;
		[SerializeField] private Vector3 leftAnchorOffset = new Vector3(-4.5f, 2f, 0f);
		[SerializeField] private Vector3 rightAnchorOffset = new Vector3(4.5f, 2f, 0f);

		private int activeRopeLaneIndex = -1;
		private bool hasFailTriggered;

		public IReadOnlyList<RopeLaneController> RopeLanes => ropeLanes;
		public RopeLaneController ActiveRopeLane => GetRopeLane(activeRopeLaneIndex);

		public void Initialize(LevelDataSO levelDataSO, MergeController mergeController)
		{
			hasFailTriggered = false;
			levelDataSO.EnsureRopeData();
			EnsureRopeLaneCount(levelDataSO.RopeCount);

			for (int i = 0; i < ropeLanes.Count; i++)
			{
				int ropeCapacity = levelDataSO.GetRopeCapacity(i);
				Vector3 ropeLocalPosition = firstRopeLocalPosition + Vector3.down * (i * ropeVerticalOffset);
				RopeLaneController ropeLaneController = ropeLanes[i];
				ropeLaneController.transform.localPosition = ropeLocalPosition;
				ropeLaneController.Initialize(i, ropeCapacity, mergeController, this);
				AssignLaneAnchors(ropeLaneController, i, ropeLocalPosition);
			}

			SetActiveRopeLane(0);
		}

		private void EnsureRopeLaneCount(int ropeCount)
		{
			while (ropeLanes.Count < ropeCount)
				ropeLanes.Add(CreateRopeLane(ropeLanes.Count));

			for (int i = ropeLanes.Count - 1; i >= ropeCount; i--)
			{
				Destroy(ropeLanes[i].gameObject);
				DestroyLaneAnchors(i);
				ropeLanes.RemoveAt(i);
			}

			if (activeRopeLaneIndex >= ropeLanes.Count)
				activeRopeLaneIndex = ropeLanes.Count - 1;
		}

		public bool SetActiveRopeLane(int laneIndex)
		{
			if (laneIndex < 0 || laneIndex >= ropeLanes.Count)
				return false;

			activeRopeLaneIndex = laneIndex;
			for (int i = 0; i < ropeLanes.Count; i++)
				ropeLanes[i].SetLaneActive(i == activeRopeLaneIndex);

			return true;
		}

		public void HandleLaneCapacityReached(RopeLaneController laneController, bool hasMergedAfterFull)
		{
			if (laneController == null || hasFailTriggered)
				return;

			if (laneController != ActiveRopeLane)
				return;

			if (!hasMergedAfterFull)
			{
				TriggerFail();
				return;
			}

			int nextLaneIndex = laneController.RopeIndex + 1;
			RopeLaneController nextLane = GetRopeLane(nextLaneIndex);
			if (nextLane == null)
			{
				TriggerFail();
				return;
			}

			List<BallController> remainingBalls = laneController.BreakLaneAndCollectRemainingBalls();
			SetActiveRopeLane(nextLaneIndex);
			for (int i = 0; i < remainingBalls.Count; i++)
			{
				var ballController = remainingBalls[i];
				if (ballController != null)
					ballController.ReleaseFromBrokenRope();
			}
		}

		private RopeLaneController CreateRopeLane(int ropeIndex)
		{
			RopeLaneController ropeLaneController = Instantiate(ReferenceManagerSO.Instance.RopeLanePrefab);
			ropeLaneController.transform.SetParent(transform, false);
			ropeLaneController.gameObject.name = $"Rope_{ropeIndex + 1}";
			return ropeLaneController;
		}

		private void AssignLaneAnchors(RopeLaneController ropeLaneController, int laneIndex, Vector3 ropeLocalPosition)
		{
			string anchorRootName = GetAnchorRootName(laneIndex);
			Transform anchorRoot = transform.Find(anchorRootName);
			if (anchorRoot == null)
			{
				anchorRoot = new GameObject(anchorRootName).transform;
				anchorRoot.SetParent(transform, false);
			}

			Transform leftAnchor = anchorRoot.Find(LEFT_ANCHOR_NAME);
			if (leftAnchor == null)
			{
				leftAnchor = new GameObject(LEFT_ANCHOR_NAME).transform;
				leftAnchor.SetParent(anchorRoot, false);
			}

			Transform rightAnchor = anchorRoot.Find(RIGHT_ANCHOR_NAME);
			if (rightAnchor == null)
			{
				rightAnchor = new GameObject(RIGHT_ANCHOR_NAME).transform;
				rightAnchor.SetParent(anchorRoot, false);
			}

			anchorRoot.localPosition = Vector3.zero;
			leftAnchor.localPosition = leftAnchorOffset;
			rightAnchor.localPosition = rightAnchorOffset;

			anchorRoot.localPosition = ropeLocalPosition;
			ropeLaneController.SetAttachmentTargets(leftAnchor, rightAnchor);
		}

		private void DestroyLaneAnchors(int laneIndex)
		{
			Transform anchorRoot = transform.Find(GetAnchorRootName(laneIndex));
			if (anchorRoot != null)
				Destroy(anchorRoot.gameObject);
		}

		private static string GetAnchorRootName(int laneIndex) => $"Rope_{laneIndex + 1}_Anchors";

		private RopeLaneController GetRopeLane(int laneIndex)
		{
			if (laneIndex < 0 || laneIndex >= ropeLanes.Count)
				return null;

			return ropeLanes[laneIndex];
		}

		private void TriggerFail()
		{
			if (hasFailTriggered)
				return;

			hasFailTriggered = true;
			LevelManager.Instance.Lose(DEFAULT_FAIL_TEXT);
		}
	}
}
