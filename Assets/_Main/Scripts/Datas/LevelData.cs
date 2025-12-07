using System;
using System.Collections.Generic;
using _Main.Scripts.Container;
using UnityEngine;

namespace _Main.Scripts.Datas
{
	[CreateAssetMenu(menuName = "Level/LevelData")]
	public class LevelData : ScriptableObject
	{
		public int levelTime;
		public Vector2Int gridSize = new Vector2Int(6, 4);
		public List<GridCellData> cells = new List<GridCellData>();
		public List<PlacedBlockData> placedBlocks = new List<PlacedBlockData>();
		public List<ShredderData> shredders = new List<ShredderData>();
	}

	[Serializable]
	public class GridCellData
	{
		public Vector2Int coord;
		public bool enabled;

		public int occupiedBlockId = -1;

		// Shredder flags
		public bool hasShredder = false;
		public int shredderId = -1;
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
	public class ShredderData
	{
		public int id;

		public ColorType colorType;
		public Vector2Int pivotGrid;
		public Axis axis;
		public int rotation;

		public Size size = Size._1; // Enum

		public List<Vector2Int> occupiedCells = new List<Vector2Int>();
	}
}