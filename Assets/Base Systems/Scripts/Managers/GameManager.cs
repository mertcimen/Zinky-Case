using Base_Systems.Scripts.Utilities.Singletons;
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
			
 #if !UNITY_EDITOR
		
#endif
		}

		private void Start()
		{
			
		}

		
		
	}
}