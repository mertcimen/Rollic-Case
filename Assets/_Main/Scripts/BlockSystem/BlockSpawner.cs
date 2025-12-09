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

                blocks.Add(blockInstance);

                // Let Start() or external code decide when to enable visuals
                blockInstance.gameObject.SetActive(false);
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
