using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.Datas;
using UnityEngine;

namespace _Main.Scripts.GridSystem
{
	public class GridArea : MonoBehaviour
	{
		[SerializeField] private Vector2Int gridSize = new Vector2Int(6, 4);

		[SerializeField, HideInInspector] private int currentWidth;
		[SerializeField, HideInInspector] private int currentHeight;

		[SerializeField, HideInInspector] private GridPointController[,] gridPoints;
		[SerializeField] private List<GridPointController> gridPointsList = new List<GridPointController>();
		[SerializeField] private Transform gridPointParent;

		public Transform GridPointParent => gridPointParent;
		public Vector2Int GridSize => gridSize;
		public GridPointController[,] GridPoints => gridPoints;
		public List<GridPointController> GridPointsList => gridPointsList;

		private LevelData levelData;

		public void Initialize(LevelData data)
		{
			this.levelData = data;
			gridSize = data.gridSize;
			CreateGrid();
			InitializeNeighbours();

			GridEnableController();
		}

		private int Index(int x, int y) => y * Mathf.Max(1, currentWidth) + x;

		public void GridEnableController()
		{
			foreach (var gp in gridPointsList) gridPoints[gp.Coordinate.x, gp.Coordinate.y] = gp;

			for (int x = 0; x < currentWidth; x++)
				for (int y = 0; y < currentHeight; y++)
				{
					bool en = levelData.cells.FirstOrDefault(g => g.coord == new Vector2Int(x, y)).enabled;
					var g = gridPoints[x, y];
					if (g != null) g.IsEnablePoint = en;
				}

			foreach (var p in gridPoints)
				if (p != null && p.IsEnablePoint)
					p.CreateEdgePoints();
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

		public void CreateGrid()
		{
			if (gridPointParent == null)
			{
				Debug.LogError("Grid Prefab Or Parent is null");
				return;
			}

			int w = gridSize.x;
			int h = gridSize.y;

			currentWidth = w;
			currentHeight = h;
			gridPoints = new GridPointController[w, h];
			gridPointsList.Clear();

			Vector2 offset = new Vector2((w - 1) * 0.5f, (h - 1) * 0.5f);
			var gridPointPrefab = ReferenceManagerSO.Instance.GridPointController;
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					Vector3 pos = new Vector3(x - offset.x, 0, y - offset.y);

					GridPointController obj = Instantiate(gridPointPrefab, pos, Quaternion.identity, gridPointParent);
					obj.CreateGridPoint(this, pos, x + "-" + y, new Vector2Int(x, y));
					gridPoints[x, y] = obj;
					gridPointsList.Add(obj);
				}
			}
		}
	}
}