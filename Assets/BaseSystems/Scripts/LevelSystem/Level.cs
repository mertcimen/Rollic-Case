using System;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Datas;
using _Main.Scripts.GamePlay;
using _Main.Scripts.GridSystem;
using _Main.Scripts.ShredderSystem;
using BaseSystems.Scripts.Managers;
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

		private int blockCount;

		public virtual void Load()
		{
			gameObject.SetActive(true);
			gridArea.Initialize(levelData);
			blockSpawner.Initialize(this, levelData);
			shredderSpawner.Initialize(this, levelData.shredders);
			TimeManager.Instance.Initialize(levelData.levelTime);

			blockCount = levelData.placedBlocks.Count;
		}

		public virtual void Play()
		{
		}

		public void DecreaseBlockCount()
		{
			if (blockCount == 0)
			{
				return;
			}

			blockCount--;

			if (blockCount == 0)
			{
				LevelManager.Instance.Win();
			}
		}

		public void TriggerBlockSelected(Block block)
		{
			OnBlockSelected?.Invoke(block);
		}
	}
}