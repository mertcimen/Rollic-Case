using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.Datas;
using BaseSystems.Scripts.LevelSystem;
using UnityEngine;
using _Main.Scripts.BlockSystem.Abstractions;
using _Main.Scripts.BlockSystem.Implementations;
using _Main.Scripts.ShredderSystem.Abstractions;
using _Main.Scripts.ShredderSystem.Implementations;

namespace _Main.Scripts.BlockSystem
{
    /// <summary>
    /// Responsible for creating and configuring Block instances for a given level.
    /// Acts as a simple composition root for Blocks by injecting strategies/providers.
    /// Also responsible for binding unit blocks to their initial grid cells.
    /// </summary>
    public class BlockSpawner : MonoBehaviour
    {
        private readonly List<Block> blocks = new List<Block>();

        // Shared dependencies for all spawned blocks
        private IBlockDestructionStrategy destructionStrategy;
        private IShredderColorProvider colorProvider;

        /// <summary>
        /// Initializes the spawner and creates all blocks defined in the level data.
        /// You can inject custom strategies/providers if needed, otherwise defaults are used.
        /// </summary>
        public void Initialize(
            Level level,
            LevelData levelData,
            IBlockDestructionStrategy destructionStrategy = null,
            IShredderColorProvider colorProvider = null)
        {
            if (level == null || levelData == null)
                return;

            if (level.gridArea == null || level.gridArea.GridPoints == null)
            {
                Debug.LogWarning("BlockSpawner: Level or GridArea is not properly initialized.");
                return;
            }

            // Create shared instances for this spawner if none are provided
            this.destructionStrategy = destructionStrategy ?? new DefaultBlockDestructionStrategy();
            this.colorProvider = colorProvider ?? new DefaultShredderColorProvider();

            blocks.Clear();

            foreach (var blockData in levelData.placedBlocks)
            {
                var prefab = ReferenceManagerSO.Instance.blocks
                    .FirstOrDefault(x => x.BlockType == blockData.type);

                if (prefab == null)
                    continue;

                var blockInstance = Instantiate(prefab, transform);

                // Position and rotation based on pivot cell
                var pivotGrid = blockData.pivotCoord;
                var targetTile = level.gridArea.GridPoints[pivotGrid.x, pivotGrid.y];

                blockInstance.transform.position = targetTile.transform.position;
                blockInstance.transform.localEulerAngles = new Vector3(0f, blockData.rotation, 0f);

                // Inject shared destruction strategy and color provider
                blockInstance.Setup(
                    blockData.color,
                    blockData.moveType,
                    this.destructionStrategy,
                    this.colorProvider);

                // Bind each UnitBlock to its corresponding GridPoint using occupiedCells data
                BindUnitBlocksToGrid(level, blockInstance, blockData);

                blocks.Add(blockInstance);

                // Let Start() or external code decide when to enable visuals
                blockInstance.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Uses PlacedBlockData.occupiedCells to bind each UnitBlock to the correct GridPoint.
        /// Assumes occupiedCells order matches the prefab's unitBlocks order (as created in the editor).
        /// </summary>
        private void BindUnitBlocksToGrid(Level level, Block blockInstance, PlacedBlockData blockData)
        {
            var gridArea = level.gridArea;
            var unitBlocks = blockInstance.unitBlocks;
            var occupiedCells = blockData.occupiedCells;

            if (unitBlocks == null || occupiedCells == null)
                return;

            int count = Mathf.Min(unitBlocks.Count, occupiedCells.Count);

            for (int i = 0; i < count; i++)
            {
                var unitBlock = unitBlocks[i];
                if (unitBlock == null)
                    continue;

                Vector2Int coord = occupiedCells[i];

                // Safety check for bounds
                if (coord.x < 0 || coord.x >= gridArea.GridPoints.GetLength(0) ||
                    coord.y < 0 || coord.y >= gridArea.GridPoints.GetLength(1))
                {
                    Debug.LogWarning($"BlockSpawner: Occupied cell {coord} is out of grid bounds.");
                    continue;
                }

                var tile = gridArea.GridPoints[coord.x, coord.y];
                if (tile == null)
                    continue;

                // Set both tile and unitBlock references
                tile.SetCurrentUnit(unitBlock);
                unitBlock.currentTile = tile;
            }
        }

        private void Start()
        {
            // Simple behaviour: once scene starts, activate all spawned blocks
            foreach (var block in blocks)
            {
                if (block == null) continue;
                block.gameObject.SetActive(true);
            }
        }
    }
}
