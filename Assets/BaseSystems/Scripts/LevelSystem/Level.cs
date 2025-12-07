using System;
using System.Linq;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Datas;
using _Main.Scripts.GridSystem;
using _Main.Scripts.ShredderSystem;
using UnityEngine;

namespace BaseSystems.Scripts.LevelSystem
{
	public class Level : MonoBehaviour
	{
		public LevelData levelData;
		public GridArea gridArea;
		public BlockSpawner blockSpawner;
		public ShredderSpawner shredderSpawner;
		public event Action<Block> OnBlockSelected;

		public virtual void Load()
		{
			gameObject.SetActive(true);
			gridArea.Initialize(levelData);
			blockSpawner.Initialize(this, levelData);
			shredderSpawner.Initialize(this, levelData.shredders);
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