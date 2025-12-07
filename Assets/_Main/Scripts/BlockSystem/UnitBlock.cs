using System.Linq;
using _Main.Scripts.Container;
using _Main.Scripts.GridSystem;
using BaseSystems.Scripts.Managers;
using UnityEngine;

namespace _Main.Scripts.BlockSystem
{
	public class UnitBlock : MonoBehaviour
	{
		public Vector2Int innerCoordinate;

		public GridPointController currentTile;
		public Block mainBlock;
		[SerializeField] private Collider collider;
		private Renderer activeRenderer;
		private MaterialPropertyBlock mpb;

		public void Initialize(Block mainBlock)
		{
			this.mainBlock = mainBlock;
			var renderers = GetComponentsInChildren<Renderer>();
			var activeRenderer = renderers.First(x => x.gameObject.activeSelf);
			if (activeRenderer != null)
			{
				this.activeRenderer = activeRenderer;
			}

			mpb = new MaterialPropertyBlock();
			SetColor(GetColor(mainBlock.ColorType));
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

		public void SetColor(Color color)
		{
			activeRenderer.GetPropertyBlock(mpb);
			mpb.SetColor("_BaseColor", color);
			activeRenderer.SetPropertyBlock(mpb);
		}

		public void Disable()
		{
			currentTile.SetCurrentUnit(null);
			collider.enabled = false;
		}

		public void PlaceOnTile()
		{
			GridPointController nearestTile =
				LevelManager.Instance.CurrentLevel.gridArea.GetNearestActiveGridPoint(transform.position);
			// Vector3 offSet = transform.position - nearestTile;
			nearestTile.SetCurrentUnit(this);
			currentTile = nearestTile;
		}

		public void RemoveOnTile()
		{
			if (currentTile != null)
			{
				currentTile.SetCurrentUnit(null);
			}
		}
	}
}