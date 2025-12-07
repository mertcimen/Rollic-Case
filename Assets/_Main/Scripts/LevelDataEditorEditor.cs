using System.Collections.Generic;
using _Main.Scripts;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Container;
using _Main.Scripts.Datas;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelDataCreator))]
public class LevelDataEditorEditor : Editor
{
	private const int CellSize = 24;

	public override void OnInspectorGUI()
	{
		LevelDataCreator creator = (LevelDataCreator)target;

		// Default inspector (LevelData reference)
		DrawDefaultInspector();

		if (creator.levelData == null)
		{
			EditorGUILayout.HelpBox("Not found Assigned LevelData", MessageType.Info);

			if (GUILayout.Button("➕ Create New LevelData"))
				CreateNewLevelData(creator);

			return;
		}

		LevelData data = creator.levelData;

		GUILayout.Space(10);

		EditorGUI.BeginChangeCheck();
		Vector2Int newSize = EditorGUILayout.Vector2IntField("Grid Size", data.gridSize);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(data, "Grid Size Change");
			data.gridSize = newSize;
			RegenerateGrid(data);
			EditorUtility.SetDirty(data);
			data.placedBlocks.Clear();
		}

		GUILayout.Space(8);
		EditorGUILayout.LabelField("GRID EDITOR", EditorStyles.boldLabel);

		// MODE SWITCH
		EditorGUILayout.Space(10);
		EditorGUILayout.LabelField("Editor Mode", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Toggle(creator.mode == LevelDataCreator.EditorMode.Grid, "Grid Edit", "Button"))
		{
			creator.mode = LevelDataCreator.EditorMode.Grid;
		}

		if (GUILayout.Toggle(creator.mode == LevelDataCreator.EditorMode.Block, "Block Place", "Button"))
		{
			creator.mode = LevelDataCreator.EditorMode.Block;
		}

		if (GUILayout.Toggle(creator.mode == LevelDataCreator.EditorMode.Shredder, "Shredder Place", "Button"))
		{
			creator.mode = LevelDataCreator.EditorMode.Shredder;
		}

		EditorGUILayout.EndHorizontal();

		EditorUtility.SetDirty(creator);

// BLOCK MODE OPTIONS
		if (creator.mode == LevelDataCreator.EditorMode.Block)
		{
			if (creator.blockDatabase == null || creator.blockDatabase.blocks == null ||
			    creator.blockDatabase.blocks.Count == 0)
			{
				EditorGUILayout.HelpBox("There is no assigned BlockData Block list Null", MessageType.Warning);
			}
			else
			{
				// --- BlockType list For DropDown ---
				List<Block> blocks = creator.blockDatabase.blocks;
				string[] names = new string[blocks.Count];
				int[] values = new int[blocks.Count];

				for (int i = 0; i < blocks.Count; i++)
				{
					names[i] = blocks[i].blockType.ToString();
					values[i] = (int)blocks[i].blockType;
				}

				int currentValue;
				if (creator.selectedBlockPrefab != null)
				{
					int idx = blocks.FindIndex(b => b == creator.selectedBlockPrefab);
					currentValue = idx >= 0 ? values[idx] : values[0];
				}
				else
				{
					currentValue = values[0];
				}

				int selectedValue = EditorGUILayout.IntPopup("Block Type", currentValue, names, values);

				// find index by value
				int selectedIndex = System.Array.IndexOf(values, selectedValue);
				if (selectedIndex < 0) selectedIndex = 0; // safety

				// change prefab if Choice changed
				//  null-check
				if (blocks.Count > 0 && creator.selectedBlockPrefab != blocks[selectedIndex])
				{
					creator.selectedBlockPrefab = blocks[selectedIndex];
					EditorUtility.SetDirty(creator);
				}
			}

			// ROTATION, COLOR, MOVE 
			creator.selectedRotation = EditorGUILayout.IntPopup("Rotation", creator.selectedRotation,
				new[] { "0", "90", "180", "270" }, new[] { 0, 90, 180, 270 });

			creator.selectedColor = (ColorType)EditorGUILayout.EnumPopup("Color", creator.selectedColor);
			creator.selectedMoveType = (MoveType)EditorGUILayout.EnumPopup("Move Type", creator.selectedMoveType);
		}

		if (creator.mode == LevelDataCreator.EditorMode.Shredder)
		{
			creator.shredderColor = (ColorType)EditorGUILayout.EnumPopup("Shredder Color", creator.shredderColor);
			creator.shredderSize = (Size)EditorGUILayout.EnumPopup("Size", creator.shredderSize);
			creator.shredderAxis = (Axis)EditorGUILayout.EnumPopup("Axis", creator.shredderAxis);
			creator.shredderRotation = EditorGUILayout.IntPopup("Rotation", creator.shredderRotation,
				new[] { "0", "90", "180", "270" }, new[] { 0, 90, 180, 270 });
		}

		DrawGrid(data);
		if (creator.selectedPlacedBlock != null)
		{
			EditorGUILayout.Space(20);
			EditorGUILayout.LabelField("Selected Block", EditorStyles.boldLabel);

			var b = creator.selectedPlacedBlock;

			b.color = (ColorType)EditorGUILayout.EnumPopup("Color", b.color);
			b.moveType = (MoveType)EditorGUILayout.EnumPopup("Move", b.moveType);

			if (GUILayout.Button("Delete Block") && creator.selectedPlacedBlock != null)
			{
				DeleteBlock(data, b);
				creator.selectedPlacedBlock = null;
			}

			EditorUtility.SetDirty(data);
		}

		if (creator.selectedShredder != null)
		{
			EditorGUILayout.Space(20);
			EditorGUILayout.LabelField("Selected Shredder", EditorStyles.boldLabel);

			var s = creator.selectedShredder;

			s.colorType = (ColorType)EditorGUILayout.EnumPopup("Color", s.colorType);
			s.axis = (Axis)EditorGUILayout.EnumPopup("Axis", s.axis);
			s.rotation = EditorGUILayout.IntField("Rotation", s.rotation);

			EditorGUILayout.LabelField("Occupied Cells:");
			foreach (var c in s.occupiedCells)
				EditorGUILayout.LabelField($"({c.x}, {c.y})");

			if (GUILayout.Button("Delete Shredder"))
			{
				DeleteShredder(data, s);
				creator.selectedShredder = null;
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(data);
			EditorUtility.SetDirty(creator);
		}
	}

	private void DeleteShredder(LevelData data, ShredderData sh)
	{
		foreach (var c in sh.occupiedCells)
		{
			int idx = CoordToIndex(data, c);
			var cell = data.cells[idx];

			if (cell.shredderId == sh.id)
			{
				cell.hasShredder = false;
				cell.shredderId = -1;
			}

			data.cells[idx] = cell;
		}

		data.shredders.Remove(sh);

		EditorUtility.SetDirty(data);
	}

	private void CreateNewLevelData(LevelDataCreator creator)
	{
		string path = EditorUtility.SaveFilePanelInProject("Create New LevelData", "LevelData.asset", "asset",
			"Name For New LevelData.");

		if (string.IsNullOrEmpty(path)) return;

		LevelData newData = ScriptableObject.CreateInstance<LevelData>();
		AssetDatabase.CreateAsset(newData, path);
		AssetDatabase.SaveAssets();

		creator.levelData = newData;
		EditorUtility.SetDirty(creator);
	}

	private void RegenerateGrid(LevelData data)
	{
		int total = data.gridSize.x * data.gridSize.y;
		data.cells = new System.Collections.Generic.List<GridCellData>(total);

		for (int y = 0; y < data.gridSize.y; y++)
		{
			for (int x = 0; x < data.gridSize.x; x++)
			{
				data.cells.Add(new GridCellData { coord = new Vector2Int(x, y), enabled = true });
			}
		}

		EditorUtility.SetDirty(data);
	}

	private void TryPlaceBlock(LevelDataCreator creator, LevelData data, Vector2Int pivot)
	{
		Block block = creator.selectedBlockPrefab;
		if (block == null) return;

		List<Vector2Int> occupied = new List<Vector2Int>();

		foreach (var unit in block.unitBlocks)
		{
			Vector2Int rotated = Rotate(unit.innerCoordinate, creator.selectedRotation);
			Vector2Int world = pivot + rotated;

			if (!IsInside(data.gridSize, world))
			{
				Debug.LogWarning("Placement out of bounds");
				return;
			}

			int idx = CoordToIndex(data, world);

			if (!data.cells[idx].enabled)
			{
				Debug.LogWarning("Placement blocked: Target grid cell is disabled.");
				return;
			}

			var cell = data.cells[idx];

			if (!cell.enabled)
			{
				Debug.LogWarning("Grid disabled");
				return;
			}

			if (data.cells[idx].occupiedBlockId != -1)
			{
				Debug.LogWarning("Placement overlaps another block");
				return;
			}

			occupied.Add(world);
		}

		PlacedBlockData p = new PlacedBlockData
		{
			id = GetNextPlacedBlockId(data),
			type = block.blockType,
			pivotCoord = pivot,
			rotation = creator.selectedRotation,
			color = creator.selectedColor,
			moveType = creator.selectedMoveType,
			occupiedCells = new List<Vector2Int>(occupied)
		};

		data.placedBlocks.Add(p);

		foreach (var c in occupied)
		{
			int idx = CoordToIndex(data, c);
			var cell = data.cells[idx];
			cell.occupiedBlockId = p.id;
			data.cells[idx] = cell;
		}

		EditorUtility.SetDirty(data);
	}

	private void TryPlaceShredder(LevelDataCreator creator, LevelData data, Vector2Int pivot)
	{
		Size sizeEnum = creator.shredderSize;
		int length = (int)sizeEnum; // ENUM → INTEGER

		Axis axis = creator.shredderAxis;

		List<Vector2Int> occ = new List<Vector2Int>();

		for (int i = 0; i < length; i++)
		{
			Vector2Int cellPos = pivot;

			if (axis == Axis.X)
				cellPos += new Vector2Int(i, 0);
			else
				cellPos += new Vector2Int(0, i);

			if (!IsInside(data.gridSize, cellPos))
			{
				Debug.LogWarning("Shredder out of bounds.");
				return;
			}

			GridCellData cell = data.cells[CoordToIndex(data, cellPos)];

			if (cell.hasShredder)
			{
				Debug.LogWarning("Cell already contains a shredder.");
				return;
			}

			occ.Add(cellPos);
		}

		ShredderData sh = new ShredderData
		{
			id = GetNextShredderId(data),
			pivotGrid = pivot,
			colorType = creator.shredderColor,
			axis = creator.shredderAxis,
			size = sizeEnum,
			rotation = creator.shredderRotation,
			occupiedCells = occ
		};

		data.shredders.Add(sh);

		foreach (var c in occ)
		{
			int idx = CoordToIndex(data, c);
			var cell = data.cells[idx];

			cell.hasShredder = true;
			cell.shredderId = sh.id;

			data.cells[idx] = cell;
		}

		EditorUtility.SetDirty(data);
	}

	private int GetNextShredderId(LevelData data)
	{
		int max = -1;
		foreach (var s in data.shredders)
			if (s.id > max)
				max = s.id;
		return max + 1;
	}

	private void DeleteBlock(LevelData data, PlacedBlockData block)
	{
		if (block == null) return;

		foreach (var c in block.occupiedCells)
		{
			if (!IsInside(data.gridSize, c)) continue;
			int idx = CoordToIndex(data, c);
			var cell = data.cells[idx];
			if (cell.occupiedBlockId == block.id)
			{
				cell.occupiedBlockId = -1;
				data.cells[idx] = cell;
			}
		}

		data.placedBlocks.RemoveAll(p => p.id == block.id);

		EditorUtility.SetDirty(data);
		EditorUtility.SetDirty(target);
		Repaint();
	}

	private bool IsInside(Vector2Int size, Vector2Int c)
	{
		return c.x >= 0 && c.y >= 0 && c.x < size.x && c.y < size.y;
	}

	private int CoordToIndex(LevelData data, Vector2Int c)
	{
		return c.y * data.gridSize.x + c.x;
	}

	private Vector2Int Rotate(Vector2Int v, int rot)
	{
		rot = ((rot % 360) + 360) % 360;
		switch (rot)
		{
			case 0: return v;
			case 90: return new Vector2Int(-v.y, v.x);
			case 180: return new Vector2Int(-v.x, -v.y);
			case 270: return new Vector2Int(v.y, -v.x);
			default: return v;
		}
	}

	private Color ColorFor(ColorType type)
	{
		switch (type)
		{
			case ColorType._1Blue: return Color.blue;
			case ColorType._2Green: return Color.green;
			case ColorType._3Orange:
				return new Color(1f, 0.55f, 0.15f);

			case ColorType._4Pink: return new Color(1f, 0.35f, 0.75f);
			case ColorType._5Purple: return new Color(0.55f, 0.25f, 0.75f);
			case ColorType._6Red: return Color.red;
			case ColorType._7Yellow: return Color.yellow;
			case ColorType._8Brown: return new Color(0.45f, 0.25f, 0.1f);
			case ColorType._9Turquoise: return new Color(0.1f, 0.85f, 0.85f);
			default: return Color.gray;
		}
	}

	private PlacedBlockData GetPlacedBlockById(LevelData data, int id)
	{
		if (id < 0) return null;
		return data.placedBlocks.Find(p => p.id == id);
	}

	private int GetNextPlacedBlockId(LevelData data)
	{
		int max = -1;
		for (int i = 0; i < data.placedBlocks.Count; i++)
			if (data.placedBlocks[i].id > max)
				max = data.placedBlocks[i].id;
		return max + 1;
	}

	private ShredderData FindShredder(LevelData data, Vector2Int coord)
	{
		return data.shredders.Find(s => s.occupiedCells.Contains(coord));
	}

	private void DrawGrid(LevelData data)
	{
		LevelDataCreator creator = (LevelDataCreator)target;
		int xCount = data.gridSize.x;
		int yCount = data.gridSize.y;

		for (int y = yCount - 1; y >= 0; y--)
		{
			EditorGUILayout.BeginHorizontal();

			for (int x = 0; x < xCount; x++)
			{
				int index = y * xCount + x;
				GridCellData cell = data.cells[index];

				Color old = GUI.backgroundColor;

				if (cell.hasShredder)
				{
					GUI.backgroundColor = new Color(1f, 0.2f, 0.6f); // pembe / magenta
				}
				else if (cell.occupiedBlockId != -1)
				{
					var pb = GetPlacedBlockById(data, cell.occupiedBlockId);
					GUI.backgroundColor = ColorFor(pb.color);
				}
				else if (!cell.enabled)
				{
					GUI.backgroundColor = Color.black;
				}
				else
				{
					GUI.backgroundColor = Color.white;
				}

				if (GUILayout.Button("", GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
				{
					if (creator.mode == LevelDataCreator.EditorMode.Grid)
					{
						cell.enabled = !cell.enabled;
						data.cells[index] = cell;
					}

					else if (creator.mode == LevelDataCreator.EditorMode.Block)
					{
						if (cell.occupiedBlockId != -1)
						{
							creator.selectedPlacedBlock = GetPlacedBlockById(data, cell.occupiedBlockId);
						}
						else
						{
							TryPlaceBlock(creator, data, cell.coord);
						}
					}

					else if (creator.mode == LevelDataCreator.EditorMode.Shredder)
					{
						if (cell.hasShredder)
						{
							// Bu hücre zaten shredder’a ait → shredder’ı seç
							creator.selectedShredder = FindShredder(data, cell.coord);
						}
						else
						{
							// Bu hücrede shredder yok → yeni shredder yerleştir
							TryPlaceShredder(creator, data, cell.coord);
						}
					}

					EditorUtility.SetDirty(data);
				}

				GUI.backgroundColor = old;
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}