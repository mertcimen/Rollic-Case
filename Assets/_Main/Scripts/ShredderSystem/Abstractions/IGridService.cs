using _Main.Scripts.GridSystem;
using UnityEngine;

namespace _Main.Scripts.ShredderSystem.Abstractions
{
	/// <summary>
	/// Abstraction over grid access so Shredder does not depend directly on GridArea.
	/// </summary>
	public interface IGridService
	{
		GridPointController GetPoint(int x, int y);
		GridPointController GetPoint(Vector2Int coord);
	}
}