using BaseSystems.Scripts.Utilities.Singletons;
using UnityEngine;

namespace BaseSystems.Scripts.Managers
{
	[DefaultExecutionOrder(-1)]
	public class GameManager : SingletonInit<GameManager>
	{
		
		protected override async void Awake()
		{
			base.Awake();
			Application.targetFrameRate = 60;
			Debug.unityLogger.logEnabled = Debug.isDebugBuild;
			

		}

	}
}