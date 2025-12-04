using System.Collections.Generic;
using _Main.Scripts.BlockSystem;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace _Main.Scripts.GridSystem
{
	public class GridPointController : MonoBehaviour
	{
		// private UnitBlock currentUnitBlock;
		// public UnitBlock CurrentUnitBlock => currentUnitBlock;
		public List<GridPointController> neighbourPoints = new List<GridPointController>();
		[SerializeField] private Transform edgePointParent;
		[SerializeField] private Collider areaBlockerCollider;
		[SerializeField] private Collider tileCollider;
		[SerializeField] private Renderer renderer;

		[SerializeField, ReadOnly] private GridArea gridArea;
		[SerializeField, ReadOnly] private Vector2Int coordinate;
		public Vector2Int Coordinate => coordinate;
		[SerializeField] private bool isEnablePoint = true;
		public bool HasBlock() => currentUnitBlock != null;
		public GridArea GridAreaReference => gridArea;

		public delegate void ItemChangedHandler(GridPointController tile, UnitBlock newItem);

		private UnitBlock currentUnitBlock;

		public UnitBlock CurrentUnitBlock
		{
			get { return currentUnitBlock; }
			set
			{
				if (currentUnitBlock != value)
				{
					currentUnitBlock = value;
					OnCurrentItemChanged();
				}
			}
		}

		public event ItemChangedHandler OnItemChanged;

		private void OnCurrentItemChanged()
		{
			OnItemChanged?.Invoke(this, currentUnitBlock);
		}

		public bool IsEnablePoint
		{
			get => isEnablePoint;
			set
			{
				isEnablePoint = value;
				foreach (Transform child in transform)
				{
					child.gameObject.SetActive(isEnablePoint);
				}

				areaBlockerCollider.enabled = !value;
			}
		}

		public void SetValuesForTutorial()
		{
			areaBlockerCollider.enabled = true;
		}

		public void SetNeighbours(List<GridPointController> neighbours, GridArea gridArea)
		{
			neighbourPoints = neighbours;
			this.gridArea = gridArea;
		}

		public void SetCurrentUnit(UnitBlock unitBlock)
		{
			currentUnitBlock = unitBlock;
		}

		public void InitForGoal()
		{
			edgePointParent.gameObject.SetActive(false);
		}

		private void Start()
		{
			// renderer.enabled = false;
		}

#if UNITY_EDITOR
		public void CreateGridPoint(GridArea gridArea, Vector3 newLocalPosition, string newName,
			Vector2Int currentGridPosition)
		{
			this.gridArea = gridArea;
			transform.localPosition = newLocalPosition;
			transform.name = newName;
			this.coordinate = currentGridPosition;
		}

		public void CreateEdgePoints()
		{
			for (int i = edgePointParent.childCount - 1; i >= 0; i--)
			{
				GameObject deletedObje = edgePointParent.GetChild(i).gameObject;
#if UNITY_EDITOR
				if (!Application.isPlaying)
					DestroyImmediate(deletedObje);
				else
#endif
					Destroy(deletedObje);
			}

			Transform edgeMiddlePointPrefab = ReferenceManagerSO.Instance.GridEgdeMiddlePoint;
			Transform edgeCornerPointPrefab = ReferenceManagerSO.Instance.GridEgdeCornerPoint;

			if (gridArea == null) return;

			int xIndex = (int)coordinate.x;
			int yIndex = (int)coordinate.y;
			Vector2 gridSize = gridArea.GridSize;

			Vector2[] directions = new Vector2[]
			{
				new Vector2(0, 1), // Up (0)
				new Vector2(1, 0), // Right (1)
				new Vector2(0, -1), // Down (2)
				new Vector2(-1, 0), // Left (3)
			};

			List<int> createdEdges = new List<int>();

			for (int i = 0; i < directions.Length; i++)
			{
				var dir = directions[i];
				int newX = xIndex + (int)dir.x;
				int newY = yIndex + (int)dir.y;

				float yRotation = 90f * i;
				bool shouldCreateEdge = false;
				if (newX < 0 || newX >= gridSize.x || newY < 0 || newY >= gridSize.y)
					shouldCreateEdge = true;
				else
				{
					GridPointController neighbor = gridArea.GridPoints[newX, newY];
					if (neighbor == null || !neighbor.IsEnablePoint)
						shouldCreateEdge = true;
				}

				if (shouldCreateEdge)
				{
					Transform newEdge =
						PrefabUtility.InstantiatePrefab(edgeMiddlePointPrefab, edgePointParent) as Transform;
					newEdge.position = transform.position;
					newEdge.localRotation = Quaternion.Euler(0f, yRotation, 0f);
					newEdge.name = $"Edge_{dir}";
					createdEdges.Add(i);
				}
				else
					createdEdges.Add(-1);
			}

			for (int i = 0; i < createdEdges.Count; i++)
			{
				if (createdEdges[i] == -1) continue;
				int current = createdEdges[i];
				int next = (current + 1) % 4; // komşu yön
				if (createdEdges.Contains(next))
				{
					Transform newCorner =
						PrefabUtility.InstantiatePrefab(edgeCornerPointPrefab, edgePointParent) as Transform;
					newCorner.position = transform.position;
					newCorner.localEulerAngles = new Vector3(0, 90 * i, 0);
					newCorner.name = $"Corner_{current}_{next}";
				}
			}
		}
#endif
	}
}