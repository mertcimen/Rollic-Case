using System.Linq;
using _Main.Scripts.GridSystem;
using _Main.Scripts.ShredderSystem.Abstractions;
using _Main.Scripts.ShredderSystem.Implementations;
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
        private IShredderColorProvider colorProvider;

        public void Initialize(Block mainBlock, IShredderColorProvider colorProvider)
        {
            this.mainBlock = mainBlock;
            this.colorProvider = colorProvider ?? new DefaultShredderColorProvider();

            var renderers = GetComponentsInChildren<Renderer>();
            var activeRenderer = renderers.FirstOrDefault(x => x.gameObject.activeSelf);
            if (activeRenderer != null)
            {
                this.activeRenderer = activeRenderer;
                mainBlock.Outline.renderers.Add(activeRenderer);
            }

            mpb = new MaterialPropertyBlock();
            SetColor(this.colorProvider.GetColor(mainBlock.ColorType));
        }

        public void SetColor(Color color)
        {
            if (activeRenderer == null || mpb == null) return;

            activeRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", color);
            activeRenderer.SetPropertyBlock(mpb);
        }

        public void Disable()
        {
            if (currentTile != null)
            {
                currentTile.SetCurrentUnit(null);
                currentTile = null;
            }

            if (collider != null)
                collider.enabled = false;
        }

        public void PlaceOnTile()
        {
            if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == null)
                return;

            var gridArea = LevelManager.Instance.CurrentLevel.gridArea;
            if (gridArea == null) return;

            GridPointController nearestTile = gridArea.GetNearestActiveGridPoint(transform.position);
            if (nearestTile == null) return;

            nearestTile.SetCurrentUnit(this);
            currentTile = nearestTile;
        }

        public void RemoveOnTile()
        {
            if (currentTile != null)
            {
                currentTile.SetCurrentUnit(null);
                currentTile = null;
            }
        }
    }
}
