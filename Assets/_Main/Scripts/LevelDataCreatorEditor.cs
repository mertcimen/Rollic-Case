using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Container;
using _Main.Scripts.Datas;
using UnityEditor;
using UnityEngine;

namespace _Main.Scripts
{
	[CustomEditor(typeof(LevelDataCreator))]
	public class LevelDataCreatorEditor : Editor
	{
		private const int CellSize = 24;

		private LevelDataCreator Creator => (LevelDataCreator)target;

		public override void OnInspectorGUI()
		{
			var creator = Creator;

			// Draw default inspector (for fields on LevelDataCreator itself)
			DrawDefaultInspector();

			if (creator.levelData == null)
			{
				DrawNoLevelDataSection(creator);
				return;
			}

			var data = creator.levelData;

			DrawLevelMetaSection(data);
			DrawGridSizeSection(data);
			DrawModeSelection(creator);

			EditorUtility.SetDirty(creator);

			DrawModeSpecificOptions(creator, data);

			DrawGrid(data);

			DrawSelectedBlockPanel(creator, data);
			DrawSelectedShredderPanel(creator, data);

			if (GUI.changed)
			{
				EditorUtility.SetDirty(data);
				EditorUtility.SetDirty(creator);
			}
		}

		#region Top-Level Sections

		private void DrawNoLevelDataSection(LevelDataCreator creator)
		{
			EditorGUILayout.HelpBox("No LevelData assigned.", MessageType.Info);

			if (GUILayout.Button("Create New LevelData"))
			{
				CreateNewLevelData(creator);
			}
		}

		private void DrawLevelMetaSection(LevelData data)
		{
			EditorGUILayout.Space(10);

			EditorGUI.BeginChangeCheck();
			int newLevelTime = EditorGUILayout.IntField("Level Time", data.levelTime);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(data, "Level Time Change");
				data.levelTime = newLevelTime;
				EditorUtility.SetDirty(data);
			}
		}

		private void DrawGridSizeSection(LevelData data)
		{
			EditorGUILayout.Space(10);

			EditorGUI.BeginChangeCheck();
			Vector2Int newSize = EditorGUILayout.Vector2IntField("Grid Size", data.gridSize);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(data, "Grid Size Change");
				data.gridSize = newSize;

				RegenerateGrid(data);
				data.placedBlocks.Clear();

				EditorUtility.SetDirty(data);
			}

			EditorGUILayout.Space(8);
			EditorGUILayout.LabelField("GRID EDITOR", EditorStyles.boldLabel);
		}

		private void DrawModeSelection(LevelDataCreator creator)
		{
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
		}

		private void DrawModeSpecificOptions(LevelDataCreator creator, LevelData data)
		{
			switch (creator.mode)
			{
				case LevelDataCreator.EditorMode.Block:
					DrawBlockModeOptions(creator);
					break;

				case LevelDataCreator.EditorMode.Shredder:
					DrawShredderModeOptions(creator);
					break;
			}
		}

		#endregion

		#region Block Mode UI

		private void DrawBlockModeOptions(LevelDataCreator creator)
		{
			var db = creator.blockDatabase;

			if (db == null || db.blocks == null || db.blocks.Count == 0)
			{
				EditorGUILayout.HelpBox("No BlockDatabase or block list assigned.", MessageType.Warning);
				return;
			}

			DrawBlockTypeDropdown(creator, db.blocks);

			creator.selectedRotation = EditorGUILayout.IntPopup("Rotation", creator.selectedRotation,
				new[] { "0", "90", "180", "270" }, new[] { 0, 90, 180, 270 });

			creator.selectedColor = (ColorType)EditorGUILayout.EnumPopup("Color", creator.selectedColor);
			creator.selectedMoveType = (MoveType)EditorGUILayout.EnumPopup("Move Type", creator.selectedMoveType);
		}

		private void DrawBlockTypeDropdown(LevelDataCreator creator, List<Block> blocks)
		{
			// Build names and values from BlockType enum
			var names = new string[blocks.Count];
			var values = new int[blocks.Count];

			for (int i = 0; i < blocks.Count; i++)
			{
				names[i] = blocks[i].BlockType.ToString();
				values[i] = (int)blocks[i].BlockType;
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

			int selectedIndex = System.Array.IndexOf(values, selectedValue);
			if (selectedIndex < 0) selectedIndex = 0;

			if (blocks.Count > 0 && creator.selectedBlockPrefab != blocks[selectedIndex])
			{
				creator.selectedBlockPrefab = blocks[selectedIndex];
				EditorUtility.SetDirty(creator);
			}
		}

		#endregion

		#region Shredder Mode UI

		private void DrawShredderModeOptions(LevelDataCreator creator)
		{
			creator.shredderColor = (ColorType)EditorGUILayout.EnumPopup("Shredder Color", creator.shredderColor);
			creator.shredderSize = (Size)EditorGUILayout.EnumPopup("Size", creator.shredderSize);
			creator.shredderAxis = (Axis)EditorGUILayout.EnumPopup("Axis", creator.shredderAxis);

			creator.shredderRotation = EditorGUILayout.IntPopup("Rotation", creator.shredderRotation,
				new[] { "0", "90", "180", "270" }, new[] { 0, 90, 180, 270 });
		}

		#endregion

		#region Selected Block / Shredder Panels

		private void DrawSelectedBlockPanel(LevelDataCreator creator, LevelData data)
		{
			if (creator.selectedPlacedBlock == null)
				return;

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

		private void DrawSelectedShredderPanel(LevelDataCreator creator, LevelData data)
		{
			if (creator.selectedShredder == null)
				return;

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

			EditorUtility.SetDirty(data);
		}

		#endregion

		#region LevelData Creation / Grid

		private void CreateNewLevelData(LevelDataCreator creator)
		{
			string path = EditorUtility.SaveFilePanelInProject("Create New LevelData", "LevelData.asset", "asset",
				"Name for new LevelData.");

			if (string.IsNullOrEmpty(path)) return;

			LevelData newData = ScriptableObject.CreateInstance<LevelData>();
			AssetDatabase.CreateAsset(newData, path);

			RegenerateGrid(newData);
			AssetDatabase.SaveAssets();

			creator.levelData = newData;
			EditorUtility.SetDirty(creator);
		}

		private void RegenerateGrid(LevelData data)
		{
			int total = data.gridSize.x * data.gridSize.y;
			data.cells = new List<GridCellData>(total);

			for (int y = 0; y < data.gridSize.y; y++)
			{
				for (int x = 0; x < data.gridSize.x; x++)
				{
					data.cells.Add(new GridCellData { coord = new Vector2Int(x, y), enabled = true });
				}
			}

			EditorUtility.SetDirty(data);
		}

		#endregion

		#region Placement Logic

		private void TryPlaceBlock(LevelDataCreator creator, LevelData data, Vector2Int pivot)
		{
			Block block = creator.selectedBlockPrefab;
			if (block == null) return;

			var occupied = new List<Vector2Int>();

			foreach (var unit in block.unitBlocks)
			{
				Vector2Int rotated = Rotate(unit.innerCoordinate, creator.selectedRotation);
				Vector2Int world = pivot + rotated;

				if (!IsInside(data.gridSize, world))
				{
					Debug.LogWarning("Block placement out of bounds.");
					return;
				}

				int idx = CoordToIndex(data, world);
				var cell = data.cells[idx];

				if (!cell.enabled)
				{
					Debug.LogWarning("Block placement blocked: target grid cell is disabled.");
					return;
				}

				if (cell.occupiedBlockId != -1)
				{
					Debug.LogWarning("Block placement overlaps another block.");
					return;
				}

				occupied.Add(world);
			}

			var placed = new PlacedBlockData
			{
				id = GetNextPlacedBlockId(data),
				type = block.BlockType,
				pivotCoord = pivot,
				rotation = creator.selectedRotation,
				color = creator.selectedColor,
				moveType = creator.selectedMoveType,
				occupiedCells = new List<Vector2Int>(occupied)
			};

			data.placedBlocks.Add(placed);

			foreach (var c in occupied)
			{
				int idx = CoordToIndex(data, c);
				var cell = data.cells[idx];
				cell.occupiedBlockId = placed.id;
				data.cells[idx] = cell;
			}

			EditorUtility.SetDirty(data);
		}

		private void TryPlaceShredder(LevelDataCreator creator, LevelData data, Vector2Int pivot)
		{
			Size sizeEnum = creator.shredderSize;
			int length = (int)sizeEnum;
			Axis axis = creator.shredderAxis;

			var occupied = new List<Vector2Int>();

			for (int i = 0; i < length; i++)
			{
				Vector2Int cellPos = pivot;

				if (axis == Axis.X)
					cellPos += new Vector2Int(i, 0);
				else
					cellPos += new Vector2Int(0, i);

				if (!IsInside(data.gridSize, cellPos))
				{
					Debug.LogWarning("Shredder placement out of bounds.");
					return;
				}

				int idx = CoordToIndex(data, cellPos);
				GridCellData cell = data.cells[idx];

				if (cell.hasShredder)
				{
					Debug.LogWarning("Cell already contains a shredder.");
					return;
				}

				occupied.Add(cellPos);
			}

			var shredder = new ShredderData
			{
				id = GetNextShredderId(data),
				pivotGrid = pivot,
				colorType = creator.shredderColor,
				axis = creator.shredderAxis,
				size = sizeEnum,
				rotation = creator.shredderRotation,
				occupiedCells = occupied
			};

			data.shredders.Add(shredder);

			foreach (var c in occupied)
			{
				int idx = CoordToIndex(data, c);
				var cell = data.cells[idx];

				cell.hasShredder = true;
				cell.shredderId = shredder.id;

				data.cells[idx] = cell;
			}

			EditorUtility.SetDirty(data);
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

		#endregion

		#region Grid Drawing

		private void DrawGrid(LevelData data)
		{
			var creator = Creator;

			int xCount = data.gridSize.x;
			int yCount = data.gridSize.y;

			float offsetPixels = CellSize * 0.25f;

			for (int y = yCount - 1; y >= 0; y--)
			{
				EditorGUILayout.BeginHorizontal();

				for (int x = 0; x < xCount; x++)
				{
					int index = y * xCount + x;
					GridCellData cell = data.cells[index];

					Color oldBg = GUI.backgroundColor;
					GUI.backgroundColor = GetCellBackgroundColor(data, cell);

					if (GUILayout.Button(GUIContent.none, GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
					{
						HandleGridCellClick(creator, data, cell, index);
						EditorUtility.SetDirty(data);
					}

					GUI.backgroundColor = oldBg;

					if (cell.hasShredder)
					{
						DrawShredderOverlay(data, cell, offsetPixels);
					}
				}

				EditorGUILayout.EndHorizontal();
			}
		}

		private Color GetCellBackgroundColor(LevelData data, GridCellData cell)
		{
			if (cell.occupiedBlockId != -1)
			{
				var pb = GetPlacedBlockById(data, cell.occupiedBlockId);
				return pb != null ? ColorFor(pb.color) : Color.gray;
			}

			if (!cell.enabled)
				return Color.black;

			return Color.white;
		}

		private void HandleGridCellClick(LevelDataCreator creator, LevelData data, GridCellData cell, int index)
		{
			switch (creator.mode)
			{
				case LevelDataCreator.EditorMode.Grid:
					cell.enabled = !cell.enabled;
					data.cells[index] = cell;
					break;

				case LevelDataCreator.EditorMode.Block:
					if (cell.occupiedBlockId != -1)
					{
						creator.selectedPlacedBlock = GetPlacedBlockById(data, cell.occupiedBlockId);
					}
					else
					{
						TryPlaceBlock(creator, data, cell.coord);
					}

					break;

				case LevelDataCreator.EditorMode.Shredder:
					if (cell.hasShredder)
					{
						creator.selectedShredder = FindShredder(data, cell.coord);
					}
					else
					{
						TryPlaceShredder(creator, data, cell.coord);
					}

					break;
			}
		}

		private void DrawShredderOverlay(LevelData data, GridCellData cell, float offsetPixels)
		{
			Rect rect = GUILayoutUtility.GetLastRect();
			ShredderData shredder = FindShredder(data, cell.coord);
			if (shredder == null)
				return;

			Color oldColor = GUI.color;
			GUI.color = ColorFor(shredder.colorType);

			float thickness = Mathf.Max(2f, CellSize * 0.12f);
			Rect lineRect;

			if (shredder.axis == Axis.X)
			{
				bool hasUpperNeighbor = false;
				Vector2Int upper = new Vector2Int(cell.coord.x, cell.coord.y + 1);
				if (IsInside(data.gridSize, upper))
				{
					var upperCell = data.cells[CoordToIndex(data, upper)];
					hasUpperNeighbor = upperCell.enabled;
				}

				float yCenter = rect.y + rect.height / 2f;
				float yPos = hasUpperNeighbor ? (yCenter + offsetPixels) : (yCenter - offsetPixels);

				lineRect = new Rect(rect.x, yPos - thickness / 2f, rect.width, thickness);
			}
			else
			{
				bool hasRightNeighbor = false;
				Vector2Int right = new Vector2Int(cell.coord.x + 1, cell.coord.y);
				if (IsInside(data.gridSize, right))
				{
					var rightCell = data.cells[CoordToIndex(data, right)];
					hasRightNeighbor = rightCell.enabled;
				}

				float xCenter = rect.x + rect.width / 2f;
				float xPos = hasRightNeighbor ? (xCenter - offsetPixels) : (xCenter + offsetPixels);

				lineRect = new Rect(xPos - thickness / 2f, rect.y, thickness, rect.height);
			}

			GUI.DrawTexture(lineRect, Texture2D.whiteTexture);
			GUI.color = oldColor;
		}

		#endregion

		#region Helpers

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
				case ColorType._3Orange: return new Color(1f, 0.55f, 0.15f);
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
			{
				if (data.placedBlocks[i].id > max)
					max = data.placedBlocks[i].id;
			}

			return max + 1;
		}

		private ShredderData FindShredder(LevelData data, Vector2Int coord)
		{
			return data.shredders.Find(s => s.occupiedCells.Contains(coord));
		}

		private int GetNextShredderId(LevelData data)
		{
			int max = -1;
			foreach (var s in data.shredders)
			{
				if (s.id > max)
					max = s.id;
			}

			return max + 1;
		}

		#endregion
	}
}