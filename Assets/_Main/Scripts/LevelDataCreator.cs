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

		[HideInInspector] public ShredderData selectedShredder;
		[HideInInspector] public ColorType shredderColor;
		[HideInInspector] public Axis shredderAxis;
		[HideInInspector] public int shredderRotation;
		[HideInInspector] public Size shredderSize = Size._1;

		[HideInInspector] public Block selectedBlockPrefab;
		[HideInInspector] public int selectedRotation;
		[HideInInspector] public ColorType selectedColor;
		[HideInInspector] public MoveType selectedMoveType;
		[HideInInspector] public PlacedBlockData selectedPlacedBlock;

		public enum EditorMode
		{
			Grid,
			Block,
			Shredder
		}
	}
}