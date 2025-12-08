using _Main.Scripts.BlockSystem;
using BaseSystems.AudioSystem.Scripts;
using BaseSystems.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Main.Scripts.Controllers
{
	public class InputController : MonoBehaviour
	{
		private Camera cam;
		private BlockMovementController selectedBlock;
		private float zCoord;
		private Vector3 offset;

		[SerializeField] private LayerMask blockLayer;

		private void Start()
		{
			cam = Camera.main;
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
				TrySelectBlock();

			if (Input.GetMouseButton(0) && selectedBlock != null)
				DragSelectedBlock();

			if (Input.GetMouseButtonUp(0) && selectedBlock != null)
				ReleaseBlock();
		}

		private void TrySelectBlock()
		{
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
				return;

			Ray ray = cam.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out var hit, 100f, blockLayer))
			{
				if (hit.transform.TryGetComponent(out BlockMovementController block))
				{
					selectedBlock = block;

					zCoord = cam.WorldToScreenPoint(block.transform.position).z;
					Vector3 mp = Input.mousePosition; mp.z = zCoord;
					offset = block.transform.position - cam.ScreenToWorldPoint(mp);

					block.BeginDrag();
					AudioManager.Instance.PlayAudio(AudioName.Plop1);
				}
			}
		}

		private void DragSelectedBlock()
		{
			Vector3 mp = Input.mousePosition; 
			mp.z = zCoord;

			Vector3 targetPos = cam.ScreenToWorldPoint(mp) + offset;
			targetPos.y = selectedBlock.transform.position.y;

			selectedBlock.DragToPosition(targetPos);
		}

		private void ReleaseBlock()
		{
			selectedBlock.EndDrag();
			selectedBlock = null;
		}
	}
}