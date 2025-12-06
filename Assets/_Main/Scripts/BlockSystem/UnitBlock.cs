using _Main.Scripts.GridSystem;
using BaseSystems.Scripts.Managers;
using Fiber.Managers;
using UnityEngine;

namespace _Main.Scripts.BlockSystem
{
	public class UnitBlock : MonoBehaviour
	{
		public Vector2Int innerCoordinate;
		
		 public GridPointController currentTile;
		 public Block mainBlock;
		
		
		
		public void Initialize(Block mainBlock)
		{
			this.mainBlock = mainBlock;
		}
		
		
		
		public void PlaceOnTile()
		{
			GridPointController nearestTile =
				LevelManager.Instance.CurrentLevel.gridArea.GetNearestActiveGridPoint(transform.position);
			// Vector3 offSet = transform.position - nearestTile;
			nearestTile.SetCurrentUnit(this);
			currentTile = nearestTile;
			
		}

		public void RemoveOnTile()
		{
			if (currentTile != null)
			{
				currentTile.SetCurrentUnit(null);
			}
		}
		
	}
}