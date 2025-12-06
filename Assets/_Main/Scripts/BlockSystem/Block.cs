using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.Container;
using _Main.Scripts.ShredderSystem;
using BaseSystems.Scripts.Utilities;
using DG.Tweening;
using Fiber.Managers;
using UnityEngine;

namespace _Main.Scripts.BlockSystem
{
	public class Block : MonoBehaviour
	{
		[SerializeField] private ColorType colorType;

		public BlockMovementController blockMovementController;
		public List<UnitBlock> unitBlocks = new List<UnitBlock>();
		[SerializeField] private Transform model;

		public bool isUnpacked;

		public Transform Model => model;
		public ColorType ColorType => colorType;

		private void Awake()
		{
			blockMovementController.Initialize(this);
			// defaultMaterial = renderer.material;
			foreach (var unitBlock in unitBlocks)
			{
				unitBlock.Initialize(this);
			}

			UpdateInnerCoordinatesAfterRotation();
		}

		private void Start()
		{
			blockMovementController.StartMovement();

			for (var i = 0; i < unitBlocks.Count; i++)
			{
				unitBlocks[i].PlaceOnTile();
			}
		}

		public void MouseUp()
		{
			for (var i = 0; i < unitBlocks.Count; i++)
			{
				unitBlocks[i].PlaceOnTile();
			}

			model.localPosition = new Vector3(model.localPosition.x, 0, model.localPosition.z);
		}

		public void MouseDown()
		{
			for (var i = 0; i < unitBlocks.Count; i++)
			{
				unitBlocks[i].RemoveOnTile();
			}

			model.localPosition += Vector3.up * .3f;
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
			isUnpacked = true;
			blockMovementController.StartShredding();
			foreach (var unitBlock in unitBlocks)
			{
				unitBlock.currentTile.SetCurrentUnit(null);
			}

			if (shredder.axis == Axis.X)
				transform.DOMoveZ(shredder.transform.position.z, 0.4f);
			else
				transform.DOMoveX(shredder.transform.position.x, 0.4f);

			Vector3 shrinkDir = (shredder.axis == Axis.X) ? shredder.transform.forward : shredder.transform.right;

			Vector3 localShrinkDir = transform.InverseTransformDirection(shrinkDir).normalized;

			int axisIndex = Mathf.Abs(localShrinkDir.x) > 0.9f ? 0 : Mathf.Abs(localShrinkDir.y) > 0.9f ? 1 : 2;

			Vector3 targetScale = transform.localScale;
			targetScale[axisIndex] = 0f;

			transform.DOScale(targetScale, 0.3f);
			var _particle = ParticlePooler.Instance.Spawn("Shrink", shredder.transform.position, Quaternion.identity);
			_particle.Play();
			StartCoroutine(Delay());

			IEnumerator Delay()
			{
				yield return new WaitForSeconds(0.3f);

				foreach (var ub in unitBlocks)
				{
					ub.currentTile.SetCurrentUnit(null);
				}
			}
		}
	}
}