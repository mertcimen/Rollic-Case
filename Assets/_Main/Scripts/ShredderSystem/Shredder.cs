using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Container;
using _Main.Scripts.GridSystem;
using BaseSystems.Scripts.Managers;
using Fiber.Managers;
using UnityEngine;

namespace _Main.Scripts.ShredderSystem
{
	public class Shredder : MonoBehaviour
	{
		public Axis axis;
		public ColorType colorType;
		public List<GridPointController> controlTiles = new List<GridPointController>();
		private Collider _collider;
		
		private void Awake()
		{
			_collider = GetComponent<Collider>();
		}

		private void Start()
		{
			_collider = GetComponent<Collider>();

			FindCoveredTiles();
			foreach (var tile in controlTiles)
			{
				tile.OnItemChanged += HandleTileItemChanged;
			}

			_collider.enabled = false;
		}

		private void FindCoveredTiles()
		{
			var _gridArea = LevelManager.Instance.CurrentLevel.gridArea;

			foreach (var tile in _gridArea.GridPointsList)
			{
				if (tile != null && IsTileWithinCollider(tile))
				{
					controlTiles.Add(tile);
				}
			}
		}

		private bool IsTileWithinCollider(GridPointController tile)
		{
			Vector3 tileWorldPosition = tile.transform.position;
			return _collider.bounds.Contains(tileWorldPosition);
		}

		private void HandleTileItemChanged(GridPointController tile, UnitBlock placedBlock)
		{
			if (placedBlock == null) return;

			var block = placedBlock.mainBlock;

			if (block == null) return;
			if (block.ColorType != colorType) return;
			if (block.isUnpacked) return;

			if (axis == Axis.X)
			{
				var unitParts = block.unitBlocks.Where(x => x.currentTile).ToList();

				int minXControl = controlTiles.Min(t => t.Coordinate.x);
				int maxXControl = controlTiles.Max(t => t.Coordinate.x);
				int shredderY =
					controlTiles[0].Coordinate.y; 

				
				bool allPartsInXRange = unitParts.All(p =>
					p.currentTile.Coordinate.x >= minXControl && p.currentTile.Coordinate.x <= maxXControl);

				if (!allPartsInXRange) return;

				bool canDestroy = true;
				foreach (var part in unitParts)
				{
					int partX = part.currentTile.Coordinate.x;
					int partY = part.currentTile.Coordinate.y;

					
					if (!IsVerticalPathClear(partX, partY, shredderY, block))
					{
						canDestroy = false;
						break;
					}
				}

				if (canDestroy)
				{
					block.DestroyBlock(this);
				}
			}
			else if (axis == Axis.Y)
			{
				var unitParts = block.unitBlocks.Where(x => x.currentTile).ToList();

				int minYControl = controlTiles.Min(t => t.Coordinate.y);
				int maxYControl = controlTiles.Max(t => t.Coordinate.y);
				int shredderX = controlTiles[0].Coordinate.x;

				bool allPartsInYRange = unitParts.All(p =>
					p.currentTile.Coordinate.y >= minYControl && p.currentTile.Coordinate.y <= maxYControl);

				if (!allPartsInYRange) return;

				bool canDestroy = true;
				foreach (var part in unitParts)
				{
					int partX = part.currentTile.Coordinate.x;
					int partY = part.currentTile.Coordinate.y;

					if (!IsHorizontalPathClear(partY, partX, shredderX, block))
					{
						canDestroy = false;
						break;
					}
				}

				if (canDestroy)
				{
					block.DestroyBlock(this);
				}
			}
		}

		private bool IsVerticalPathClear(int x, int yA, int yB, Block currentItem)
		{
			var grid = LevelManager.Instance.CurrentLevel.gridArea;
			int start = Mathf.Min(yA, yB);
			int end = Mathf.Max(yA, yB);

			for (int y = start; y <= end; y++)
			{
				GridPointController tile = grid.GridPoints[x, y];
				if (tile == null) continue;
				var unitBlock = tile.CurrentUnitBlock;
				if (unitBlock != null && unitBlock.mainBlock != currentItem)
					return false; 
			}

			return true;
		}

		private bool IsHorizontalPathClear(int y, int xA, int xB, Block currentItem)
		{
			var grid = LevelManager.Instance.CurrentLevel.gridArea;
			int start = Mathf.Min(xA, xB);
			int end = Mathf.Max(xA, xB);

			for (int x = start; x <= end; x++)
			{
				GridPointController tile = grid.GridPoints[x, y];
				if (tile == null) continue;
				var unitBlock = tile.CurrentUnitBlock;
				if (unitBlock != null && unitBlock.mainBlock != currentItem)
					return false;
			}

			return true;
		}

	
	
	}
}