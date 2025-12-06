using System.Collections;
using System.Linq;
using _Main.Scripts.Datas;
using BaseSystems.Scripts.LevelSystem;
using UnityEngine;

namespace _Main.Scripts.BlockSystem
{
	public class BlockSpawner : MonoBehaviour
	{
		public IEnumerator Initialize(Level level, LevelData levelData)
		{

			yield return null;
			yield return null;
			yield return null;
			foreach (var blockData in levelData.placedBlocks)
			{
				var prefab = ReferenceManagerSO.Instance.blocks.FirstOrDefault(x => x.blockType == blockData.type);
				if (prefab)
				{
					var _block = Instantiate(prefab);
					var pivotGrid = blockData.pivotCoord;
					var targetTile = level.gridArea.GridPoints[pivotGrid.x, pivotGrid.y];
					Debug.Log(targetTile.transform.position);
					_block.transform.position = targetTile.transform.position;
					_block.transform.localEulerAngles = new Vector3(0, blockData.rotation, 0);
					_block.Setup(blockData.color, blockData.moveType);
					_block.transform.SetParent(transform);
				}
			}
		}
	}
}