using System.Collections.Generic;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Container;
using _Main.Scripts.Datas;
using UnityEngine;

namespace _Main.Scripts
{
	public class LevelDataCreator : MonoBehaviour
	{
		public LevelData levelData;
		public BlockDatabase blockDatabase;
		public EditorMode mode;
		
		
		
		public ShredderData selectedShredder;
		public ColorType shredderColor;
		public Axis shredderAxis;
		public int shredderRotation;
		public Size shredderSize = Size._1;

		
		public Block selectedBlockPrefab;
		public int selectedRotation;
		public ColorType selectedColor;
		public MoveType selectedMoveType;
		public PlacedBlockData selectedPlacedBlock;
		public enum EditorMode
		{
			Grid,
			Block,
			Shredder
		}
	}
}