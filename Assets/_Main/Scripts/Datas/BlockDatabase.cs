using System.Collections.Generic;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Container;
using UnityEngine;

namespace _Main.Scripts.Datas
{
	[CreateAssetMenu(menuName = "Game/BlockDatabase")]
	public class BlockDatabase : ScriptableObject
	{
		public List<Block> blocks;

		public Block GetBlockPrefab(BlockType type)
		{
			return blocks.Find(b => b.BlockType == type);
		}
	}
}