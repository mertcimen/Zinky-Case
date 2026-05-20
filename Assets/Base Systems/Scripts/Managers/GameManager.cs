using _Main.Scripts.Analytics;
using Base_Systems.Scripts.Utilities.Singletons;
using Fiber.Utilities;
using UnityEngine;

namespace Base_Systems.Scripts.Managers
{
	[DefaultExecutionOrder(-1)]
	public class GameManager : SingletonInit<GameManager>
	{
		
		protected override async void Awake()
		{
			base.Awake();
			Application.targetFrameRate = 60;
			Debug.unityLogger.logEnabled = Debug.isDebugBuild;
			
			// await FiberAmplitude.Instance.Init();
 #if !UNITY_EDITOR
			if(ReferenceManager.Instance == null)
				await new WaitUntil(()=>ReferenceManager.Instance != null);
			ReferenceManager.Instance.LoadingPanelController.gameObject.SetActive(true);
#endif
		}

		private void Start()
		{
			AnalyticsManager.Instance.StartSession();
			
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (hasFocus) AnalyticsManager.Instance.StartSession();
			else AnalyticsManager.Instance.EndSession(AnalyticsReferences.EGameEndState.Pause);
		}

		private void OnApplicationQuit()
		{
			AnalyticsManager.Instance.EndSession(AnalyticsReferences.EGameEndState.Quit);
		}
	}
}