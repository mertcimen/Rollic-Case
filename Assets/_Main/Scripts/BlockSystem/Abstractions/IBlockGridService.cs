using _Main.Scripts.GridSystem;
using UnityEngine;

namespace _Main.Scripts.BlockSystem.Abstractions
{
	/// <summary>
	/// Abstraction for grid-related queries used by BlockMovementController.
	/// </summary>
	public interface IBlockGridService
	{
		GridPointController GetNearestActiveGridPoint(Vector3 worldPosition);
	}
}