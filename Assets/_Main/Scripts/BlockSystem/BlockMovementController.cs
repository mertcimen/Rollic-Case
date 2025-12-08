using _Main.Scripts.Container;
using _Main.Scripts.Datas;
using _Main.Scripts.GridSystem;
using BaseSystems.AudioSystem.Scripts;
using BaseSystems.Scripts.Managers;
using Fiber.LevelSystem;
using Lofelt.NiceVibrations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Main.Scripts.BlockSystem
{
	[RequireComponent(typeof(Rigidbody))]
	public class BlockMovementController : MonoBehaviour
	{
		private Block blockController;
		private Rigidbody rb;
		private Camera mainCam;

		private bool isDragging;
		private Vector3 offset;
		private float zCoord;

		private float moveSpeed = 10f;
		private LayerMask tileLayerMask;

		public void Initialize(Block blockController)
		{
			this.blockController = blockController;
			rb = GetComponent<Rigidbody>();
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rb.isKinematic = true;
			mainCam = Camera.main;
			moveSpeed = ReferenceManagerSO.Instance.BlockMoveSpeed;
			tileLayerMask = LayerMask.GetMask("Tile");
		}

		public void SnapOnGrid()
		{
			GridPointController nearestTile =
				LevelManager.Instance.CurrentLevel.gridArea.GetNearestActiveGridPoint(transform.position);
			transform.position = nearestTile.transform.position;
		}

		private void OnMouseDown()
		{
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
			if (StateManager.Instance.CurrentState != GameState.OnStart) return;

			isDragging = true;
			rb.isKinematic = false;
			blockController.MouseDown();
			rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

			zCoord = mainCam.WorldToScreenPoint(transform.position).z;
			Vector3 mousePoint = Input.mousePosition;
			mousePoint.z = zCoord;

			LevelManager.Instance.CurrentLevel.TriggerBlockSelected(blockController);
			AudioManager.Instance.PlayAudio(AudioName.Plop1);
			offset = transform.position - mainCam.ScreenToWorldPoint(mousePoint);
		}

		private void OnMouseUp()
		{
			if (!isDragging) return;

			isDragging = false;
			StopMovement();
			SnapOnGrid();
			blockController.MouseUp();

			AudioManager.Instance.PlayAudio(AudioName.Plop3);
			HapticManager.Instance.PlayHaptic(HapticPatterns.PresetType.LightImpact);
		}

		private void FixedUpdate()
		{
			if (!isDragging || rb.isKinematic) return;

			Vector3 mousePoint = Input.mousePosition;
			mousePoint.z = zCoord;

			Vector3 targetPos = mainCam.ScreenToWorldPoint(mousePoint) + offset;
			targetPos.y = transform.position.y;

			switch (blockController.moveDirection)
			{
				case MoveType.Horizontal:
					targetPos.z = transform.position.z;
					break;

				case MoveType.Vertical:
					targetPos.x = transform.position.x;
					break;

				case MoveType.Both:
				default:
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
			for (int i = 0; i < blockController.unitBlocks.Count; i++)
			{
				var unitBlock = blockController.unitBlocks[i];

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