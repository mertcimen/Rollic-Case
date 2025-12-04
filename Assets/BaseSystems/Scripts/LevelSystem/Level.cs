using System;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.GridSystem;
using UnityEngine;

namespace BaseSystems.Scripts.LevelSystem
{
	public class Level : MonoBehaviour
	{
		public GridArea gridArea;

		public event Action<Block> OnBlockSelected;

		public virtual void Load()
		{
			gameObject.SetActive(true);
			// TimeManager.Instance.Initialize(46);
		}

		public virtual void Play()
		{
		}

		public void TriggerBlockSelected(Block block)
		{
			OnBlockSelected?.Invoke(block);
		}
		
		
	
		
	}
}