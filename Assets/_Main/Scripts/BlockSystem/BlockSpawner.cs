using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.Datas;
using BaseSystems.Scripts.LevelSystem;
using UnityEngine;

namespace _Main.Scripts.BlockSystem
{
	public class BlockSpawner : MonoBehaviour
	{
		private List<Block> blocks = new List<Block>();

		public void Initialize(Level level, LevelData levelData)
		{
			foreach (var blockData in levelData.placedBlocks)
			{
				var prefab = ReferenceManagerSO.Instance.blocks.FirstOrDefault(x => x.BlockType == blockData.type);
				if (prefab)
				{
					var _block = Instantiate(prefab);
					var pivotGrid = blockData.pivotCoord;
					var targetTile = level.gridArea.GridPoints[pivotGrid.x, pivotGrid.y];
					_block.transform.position = targetTile.transform.position;
					_block.transform.localEulerAngles = new Vector3(0, blockData.rotation, 0);
					_block.Setup(blockData.color, blockData.moveType);
					_block.transform.SetParent(transform);
					blocks.Add(_block);
					_block.gameObject.SetActive(false);
				}
			}

			
		}

		private void Start()
		{
			foreach (var block in blocks)
			{
				block.gameObject.SetActive(true);
			}
		}
	}
}