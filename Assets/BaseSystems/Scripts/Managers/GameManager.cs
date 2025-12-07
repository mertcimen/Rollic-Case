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
			
 #if !UNITY_EDITOR
			if(ReferenceManager.Instance == null)
				await new WaitUntil(()=>ReferenceManager.Instance != null);
			ReferenceManager.Instance.LoadingPanelController.gameObject.SetActive(true);
#endif
		}

	}
}