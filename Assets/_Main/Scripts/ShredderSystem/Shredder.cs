using System.Collections.Generic;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.Container;
using _Main.Scripts.GridSystem;
using _Main.Scripts.ShredderSystem.Abstractions;
using _Main.Scripts.ShredderSystem.Implementations;
using BaseSystems.Scripts.Managers;
using UnityEngine;

namespace _Main.Scripts.ShredderSystem
{
    /// <summary>
    /// Shredder is responsible for:
    /// - Reacting to tiles' item changes
    /// - Checking if a block can be shredded along X or Y axis
    /// - Handling visual appearance (color and position)
    ///
    /// It depends on abstractions (IGridService, IShredderColorProvider)
    /// to comply with Dependency Inversion Principle.
    /// </summary>
    public class Shredder : MonoBehaviour
    {
        [Header("Config")]
        public Size size;
        public Axis axis;
        public ColorType colorType;
        public List<GridPointController> controlTiles = new List<GridPointController>();

        [Header("References")]
        [SerializeField] private Renderer renderer;
        [SerializeField] private GameObject maskCube;
        [SerializeField] private GridArea gridArea;

        private MaterialPropertyBlock mpb;

        // Dependencies (abstractions, not concrete implementations)
        private IShredderColorProvider colorProvider;
        private IGridService gridService;

        #region Unity Lifecycle

        private void Awake()
        {
            EnsureDependencies();
        }

        private void Start()
        {
            SubscribeTileEvents();
            SetupVisuals();

            if (maskCube != null)
                maskCube.SetActive(true);
        }

        private void OnDestroy()
        {
            UnsubscribeTileEvents();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialization entry point for setting up the shredder.
        /// All dependencies can be injected here. For simple usage,
        /// you can pass only axis, colorType and controlTiles,
        /// leaving other parameters null to use default implementations.
        /// </summary>
        public void Initialize(
            Axis axis,
            ColorType colorType,
            List<GridPointController> controlTiles,
            IGridService gridService = null,
            IShredderColorProvider colorProvider = null)
        {
            this.axis = axis;
            this.colorType = colorType;
            this.controlTiles = controlTiles ?? new List<GridPointController>();

            if (gridService != null)
                this.gridService = gridService;

            if (colorProvider != null)
                this.colorProvider = colorProvider;

            EnsureDependencies();

            SetPosition();
            SetupVisuals();
        }

        public void SetParticleColor(ParticleSystem particleSystem)
        {
            if (particleSystem == null) return;

            var psRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null && psRenderer.material != null)
            {
                psRenderer.material.color = GetColor(colorType);
            }
        }

        public Color GetColor(ColorType type)
        {
            return colorProvider != null ? colorProvider.GetColor(type) : Color.white;
        }

        #endregion

        #region Dependency Setup

        /// <summary>
        /// Ensures that all required dependencies are available.
        /// Uses default implementations if nothing is injected.
        /// </summary>
        private void EnsureDependencies()
        {
            if (gridService == null)
            {
                var effectiveGridArea = gridArea;

                if (effectiveGridArea == null &&
                    LevelManager.Instance != null &&
                    LevelManager.Instance.CurrentLevel != null)
                {
                    effectiveGridArea = LevelManager.Instance.CurrentLevel.gridArea;
                }

                if (effectiveGridArea != null)
                {
                    gridArea = effectiveGridArea;
                    gridService = new GridAreaGridService(effectiveGridArea);
                }
            }

            // Color provider: default implementation if not injected.
            if (colorProvider == null)
            {
                colorProvider = new DefaultShredderColorProvider();
            }

            if (mpb == null)
                mpb = new MaterialPropertyBlock();
        }

        #endregion

        #region Visual & Position

        private void SetupVisuals()
        {
            if (renderer == null) return;

            SetColor(GetColor(colorType));
        }

        private void SetColor(Color color)
        {
            if (renderer == null || mpb == null) return;

            renderer.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(mpb);
        }

        private void SetPosition()
        {
            if (controlTiles == null || controlTiles.Count == 0)
                return;

            Vector3 centerPos = Vector3.zero;
            int validTileCount = 0;

            foreach (var tile in controlTiles)
            {
                if (tile == null) continue;
                centerPos += tile.transform.position;
                validTileCount++;
            }

            if (validTileCount == 0)
                return;

            centerPos /= validTileCount;
            transform.position = centerPos;

            if (gridService == null)
                return;

            var firstTile = controlTiles[0];
            if (firstTile == null) return;

            switch (axis)
            {
                case Axis.X:
                    SetPositionForAxisX(firstTile);
                    break;
                case Axis.Y:
                    SetPositionForAxisY(firstTile);
                    break;
            }
        }

        private void SetPositionForAxisX(GridPointController firstTile)
        {
            var upNeighbour = gridService.GetPoint(
                firstTile.Coordinate.x,
                firstTile.Coordinate.y + 1);

            bool hasUpNeighbour = firstTile.neighbourPoints != null &&
                                  upNeighbour != null &&
                                  firstTile.neighbourPoints.Contains(upNeighbour);

            if (hasUpNeighbour)
            {
                transform.position += Vector3.back * 0.7f;
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                transform.position += Vector3.forward * 0.7f;
                transform.rotation = Quaternion.identity;
            }
        }

        private void SetPositionForAxisY(GridPointController firstTile)
        {
            var rightNeighbour = gridService.GetPoint(
                firstTile.Coordinate.x + 1,
                firstTile.Coordinate.y);

            bool hasRightNeighbour = firstTile.neighbourPoints != null &&
                                     rightNeighbour != null &&
                                     firstTile.neighbourPoints.Contains(rightNeighbour);

            if (hasRightNeighbour)
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

        #endregion

        #region Event Subscribe

        private void SubscribeTileEvents()
        {
            if (controlTiles == null) return;

            foreach (var tile in controlTiles)
            {
                if (tile == null) continue;
                tile.OnItemChanged += HandleTileItemChanged;
            }
        }

        private void UnsubscribeTileEvents()
        {
            if (controlTiles == null) return;

            foreach (var tile in controlTiles)
            {
                if (tile == null) continue;
                tile.OnItemChanged -= HandleTileItemChanged;
            }
        }

        #endregion

        #region Shred Logic

        private void HandleTileItemChanged(GridPointController tile, UnitBlock placedBlock)
        {
            if (!IsValidBlock(placedBlock, out var block)) return;

            var unitParts = GetUnitParts(block);
            if (unitParts.Count == 0) return;

            if (axis == Axis.X)
                HandleAxisX(block, unitParts);
            else if (axis == Axis.Y)
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
            if (block?.unitBlocks == null) return list;

            foreach (var ub in block.unitBlocks)
            {
                if (ub != null && ub.currentTile != null)
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

            if (controlTiles == null) return;

            foreach (var t in controlTiles)
            {
                if (t == null) continue;

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
            if (controlTiles == null || controlTiles.Count == 0)
                return;

            GetControlTileRange(out int minX, out int maxX, out _, out _);
            int shredderY = controlTiles[0].Coordinate.y;

            if (!AreAllPartsInRangeX(unitParts, minX, maxX))
                return;

            if (IsPathClearForX(block, unitParts, shredderY))
                block.DestroyBlock(this);
        }

        private void HandleAxisY(Block block, List<UnitBlock> unitParts)
        {
            if (controlTiles == null || controlTiles.Count == 0)
                return;

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
                if (p?.currentTile == null) return false;

                int px = p.currentTile.Coordinate.x;
                if (px < min || px > max)
                    return false;
            }

            return true;
        }

        private bool AreAllPartsInRangeY(List<UnitBlock> parts, int min, int max)
        {
            foreach (var p in parts)
            {
                if (p?.currentTile == null) return false;

                int py = p.currentTile.Coordinate.y;
                if (py < min || py > max)
                    return false;
            }

            return true;
        }

        private bool IsPathClearForX(Block block, List<UnitBlock> parts, int shredderY)
        {
            if (gridService == null) return false;

            foreach (var part in parts)
            {
                if (part?.currentTile == null) return false;

                int px = part.currentTile.Coordinate.x;
                int py = part.currentTile.Coordinate.y;

                if (!IsVerticalPathClear(px, py, shredderY, block))
                    return false;
            }

            return true;
        }

        private bool IsPathClearForY(Block block, List<UnitBlock> parts, int shredderX)
        {
            if (gridService == null) return false;

            foreach (var part in parts)
            {
                if (part?.currentTile == null) return false;

                int px = part.currentTile.Coordinate.x;
                int py = part.currentTile.Coordinate.y;

                if (!IsHorizontalPathClear(py, px, shredderX, block))
                    return false;
            }

            return true;
        }

        private bool IsVerticalPathClear(int x, int yA, int yB, Block currentItem)
        {
            if (gridService == null) return false;

            int start = Mathf.Min(yA, yB);
            int end = Mathf.Max(yA, yB);

            for (int y = start; y <= end; y++)
            {
                var tile = gridService.GetPoint(x, y);
                if (tile == null) continue;

                var unitBlock = tile.CurrentUnitBlock;
                if (unitBlock != null && unitBlock.mainBlock != currentItem)
                    return false;
            }

            return true;
        }

        private bool IsHorizontalPathClear(int y, int xA, int xB, Block currentItem)
        {
            if (gridService == null) return false;

            int start = Mathf.Min(xA, xB);
            int end = Mathf.Max(xA, xB);

            for (int x = start; x <= end; x++)
            {
                var tile = gridService.GetPoint(x, y);
                if (tile == null) continue;

                var unitBlock = tile.CurrentUnitBlock;
                if (unitBlock != null && unitBlock.mainBlock != currentItem)
                    return false;
            }

            return true;
        }

        #endregion
    }
}
