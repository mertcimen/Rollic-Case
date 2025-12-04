using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Container;
using _Main.Scripts.GridSystem;
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

				float minXControlTile = controlTiles.Min(t => t.Coordinate.x);
				minXControlTile -= 0.5f;

				float maxXControlTile = controlTiles.Max(t => t.Coordinate.x);
				maxXControlTile += 0.5f;
				bool isWithinBounds = unitParts.All(p =>
					p.currentTile.Coordinate.x >= minXControlTile && p.currentTile.Coordinate.x <= maxXControlTile);

				if (isWithinBounds)
				{
					bool _canDestroyable = true;

					for (var i = 0; i < unitParts.Count; i++)
					{
						var upperY = unitParts[i].currentTile.Coordinate.y;
						var minY = controlTiles[0].Coordinate.y;
						if (CheckAnyDifferentItemBetween(unitParts[i].currentTile, controlTiles[0], block))
						{
							_canDestroyable = false;
							break;
						}
					}

					if (_canDestroyable)
					{
						block.Unpack(this);
					}
				}
			}
			else if (axis == Axis.Y)
			{
				var unitParts = block.unitBlocks.Where(x => x.currentTile).ToList();

				float minYControlTile = controlTiles.Min(t => t.Coordinate.y);
				minYControlTile -= 0.5f;

				float maxYControlTile = controlTiles.Max(t => t.Coordinate.y);
				maxYControlTile += 0.5f;

				bool isWithinBounds = unitParts.All(p =>
					p.currentTile.Coordinate.y >= minYControlTile && p.currentTile.Coordinate.y <= maxYControlTile);

				if (isWithinBounds)
				{
					bool _canDestroyable = true;

					for (var i = 0; i < unitParts.Count; i++)
					{
						var placedX = unitParts[i].currentTile.Coordinate.x;
						var controlX = controlTiles[0].Coordinate.x;

						if (CheckAnyDifferentItemBetweenForY(unitParts[i].currentTile, controlTiles[0], block))
						{
							_canDestroyable = false;
							break;
						}
					}

					if (_canDestroyable)
					{
						block.Unpack(this);
					}
				}
				else
				{
				}
			}
		}

		private bool CheckAnyDifferentItemBetween(GridPointController maksYTile, GridPointController minYTile,
			Block currentItem)
		{
			var gridManager = LevelManager.Instance.CurrentLevel.gridArea;

			int x = maksYTile.Coordinate.x;

			int maksY = maksYTile.Coordinate.y;
			int minY = minYTile.Coordinate.y;

			for (int y = maksY; y >= minY; y--)
			{
				GridPointController tile = gridManager.GridPoints[x, y];
				if (tile != null && tile.CurrentUnitBlock != null && tile.CurrentUnitBlock.mainBlock != currentItem)
				{
					return true;
				}
			}

			return false;
		}

		private bool CheckAnyDifferentItemBetweenForY(GridPointController maxXTile, GridPointController minXTile,
			Block currentItem)
		{
			var gridManager = LevelManager.Instance.CurrentLevel.gridArea;

			int y = maxXTile.Coordinate.y;
			int maxX = maxXTile.Coordinate.x;
			int minX = minXTile.Coordinate.x;

			for (int x = minX; x <= maxX; x++)
			{
				GridPointController tile = gridManager.GridPoints[x, y];

				if (tile != null && tile.CurrentUnitBlock != null && tile.CurrentUnitBlock.mainBlock != currentItem)
				{
					return true;
				}
			}

			return false;
		}
	}
}