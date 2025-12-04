using _Main.Scripts.Analytics;
using _Main.Scripts.Datas;
using _Main.Scripts.Manager;
using Cysharp.Threading.Tasks;
using Fiber.Utilities;
using TriInspector;
using UnityEngine;

namespace Fiber.Managers
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

		private void Start()
		{
			
		}

		private void OnApplicationFocus(bool hasFocus)
		{
		}

		private void OnApplicationQuit()
		{
		}
	}
}