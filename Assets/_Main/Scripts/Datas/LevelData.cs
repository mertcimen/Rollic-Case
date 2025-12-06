using System;
using System.Collections.Generic;
using _Main.Scripts.Container;
using UnityEngine;

namespace _Main.Scripts.Datas
{
	[Serializable]
	public class GridCellData
	{
		public Vector2Int coord;
		public bool enabled;
		public int occupiedBlockId = -1;
	}

	
	[System.Serializable]
	public class PlacedBlockData
	{
		public int id;

		public BlockType type;
		public Vector2Int pivotCoord;
		public int rotation;
		public ColorType color;
		public MoveType moveType;

		public List<Vector2Int> occupiedCells = new List<Vector2Int>();
	}


	[Serializable]
	public struct ShredderData
	{
		public ColorType colorType;
		public Vector2Int pivotGrid;
		public int rotation;
		
	}
	
	
	[CreateAssetMenu(menuName = "Level/LevelData")]
	public class LevelData : ScriptableObject
	{
		public Vector2Int gridSize = new Vector2Int(6,4);
		public List<GridCellData> cells = new List<GridCellData>();
		public List<PlacedBlockData> placedBlocks = new List<PlacedBlockData>();

		
	}
}