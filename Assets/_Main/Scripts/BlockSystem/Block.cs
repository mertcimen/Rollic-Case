using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.Container;
using _Main.Scripts.ShredderSystem;
using BaseSystems.Scripts.Managers;
using BaseSystems.Scripts.Utilities;
using DG.Tweening;
using UnityEngine;

namespace _Main.Scripts.BlockSystem
{
	public class Block : MonoBehaviour
	{
		private BlockType blockType;
		private ColorType colorType;
		private Outline outline;

		[SerializeField] private BlockMovementController blockMovementController;
		[SerializeField] private Transform model;

		public MoveType moveDirection;
		public bool isDestroyed;
		public List<UnitBlock> unitBlocks = new List<UnitBlock>();

		#region Properties

		public Transform Model => model;
		public ColorType ColorType => colorType;
		public BlockType BlockType => blockType;
		public Outline Outline => outline;

		#endregion

		[SerializeField] List<Transform> arrows = new List<Transform>();

		public void Setup(ColorType colorType, MoveType moveType)
		{
			moveDirection = moveType;
			this.colorType = colorType;
			outline = GetComponent<Outline>();
			blockMovementController.Initialize(this);
			foreach (var unitBlock in unitBlocks)
			{
				unitBlock.Initialize(this);
			}

			UpdateInnerCoordinatesAfterRotation();
			UpdateArrows();
			outline.enabled = false;
		}

		private void UpdateArrows()
		{
			foreach (Transform arrow in arrows)
			{
				Vector3 dir = arrow.forward;

				float dotForward = Vector3.Dot(dir, Vector3.forward);
				float dotBack = Vector3.Dot(dir, Vector3.back);
				float dotLeft = Vector3.Dot(dir, Vector3.left);
				float dotRight = Vector3.Dot(dir, Vector3.right);

				bool shouldBeActive = false;

				if (moveDirection == MoveType.Vertical)
				{
					if (dotForward > 0.9f || dotBack > 0.9f)
						shouldBeActive = true;
				}
				else if (moveDirection == MoveType.Horizontal)
				{
					if (dotRight > 0.9f || dotLeft > 0.9f)
						shouldBeActive = true;
				}

				arrow.gameObject.SetActive(shouldBeActive);
			}
		}

		public void MouseUp()
		{
			for (var i = 0; i < unitBlocks.Count; i++)
			{
				unitBlocks[i].PlaceOnTile();
			}

			model.localPosition = new Vector3(model.localPosition.x, 0, model.localPosition.z);
			CloseOutline();
		}

		public void MouseDown()
		{
			for (var i = 0; i < unitBlocks.Count; i++)
			{
				unitBlocks[i].RemoveOnTile();
			}

			model.localPosition += Vector3.up * .3f;
			OpenOutline();
		}

		private void CloseOutline()
		{
			outline.OutlineWidth = 0f;
			outline.enabled = false;
			// trail.emitting = false;
		}

		private void OpenOutline()
		{
			outline.OutlineWidth = 5f;
			outline.enabled = true;
		}

		private void UpdateInnerCoordinatesAfterRotation()
		{
			foreach (var unit in unitBlocks)
			{
				unit.innerCoordinate =
					new Vector2Int(Mathf.RoundToInt(unit.transform.position.x - transform.position.x),
						Mathf.RoundToInt(unit.transform.position.z - transform.position.z));
			}
		}

		public void DestroyBlock(Shredder shredder)
		{
			isDestroyed = true;
			blockMovementController.StartShredding();
			transform.DOMoveY(0, 0.1f);
			CloseOutline();
			foreach (var unitBlock in unitBlocks)
			{
				unitBlock.Disable();
			}

			Vector3 direction = (transform.position - shredder.transform.position).normalized;

			direction.y = 0;
			if (shredder.axis == Axis.X)
				transform.DOMove(transform.position + Vector3.forward * 5 * -direction.z, 3f).SetSpeedBased(true)
					.OnComplete((() => gameObject.SetActive(false)));

			else
				transform.DOMove(transform.position + (Vector3.right * 5 * -direction.x), 3f).SetSpeedBased(true)
					.OnComplete((() => gameObject.SetActive(false)));

			var _particle = ParticlePooler.Instance.Spawn("Shrink", shredder.transform.position + Vector3.up * 1.5f,
				shredder.transform.rotation);

			shredder.SetParticleColor(_particle);
			_particle.Play();

			foreach (var ub in unitBlocks)
			{
				ub.currentTile.SetCurrentUnit(null);
			}

			for (var i = 0; i < shredder.controlTiles.Count; i++)
			{
				shredder.controlTiles[i].SetCurrentUnit(null);
			}

			LevelManager.Instance.CurrentLevel.DecreaseBlockCount();
		}
	}
}