using System;
using System.Collections.Generic;
using _Main.Scripts.Containers;
using UnityEngine;

namespace _Main.Scripts.Datas
{
	[CreateAssetMenu(fileName = "MaterialData", menuName = "Data/MaterialData")]
	public class MaterialDataSO : ScriptableObject
	{
		public List<MaterialByColorType> materialDatas = new List<MaterialByColorType>();
	}

	[Serializable]
	public class MaterialByColorType
	{
		public ColorType colorType;
		public Material material;
	}
}