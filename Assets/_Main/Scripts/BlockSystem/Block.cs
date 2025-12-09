using System.Collections.Generic;
using _Main.Scripts.BlockSystem.Abstractions;
using _Main.Scripts.BlockSystem.Implementations;
using _Main.Scripts.Container;
using _Main.Scripts.ShredderSystem;
using _Main.Scripts.ShredderSystem.Abstractions;
using _Main.Scripts.ShredderSystem.Implementations;
using UnityEngine;

namespace _Main.Scripts.BlockSystem
{
	public class Block : MonoBehaviour
	{
		[SerializeField] private BlockType blockType;
		[SerializeField] private BlockMovementController blockMovementController;
		[SerializeField] private Transform model;
		[SerializeField] private List<Transform> arrows = new List<Transform>();

		private ColorType colorType;
		private Outline outline;

		private IBlockDestructionStrategy destructionStrategy;
		private IShredderColorProvider colorProvider;

		public MoveType moveDirection;
		public bool isDestroyed;
		public List<UnitBlock> unitBlocks = new List<UnitBlock>();

		#region Properties

		public Transform Model => model;
		public ColorType ColorType => colorType;
		public BlockType BlockType => blockType;
		public Outline Outline => outline;
		public BlockMovementController BlockMovementController => blockMovementController;
		public IReadOnlyList<UnitBlock> UnitBlocks => unitBlocks;
		public bool IsDestroyed
		{
			get => isDestroyed;
			set => isDestroyed = value;
		}

		#endregion

		private void Awake()
		{
			outline = GetComponent<Outline>();

			// Default dependencies if none are provided
			if (destructionStrategy == null)
				destructionStrategy = new DefaultBlockDestructionStrategy();

			if (colorProvider == null)
				colorProvider = new DefaultShredderColorProvider();
		}

		/// <summary>
		/// Setup block with color and movement direction.
		/// Optionally allows injecting a custom destruction strategy and color provider.
		/// </summary>
		public void Setup(ColorType colorType, MoveType moveType, IBlockDestructionStrategy destructionStrategy = null,
			IShredderColorProvider colorProvider = null)
		{
			moveDirection = moveType;
			this.colorType = colorType;

			if (destructionStrategy != null)
				this.destructionStrategy = destructionStrategy;

			if (colorProvider != null)
				this.colorProvider = colorProvider;

			if (blockMovementController != null)
				blockMovementController.Initialize(this);

			foreach (var unitBlock in unitBlocks)
			{
				if (unitBlock == null) continue;
				unitBlock.Initialize(this, this.colorProvider);
			}

			UpdateInnerCoordinatesAfterRotation();
			UpdateArrows();
			CloseOutlineInternal();
		}

		private void UpdateArrows()
		{
			foreach (Transform arrow in arrows)
			{
				if (arrow == null) continue;

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
			for (int i = 0; i < unitBlocks.Count; i++)
			{
				unitBlocks[i].PlaceOnTile();
			}

			if (model != null)
			{
				model.localPosition = new Vector3(model.localPosition.x, 0, model.localPosition.z);
			}

			CloseOutlineInternal();
		}

		public void MouseDown()
		{
			for (int i = 0; i < unitBlocks.Count; i++)
			{
				unitBlocks[i].RemoveOnTile();
			}

			if (model != null)
			{
				model.localPosition += Vector3.up * 0.3f;
			}

			OpenOutlineInternal();
		}

		internal void CloseOutlineInternal()
		{
			if (outline == null) return;

			outline.OutlineWidth = 0f;
			outline.enabled = false;
		}

		internal void OpenOutlineInternal()
		{
			if (outline == null) return;

			outline.OutlineWidth = 5f;
			outline.enabled = true;
		}

		private void UpdateInnerCoordinatesAfterRotation()
		{
			foreach (var unit in unitBlocks)
			{
				if (unit == null) continue;

				unit.innerCoordinate =
					new Vector2Int(Mathf.RoundToInt(unit.transform.position.x - transform.position.x),
						Mathf.RoundToInt(unit.transform.position.z - transform.position.z));
			}
		}

		public void DestroyBlock(Shredder shredder)
		{
			destructionStrategy?.DestroyBlock(this, shredder);
			for (var i = 0; i < arrows.Count; i++)
			{
				arrows[i].gameObject.SetActive(false);
			}
		}
	}
}