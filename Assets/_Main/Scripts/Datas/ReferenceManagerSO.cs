using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.BallSystem;
using _Main.Scripts.Datas;
using _Main.Scripts.GridSystem;
using _Main.Scripts.RopeSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ReferenceManagerSO", menuName = "Data/Reference ManagerSO")]
public class ReferenceManagerSO : ScriptableObject
{
	#region Singleton

	private static ReferenceManagerSO _instance;

	public static ReferenceManagerSO Instance
	{
		get
		{
			if (_instance == null)
				_instance = Resources.Load<ReferenceManagerSO>("ReferenceManagerSO");
			return _instance;
		}
	}

	#endregion

	[SerializeField] private GridCell gridCellPrefab;
	[SerializeField] private BallController ballControllerPrefab;
	[SerializeField] private RopeLaneController ropeLanePrefab;
	[SerializeField] private MaterialDataSO ballMaterialData;

	public GridCell GridCellPrefab => gridCellPrefab;
	public BallController BallControllerPrefab => ballControllerPrefab;
	public RopeLaneController RopeLanePrefab => ropeLanePrefab;
	public MaterialDataSO BallMaterialData => ballMaterialData;
}