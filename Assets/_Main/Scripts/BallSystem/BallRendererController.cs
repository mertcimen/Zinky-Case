using System.Linq;
using _Main.Scripts.Containers;
using DG.Tweening;
using UnityEngine;

namespace _Main.Scripts.BallSystem
{
	public class BallRendererController : MonoBehaviour
	{
		[SerializeField] private Renderer renderer;

		private static readonly int FoamWidthId = Shader.PropertyToID("_FoamWidth");
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
		
		
		public Tween DoFoamWidth(float targetValue, float duration)
		{
			Material material = renderer.material;

			return DOTween.To(
				() => material.GetFloat(FoamWidthId),
				value => material.SetFloat(FoamWidthId, value),
				targetValue,
				duration
			);
		}
		
	}
}