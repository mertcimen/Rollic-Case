using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.BlockSystem;
using Fiber.Managers;
using Lofelt.NiceVibrations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _Main.Scripts.GridSystem
{
	public class GridArea : MonoBehaviour
	{
		[SerializeField] private Vector2Int gridSize = new Vector2Int(6, 4);
		public Vector2Int GridSize => gridSize;

		[SerializeField, HideInInspector] private int currentWidth;
		[SerializeField, HideInInspector] private int currentHeight;

		[SerializeField, HideInInspector]
		private List<bool> gridCellBools = new List<bool>();

		[SerializeField, HideInInspector] private GridPointController[,] gridPoints;
		[SerializeField] private List<GridPointController> gridPointsList = new List<GridPointController>();
		[SerializeField] private Transform gridPointParent;

		public Transform GridPointParent => gridPointParent;
		public GridPointController[,] GridPoints => gridPoints;
		public List<GridPointController> GridPointsList => gridPointsList;

		private void Awake()
		{
			gridPoints = new GridPointController[gridSize.x, gridSize.y];
			for (var i = 0; i < gridPointsList.Count; i++)
			{
				var grid = gridPointsList[i];
				var arrayPosition = grid.Coordinate;
				gridPoints[arrayPosition.x, arrayPosition.y] = grid;
			}

			InitializeNeighbours();
		}

		public void SetGridSize(int x, int y)
		{
			gridSize = new Vector2Int(x, y);
		}

		public void InitializeNeighbours()
		{
			if (gridPoints == null)
				return;

			int width = gridPoints.GetLength(0);
			int height = gridPoints.GetLength(1);

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					GridPointController current = gridPoints[x, y];
					if (current == null || !current.IsEnablePoint)
						continue;

					List<GridPointController> neighbours = new List<GridPointController>();

					if (y > 0 && gridPoints[x, y - 1]?.IsEnablePoint == true)
						neighbours.Add(gridPoints[x, y - 1]);
					if (y < height - 1 && gridPoints[x, y + 1]?.IsEnablePoint == true)
						neighbours.Add(gridPoints[x, y + 1]);
					if (x > 0 && gridPoints[x - 1, y]?.IsEnablePoint == true)
						neighbours.Add(gridPoints[x - 1, y]);
					if (x < width - 1 && gridPoints[x + 1, y]?.IsEnablePoint == true)
						neighbours.Add(gridPoints[x + 1, y]);

					current.SetNeighbours(neighbours, this);
				}
			}
		}

		public GridPointController GetGridPointAt(Vector2Int coord)
		{
			if (coord.x < 0 || coord.y < 0 || coord.x >= gridSize.x || coord.y >= gridSize.y)
				return null;
			return gridPoints[coord.x, coord.y];
		}
		
		public GridPointController GetNearestActiveGridPoint(Vector3 position)
		{
			GridPointController nearestPoint = null;
			float nearestDistance = float.MaxValue;

			foreach (var gridPoint in gridPoints)
			{
				if (gridPoint == null || !gridPoint.IsEnablePoint)
					continue;

				float distance = Vector3.SqrMagnitude(gridPoint.transform.position - position);
				if (distance < nearestDistance)
				{
					nearestDistance = distance;
					nearestPoint = gridPoint;
				}
			}

			if (nearestPoint == null) return nearestPoint;

			return nearestPoint;
		}

		

#if UNITY_EDITOR

		/*─────────────────────────────────────────────────────────────
		 *
		 *─────────────────────────────────────────────────────────────*/

		[FoldoutGroup("Grid View")]
		[ShowIf(nameof(IsGridCreated))]
		[OnInspectorGUI("SetActiveGridArea")]
		[ShowInInspector]
		[TableMatrix(HorizontalTitle = "Grid Enable Matrix", DrawElementMethod = nameof(DrawCell), SquareCells = true,
			ResizableColumns = false, RowHeight = 22)]
		private bool[,] GridMatrix
		{
			get => GetGrid2D();
			set => SetFrom2D(value);
		}

		private bool IsGridCreated => currentWidth > 0 && currentHeight > 0;

		private int Index(int x, int y) => y * Mathf.Max(1, currentWidth) + x;

		private bool[,] GetGrid2D()
		{
			var g = new bool[currentWidth, currentHeight];
			for (int x = 0; x < currentWidth; x++)
				for (int y = 0; y < currentHeight; y++)
				{
					int fy = (currentHeight - 1) - y;
					g[x, y] = gridCellBools[Index(x, fy)];
				}

			return g;
		}

		private void SetFrom2D(bool[,] value)
		{
			int w = Mathf.Min(currentWidth, value.GetLength(0));
			int h = Mathf.Min(currentHeight, value.GetLength(1));

			for (int x = 0; x < w; x++)
				for (int y = 0; y < h; y++)
				{
					int fy = (currentHeight - 1) - y;
					gridCellBools[Index(x, fy)] = value[x, y];
				}

			GridEnableController();
		}

		private static GridArea activeGridArea;
		private void SetActiveGridArea() => activeGridArea = this;

		private static bool DrawCell(Rect rect, bool v, int uiX, int uiY)
		{
			var area = activeGridArea;
			if (area == null) return v;

			// BLOCK EDIT MODE
			if (area.editorMode == EditorMode.BlockEdit)
			{
				area.HandleRotationHotkey();
				Event e = Event.current;
				bool isHovered = rect.Contains(e.mousePosition);

				// Hover olan hücreyi pivot olarak kaydet
				if (isHovered)
				{
					area.hoveredUIPos = new Vector2Int(uiX, uiY);
					GUI.changed = true; // yeniden çizim tetikle
				}

				bool isPartOfBlock = false;
				bool canPlaceFromPivot = false;

				if (area.selectedBlockPrefab != null && area.hoveredUIPos.x >= 0)
				{
					// Bu hücre current pivot’a göre block shape’inin parçası mı?
					isPartOfBlock = area.IsBlockCell(uiX, uiY);

					// Sadece 1 kere pivot’tan yerleşebilir mi kontrolü (ghost rengi için)
					if (isPartOfBlock) canPlaceFromPivot = area.CanPlaceBlockAtInspector(area.hoveredUIPos);
				}

				Color col;

				if (isPartOfBlock)
				{
					
					col = canPlaceFromPivot
						? new Color(0f, 1f, 0f, 0.6f) 
						: new Color(1f, 0f, 0f, 0.6f); 
				}
				else
				{
					
					col = v ? new Color(0.15f, 0.8f, 0.25f) : new Color(0.9f, 0.2f, 0.2f);
				}

				EditorGUI.DrawRect(rect.Padding(2f), col);

				
				if (e.type == EventType.MouseDown && isHovered)
				{
					area.PlaceBlockFromInspector(area.hoveredUIPos);
					e.Use();
				}

				return v;
			}

			
			EditorGUI.DrawRect(rect.Padding(2f), v ? new Color(0.15f, 0.8f, 0.25f) : new Color(0.9f, 0.2f, 0.2f));

			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				v = !v;
				GUI.changed = true;
				Event.current.Use();
			}

			return v;
		}

		/*─────────────────────────────────────────────────────────────
		 *   BLOCK EDITOR
		 *─────────────────────────────────────────────────────────────*/

		public enum EditorMode
		{
			GridEdit,
			BlockEdit
		}

		[FoldoutGroup("Level Editor")] [SerializeField]
		private EditorMode editorMode = EditorMode.GridEdit;

		[FoldoutGroup("Level Editor")] [ValueDropdown(nameof(GetBlockList))] [SerializeField]
		private GameObject selectedBlockPrefab;

		[SerializeField] private List<GameObject> allBlockList;
		[SerializeField] private GameObject ghostBlock;
		[SerializeField] private int selectedRotation = 0;

		private GameObject lastPlacedBlock;
		private GameObject hoveredPlacedBlock;

		private IEnumerable<object> GetBlockList() => allBlockList;
		private Vector2Int hoveredUIPos = new Vector2Int(-1, -1);

		private void HandleRotationHotkey()
		{
			if (editorMode != EditorMode.BlockEdit)
				return;

			Event e = Event.current;
			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
			{
				selectedRotation = (selectedRotation + 90) % 360;
				GUI.changed = true; // Matrix’i repaint et
				SceneView.RepaintAll(); // SceneView’ı repaint et
				e.Use(); // Event'i tüket
			}
		}

		private Vector2Int RotateOffset(Vector2Int offset, int rot)
		{
			rot = (rot % 360 + 360) % 360;

			switch (rot)
			{
				case 0: return offset;
				case 90: return new Vector2Int(offset.y, -offset.x);
				case 180: return new Vector2Int(-offset.x, -offset.y);
				case 270: return new Vector2Int(-offset.y, offset.x);
			}

			return offset;
		}

		public bool IsBlockCell(int uiX, int uiY)
		{
			if (selectedBlockPrefab == null) return false;

			Block b = selectedBlockPrefab.GetComponent<Block>();
			if (b == null) return false;

			if (hoveredUIPos.x < 0 || hoveredUIPos.y < 0)
				return false;

			// Pivot UI → Grid
			Vector2Int pivotGrid = new Vector2Int(hoveredUIPos.x, (currentHeight - 1) - hoveredUIPos.y);

			// Hücre UI → Grid
			Vector2Int cellGrid = new Vector2Int(uiX, (currentHeight - 1) - uiY);

			foreach (var u in b.unitBlocks)
			{
				Vector2Int rotated = RotateOffset(u.innerCoordinate, selectedRotation);
				Vector2Int check = pivotGrid + rotated;

				if (check == cellGrid)
					return true;
			}

			return false;
		}

		public bool CanPlaceBlockAtInspector(Vector2Int pivotCellUI)
		{
			if (selectedBlockPrefab == null) return false;

			Block b = selectedBlockPrefab.GetComponent<Block>();
			if (b == null) return false;

			Vector2Int pivotGrid = new Vector2Int(pivotCellUI.x, (currentHeight - 1) - pivotCellUI.y);

			foreach (var u in b.unitBlocks)
			{
				Vector2Int rotated = RotateOffset(u.innerCoordinate, selectedRotation);

				Vector2Int cellGrid = new Vector2Int(pivotGrid.x + rotated.x, pivotGrid.y + rotated.y);

				GridPointController gp = GetGridPointAt(cellGrid);

				if (gp == null || !gp.IsEnablePoint || gp.HasBlock())
					return false;
			}

			return true;
		}

		public void PlaceBlockFromInspector(Vector2Int pivotCellUI)
		{
			if (selectedBlockPrefab == null) return;
			if (!CanPlaceBlockAtInspector(pivotCellUI)) return;

			Vector2Int pivotGrid = new Vector2Int(pivotCellUI.x, (currentHeight - 1) - pivotCellUI.y);

			GridPointController pivotPoint = GetGridPointAt(pivotGrid);
			if (pivotPoint == null) return;

			GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(selectedBlockPrefab, transform);
			Block comp = obj.GetComponent<Block>();
			

			UnitBlock pivotUnit = comp.unitBlocks.First(u => u.innerCoordinate == Vector2Int.zero);

			Vector3 pivotLocal = pivotUnit.transform.localPosition;

			
			Vector3 rotatedOffset = Quaternion.Euler(0, selectedRotation, 0) * pivotLocal;

			Vector3 finalPos = pivotPoint.transform.position - rotatedOffset + Vector3.up * 0.1f;

			obj.transform.position = finalPos;
			obj.transform.rotation = Quaternion.Euler(0, selectedRotation, 0);

			Undo.RegisterCreatedObjectUndo(obj, "Place Block");
			EditorSceneManager.MarkSceneDirty(obj.scene);
		}

		public Vector2Int WorldToGrid(Vector3 worldPos)
		{
			Vector3 local = worldPos - gridPointParent.position;

			float ox = (currentWidth - 1) / 2f;
			float oy = (currentHeight - 1) / 2f;

			int gx = Mathf.RoundToInt(local.x + ox);
			int gy = Mathf.RoundToInt(local.z + oy);

			return new Vector2Int(gx, gy);
		}

	
		public void GridEnableController()
		{
			gridPoints = new GridPointController[currentWidth, currentHeight];

			foreach (var gp in gridPointsList) gridPoints[gp.Coordinate.x, gp.Coordinate.y] = gp;

			for (int x = 0; x < currentWidth; x++)
				for (int y = 0; y < currentHeight; y++)
				{
					bool en = gridCellBools[Index(x, y)];
					var g = gridPoints[x, y];
					if (g != null) g.IsEnablePoint = en;
				}

			foreach (var p in gridPoints)
				if (p != null && p.IsEnablePoint)
					p.CreateEdgePoints();

			SceneView.RepaintAll();
		}

		[Button(ButtonSizes.Large), GUIColor(0.3f, 0.9f, 0.4f)]
		public void CreateGrid()
		{
			int w = gridSize.x;
			int h = gridSize.y;
			currentWidth = w;
			currentHeight = h;

			gridCellBools = new List<bool>(new bool[w * h]);
			for (int i = 0; i < gridCellBools.Count; i++) gridCellBools[i] = true;

			EditorUtility.SetDirty(this);
			gridPointsList.Clear();
			CreateGridPointsFromData();
			GridEnableController();
		}

		private void CreateGridPointsFromData()
		{
			if (!gridPointParent)
			{
				Debug.LogWarning("GridPointParent missing");
				return;
			}

			var old = gridPointParent.GetComponentsInChildren<GridPointController>().ToList();
			foreach (var o in old) DestroyImmediate(o.gameObject);

			gridPoints = new GridPointController[currentWidth, currentHeight];

			var refSO = ReferenceManagerSO.Instance;

			float ox = (currentWidth - 1) / 2f;
			float oy = (currentHeight - 1) / 2f;

			for (int x = 0; x < currentWidth; x++)
				for (int y = 0; y < currentHeight; y++)
				{
					var gp = (GridPointController)PrefabUtility.InstantiatePrefab(refSO.GridPointController,
						gridPointParent);

					float px = x - ox;
					float pz = y - oy;

					gp.CreateGridPoint(this, new Vector3(px, 0, pz), $"GridPoint_{y}_{x}", new Vector2Int(x, y));

					gridPoints[x, y] = gp;
					gridPointsList.Add(gp);
				}
		}

#endif
	}
}