using _Main.Scripts.GridSystem;
using _Main.Scripts.ShredderSystem.Abstractions;
using UnityEngine;

namespace _Main.Scripts.ShredderSystem.Implementations
{
	/// <summary>
	/// Adapter for GridArea so that Shredder only depends on IGridService.
	/// </summary>
	public sealed class GridAreaGridService : IGridService
	{
		private readonly GridArea gridArea;

		public GridAreaGridService(GridArea gridArea)
		{
			this.gridArea = gridArea;
		}

		public GridPointController GetPoint(int x, int y)
		{
			if (gridArea?.GridPoints == null)
				return null;

			int width = gridArea.GridPoints.GetLength(0);
			int height = gridArea.GridPoints.GetLength(1);

			if (x < 0 || x >= width) return null;
			if (y < 0 || y >= height) return null;

			return gridArea.GridPoints[x, y];
		}

		public GridPointController GetPoint(Vector2Int coord)
		{
			return GetPoint(coord.x, coord.y);
		}
	}
}