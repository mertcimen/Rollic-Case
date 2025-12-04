using _Main.Scripts.GamePlay;
using UnityEngine;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		public virtual void Load()
		{
			gameObject.SetActive(true);
			// TimeManager.Instance.Initialize(46);
		}

		public virtual void Play()
		{
		}
	}
}