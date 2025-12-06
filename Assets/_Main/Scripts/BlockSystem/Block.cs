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


		public void Setup()
		{
			
			
			
			
			
		}
		
		private void Awake()
		{
			blockMovementController.Initialize(this);
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
				unitBlock.Disable();
			}

			// Yön hesaplama
			Vector3 direction = (transform.position - shredder.transform.position).normalized;

			direction.y = 0;
			if (shredder.axis == Axis.X)
				transform.DOMove(transform.position + Vector3.forward * 3 * -direction.z, 1f).SetSpeedBased(true);

			else
				transform.DOMove(transform.position + Vector3.right * 3 * -direction.x, 1f).SetSpeedBased(true);

			// Particles
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