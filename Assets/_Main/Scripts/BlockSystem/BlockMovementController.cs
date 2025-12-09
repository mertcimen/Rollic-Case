using _Main.Scripts.BlockSystem.Abstractions;
using _Main.Scripts.BlockSystem.Implementations;
using _Main.Scripts.Container;
using _Main.Scripts.Datas;
using _Main.Scripts.GridSystem;
using BaseSystems.Scripts.Managers;
using UnityEngine;

namespace _Main.Scripts.BlockSystem
{
	[RequireComponent(typeof(Rigidbody))]
	public class BlockMovementController : MonoBehaviour
	{
		private Block block;
		private Rigidbody rb;

		private bool isDragging;
		private float moveSpeed = 10f;
		private LayerMask tileLayerMask;

		private IBlockGridService gridService;

		public void Initialize(Block blockController, IBlockGridService gridService = null)
		{
			block = blockController;
			rb = GetComponent<Rigidbody>();
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rb.isKinematic = true;

			moveSpeed = ReferenceManagerSO.Instance.BlockMoveSpeed;
			tileLayerMask = LayerMask.GetMask("Tile");

			this.gridService = gridService ?? new DefaultBlockGridService();
		}

		private void Start()
		{
			
		}

		public void SnapOnGrid()
		{
			if (gridService == null) return;

			GridPointController nearestTile = gridService.GetNearestActiveGridPoint(transform.position);
			if (nearestTile == null)
			{
				return;
			}

			transform.position = nearestTile.transform.position;
		}

		public void BeginDrag()
		{
			isDragging = true;
			rb.isKinematic = false;
			block.MouseDown();
			rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

			if (LevelManager.Instance?.CurrentLevel != null)
			{
				LevelManager.Instance.CurrentLevel.TriggerBlockSelected(block);
			}
		}

		public void EndDrag()
		{
			isDragging = false;
			StopMovement();
			SnapOnGrid();
			block.MouseUp();
		}

		public void DragToPosition(Vector3 targetPos)
		{
			if (!isDragging) return;

			switch (block.moveDirection)
			{
				case MoveType.Horizontal:
					targetPos.z = transform.position.z;
					break;

				case MoveType.Vertical:
					targetPos.x = transform.position.x;
					break;
			}

			MoveTowardsTarget(targetPos);
		}

		private void MoveTowardsTarget(Vector3 targetPos)
		{
			float step = moveSpeed * Time.fixedDeltaTime;
			Vector3 nextPosition = Vector3.MoveTowards(rb.position, targetPos, step);
			Vector3 requiredVelocity = (nextPosition - rb.position) / Time.fixedDeltaTime;

			rb.velocity = requiredVelocity;

			UpdateUnitBlockTiles();
		}

		private void UpdateUnitBlockTiles()
		{
			if (block == null || block.unitBlocks == null) return;

			for (int i = 0; i < block.unitBlocks.Count; i++)
			{
				var unitBlock = block.unitBlocks[i];
				if (unitBlock == null) continue;

				if (Physics.Raycast(unitBlock.transform.position, Vector3.down, out var hit, 2f, tileLayerMask))
				{
					if (hit.transform.TryGetComponent(out GridPointController tile))
					{
						if (unitBlock.currentTile != null)
						{
							unitBlock.currentTile.CurrentUnitBlock = null;
						}

						if (tile.CurrentUnitBlock != unitBlock)
						{
							tile.CurrentUnitBlock = unitBlock;
						}

						unitBlock.currentTile = tile;
					}
				}
			}
		}

		private void StopMovement()
		{
			rb.isKinematic = true;
			rb.constraints = RigidbodyConstraints.FreezeAll;
			rb.velocity = Vector3.zero;
		}

		public void StartShredding()
		{
			rb.velocity = Vector3.zero;
			rb.isKinematic = true;
			isDragging = false;
			SnapOnGrid();
		}
	}
}