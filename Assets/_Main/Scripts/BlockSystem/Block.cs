using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.Container;
using _Main.Scripts.ShredderSystem;
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

		public void Unpack(Shredder shredder)
		{
			isUnpacked = true;
			blockMovementController.StartShredding();

			StartCoroutine(Delay());
			transform.DOMove(shredder.transform.position, 0.4f);

			IEnumerator Delay()
			{
				yield return new WaitForSeconds(0.3f);

				for (var i = 0; i < unitBlocks.Count; i++)
				{
					unitBlocks[i].currentTile.SetCurrentUnit(null);
					yield return new WaitForSeconds(0.1f);
					unitBlocks[i].gameObject.SetActive(false);
				}
			}
		}
	}
}