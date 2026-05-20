using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.GridSystem;
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

	public GridCell GridCellPrefab => gridCellPrefab;
}