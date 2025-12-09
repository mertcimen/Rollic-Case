using _Main.Scripts.BlockSystem.Abstractions;
using _Main.Scripts.Container;
using _Main.Scripts.ShredderSystem;
using BaseSystems.Scripts.Managers;
using BaseSystems.Scripts.Utilities;
using DG.Tweening;
using UnityEngine;

namespace _Main.Scripts.BlockSystem.Implementations
{
    /// <summary>
    /// Default implementation of IBlockDestructionStrategy.
    /// Contains the visual effects, particle spawning and bookkeeping.
    /// </summary>
    public sealed class DefaultBlockDestructionStrategy : IBlockDestructionStrategy
    {
        public void DestroyBlock(Block block, Shredder shredder)
        {
            if (block == null || shredder == null)
                return;

            block.IsDestroyed = true;

            // Stop movement and snap to grid before shredding
            block.BlockMovementController.StartShredding();
            block.transform.DOMoveY(0, 0.1f);
            block.CloseOutlineInternal();

            // Disable unit blocks
            foreach (var unitBlock in block.UnitBlocks)
            {
                if (unitBlock == null) continue;
                unitBlock.Disable();
            }

            // Calculate movement direction away from shredder
            Vector3 direction = (block.transform.position - shredder.transform.position).normalized;
            direction.y = 0;

            Tween moveTween;

            if (shredder.axis == Axis.X)
            {
                Vector3 target = block.transform.position + Vector3.forward * 5f * -direction.z;
                moveTween = block.transform.DOMove(target, 3f).SetSpeedBased(true);
            }
            else
            {
                Vector3 target = block.transform.position + Vector3.right * 5f * -direction.x;
                moveTween = block.transform.DOMove(target, 3f).SetSpeedBased(true);
            }

            moveTween.OnComplete(() => block.gameObject.SetActive(false));

            // Spawn particle effect
            var particle = ParticlePooler.Instance.Spawn(
                "Shrink",
                shredder.transform.position + Vector3.up * 1.5f,
                shredder.transform.rotation);

            shredder.SetParticleColor(particle);
            particle.Play();

            // Clear tiles of unit blocks
            foreach (var ub in block.UnitBlocks)
            {
                if (ub?.currentTile != null)
                {
                    ub.currentTile.SetCurrentUnit(null);
                    ub.currentTile = null;
                }
            }

            // Clear tiles controlled by shredder
            for (int i = 0; i < shredder.controlTiles.Count; i++)
            {
                var tile = shredder.controlTiles[i];
                if (tile == null) continue;
                tile.SetCurrentUnit(null);
            }

            // Decrease block count in level manager
            LevelManager.Instance.CurrentLevel.DecreaseBlockCount();
        }
    }
}
