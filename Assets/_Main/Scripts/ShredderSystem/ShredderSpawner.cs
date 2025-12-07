using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.Datas;
using _Main.Scripts.GridSystem;
using BaseSystems.Scripts.LevelSystem;
using UnityEngine;

namespace _Main.Scripts.ShredderSystem
{
	public class ShredderSpawner : MonoBehaviour
	{
		private Level level;

		public void Initialize(Level level, List<ShredderData> shredders)
		{
			this.level = level;
			foreach (var data in shredders)
			{
				var prefab = ReferenceManagerSO.Instance.shredders.FirstOrDefault(x => x.size == data.size);

				if (prefab)
				{
					var _shredder = Instantiate(prefab);
					var pivotGrid = data.pivotGrid;
					var targetTile = level.gridArea.GridPoints[pivotGrid.x, pivotGrid.y];
					_shredder.transform.position = targetTile.transform.position;
					_shredder.transform.localEulerAngles = new Vector3(0, data.rotation, 0);

					var controlTiles = new List<GridPointController>();
					foreach (var occupiedCell in data.occupiedCells)
					{
						var controlTile = level.gridArea.GridPoints[occupiedCell.x, occupiedCell.y];
						controlTiles.Add(controlTile);
					}

					_shredder.Initialize(data.axis, data.colorType, controlTiles);
					_shredder.transform.SetParent(transform);
				
				}
			}
		}
	}
}