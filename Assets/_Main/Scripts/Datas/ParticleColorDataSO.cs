using System;
using System.Collections.Generic;
using _Main.Scripts.Containers;
using UnityEngine;

namespace _Main.Scripts.Datas
{
	[CreateAssetMenu(fileName = "ParticleColorData", menuName = "Data/Particle Color Data")]
	public class ParticleColorDataSO : ScriptableObject
	{
		[SerializeField] private List<ParticleColorByType> particleColors = new();

		public bool TryGetColor(ColorType colorType, out Color color)
		{
			for (int i = 0; i < particleColors.Count; i++)
			{
				ParticleColorByType entry = particleColors[i];
				if (entry.colorType != colorType)
					continue;

				color = entry.color;
				return true;
			}

			color = Color.white;
			return false;
		}
	}

	[Serializable]
	public class ParticleColorByType
	{
		public ColorType colorType;
		public Color color = Color.white;
	}
}
