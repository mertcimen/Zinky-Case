using System.Linq;
using _Main.Scripts.Containers;
using UnityEngine;

namespace _Main.Scripts.BallSystem
{
	public class BallRendererController : MonoBehaviour
	{
		[SerializeField] private Renderer renderer;

		public void Initialize(ColorType colorType)
		{
			var targetMaterialData =
				ReferenceManagerSO.Instance.BallMaterialData.materialDatas.FirstOrDefault(x =>
					x.colorType == colorType);

			if (targetMaterialData != null)
			{
				renderer.material = targetMaterialData.material;
			}
		}
	}
}