using _Main.Scripts.BlockSystem.Abstractions;
using _Main.Scripts.GridSystem;
using BaseSystems.Scripts.Managers;
using UnityEngine;

namespace _Main.Scripts.BlockSystem.Implementations
{
	/// <summary>
	/// Default implementation of IBlockGridService using LevelManager and GridArea.
	/// </summary>
	public sealed class DefaultBlockGridService : IBlockGridService
	{
		public GridPointController GetNearestActiveGridPoint(Vector3 worldPosition)
		{
			if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == null)
				return null;

			var gridArea = LevelManager.Instance.CurrentLevel.gridArea;
			if (gridArea == null)
				return null;

			return gridArea.GetNearestActiveGridPoint(worldPosition);
		}
	}
}