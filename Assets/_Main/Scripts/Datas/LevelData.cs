using System.Collections.Generic;
using _Main.Scripts.Container;
using UnityEngine;

namespace _Main.Scripts.Datas
{
	[CreateAssetMenu(fileName = "LevelData", menuName = "LevelDataSO", order = 0)]
	public class LevelData : ScriptableObject
	{
		public Vector2Int gridSize = new Vector2Int(6,4);
		public List<GridCellData> cells = new List<GridCellData>();
		public List<BlockPlacementData> blocks = new List<BlockPlacementData>();
		
		
		
	}
	
	
	
	
	[System.Serializable]
	public struct BlockPlacementData
	{
		public string prefabPath; 
		public Vector2Int pivotGrid; // pivot cell on grid
		public int rotation; // 0,90,180,270
		public ColorType colorType;
		public MoveType moveDirection;
	}
	
	
	[System.Serializable]
	public struct GridCellData
	{
		public Vector2Int coord;
		public bool enabled;
	}
}