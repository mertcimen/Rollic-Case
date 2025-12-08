using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Container;
using _Main.Scripts.GridSystem;
using BaseSystems.Scripts.Managers;
using UnityEngine;

namespace _Main.Scripts.ShredderSystem
{
	public class Shredder : MonoBehaviour
	{
		public Size size;
		public Axis axis;
		public ColorType colorType;
		public List<GridPointController> controlTiles = new List<GridPointController>();
		[SerializeField] private Renderer renderer;
		private MaterialPropertyBlock mpb;
		[SerializeField] private GameObject maskCube;

		public void Initialize(Axis axis, ColorType colorType, List<GridPointController> controlTiles)
		{
			this.axis = axis;
			this.colorType = colorType;
			this.controlTiles = controlTiles;

			SetPosition();
		}

		private void SetPosition()
		{
			Vector3 centerPos = Vector3.zero;
			foreach (var tile in controlTiles)
			{
				centerPos += tile.transform.position;
			}

			centerPos /= controlTiles.Count;

			transform.position = centerPos;

			var _gridArea = LevelManager.Instance.CurrentLevel.gridArea;
			if (_gridArea == null) return;
			if (axis == Axis.X)
			{
				if (controlTiles[0].neighbourPoints.Contains(
					    _gridArea.GetGridPointAt(new Vector2Int(controlTiles[0].Coordinate.x,
						    controlTiles[0].Coordinate.y + 1))))
				{
					transform.position += Vector3.back * 0.7f;
					transform.rotation = Quaternion.Euler(0, 180, 0);
				}
				else
				{
					transform.position += Vector3.forward * 0.7f;
				}
			}

			if (axis == Axis.Y)
			{
				if (controlTiles[0].neighbourPoints.Contains(
					    _gridArea.GetGridPointAt(new Vector2Int(controlTiles[0].Coordinate.x + 1,
						    controlTiles[0].Coordinate.y))))
				{
					transform.rotation = Quaternion.Euler(0, -90, 0);
					transform.position += Vector3.left * 0.7f;
				}
				else
				{
					transform.rotation = Quaternion.Euler(0, 90, 0);
					transform.position += Vector3.right * 0.7f;
				}
			}
		}

		private void Start()
		{
			foreach (var tile in controlTiles)
			{
				tile.OnItemChanged += HandleTileItemChanged;
			}

			Setup();
			maskCube.SetActive(true);
		}

		private void Setup()
		{
			mpb = new MaterialPropertyBlock();
			SetColor(GetColor(colorType));
		}

		public void SetParticleColor(ParticleSystem particleSystem)
		{
			if (particleSystem == null) return;

			var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
			if (renderer != null && renderer.material != null)
			{
				renderer.material.color = GetColor(colorType);
			}
		}

		private void SetColor(Color color)
		{
			renderer.GetPropertyBlock(mpb);
			mpb.SetColor("_BaseColor", color);
			renderer.SetPropertyBlock(mpb);
		}

		private void HandleTileItemChanged(GridPointController tile, UnitBlock placedBlock)
		{
			if (!IsValidBlock(placedBlock, out var block)) return;

			var unitParts = GetUnitParts(block);
			if (unitParts.Count == 0) return;

			if (axis == Axis.X)
				HandleAxisX(block, unitParts);
			else
				HandleAxisY(block, unitParts);
		}

		private bool IsValidBlock(UnitBlock placedBlock, out Block block)
		{
			block = placedBlock?.mainBlock;

			if (block == null) return false;
			if (block.ColorType != colorType) return false;
			if (block.isDestroyed) return false;

			return true;
		}

		private List<UnitBlock> GetUnitParts(Block block)
		{
			var list = new List<UnitBlock>();
			foreach (var ub in block.unitBlocks)
			{
				if (ub.currentTile != null)
					list.Add(ub);
			}

			return list;
		}

		private void GetControlTileRange(out int minX, out int maxX, out int minY, out int maxY)
		{
			minX = int.MaxValue;
			maxX = int.MinValue;
			minY = int.MaxValue;
			maxY = int.MinValue;

			foreach (var t in controlTiles)
			{
				int x = t.Coordinate.x;
				int y = t.Coordinate.y;

				if (x < minX) minX = x;
				if (x > maxX) maxX = x;
				if (y < minY) minY = y;
				if (y > maxY) maxY = y;
			}
		}

		private void HandleAxisX(Block block, List<UnitBlock> unitParts)
		{
			GetControlTileRange(out int minX, out int maxX, out _, out _);
			int shredderY = controlTiles[0].Coordinate.y;

			if (!AreAllPartsInRangeX(unitParts, minX, maxX))
				return;

			if (IsPathClearForX(block, unitParts, shredderY))
				block.DestroyBlock(this);
		}

		private void HandleAxisY(Block block, List<UnitBlock> unitParts)
		{
			GetControlTileRange(out _, out _, out int minY, out int maxY);
			int shredderX = controlTiles[0].Coordinate.x;

			if (!AreAllPartsInRangeY(unitParts, minY, maxY))
				return;

			if (IsPathClearForY(block, unitParts, shredderX))
				block.DestroyBlock(this);
		}

		private bool AreAllPartsInRangeX(List<UnitBlock> parts, int min, int max)
		{
			foreach (var p in parts)
			{
				int px = p.currentTile.Coordinate.x;
				if (px < min || px > max)
					return false;
			}

			return true;
		}

		private bool IsPathClearForX(Block block, List<UnitBlock> parts, int shredderY)
		{
			foreach (var part in parts)
			{
				int px = part.currentTile.Coordinate.x;
				int py = part.currentTile.Coordinate.y;

				if (!IsVerticalPathClear(px, py, shredderY, block))
					return false;
			}

			return true;
		}

		private bool IsPathClearForY(Block block, List<UnitBlock> parts, int shredderX)
		{
			foreach (var part in parts)
			{
				int px = part.currentTile.Coordinate.x;
				int py = part.currentTile.Coordinate.y;

				if (!IsHorizontalPathClear(py, px, shredderX, block))
					return false;
			}

			return true;
		}

		private bool AreAllPartsInRangeY(List<UnitBlock> parts, int min, int max)
		{
			foreach (var p in parts)
			{
				int py = p.currentTile.Coordinate.y;
				if (py < min || py > max)
					return false;
			}

			return true;
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

		public Color GetColor(ColorType type)
		{
			switch (type)
			{
				case ColorType._1Blue:
					return new Color(0.15f, 0.35f, 1f);

				case ColorType._2Green:
					return new Color(0.15f, 0.85f, 0.35f);

				case ColorType._3Orange:
					return new Color(1f, 0.55f, 0.15f);

				case ColorType._4Pink:
					return new Color(1f, 0.35f, 0.75f);

				case ColorType._5Purple:
					return new Color(0.55f, 0.25f, 0.75f);

				case ColorType._6Red:
					return new Color(1f, 0.2f, 0.25f);

				case ColorType._7Yellow:
					return new Color(1f, 0.9f, 0.15f);

				case ColorType._8Brown:
					return new Color(0.45f, 0.25f, 0.1f);

				case ColorType._9Turquoise:
					return new Color(0.1f, 0.85f, 0.85f);

				default:
					return Color.white;
			}
		}
	}
}