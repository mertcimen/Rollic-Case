using _Main.Scripts.Container;
using UnityEngine;

namespace _Main.Scripts.ShredderSystem.Abstractions
{
	/// <summary>
	/// Abstraction for providing colors based on ColorType.
	/// Shredder depends on this instead of a concrete implementation.
	/// </summary>
	public interface IShredderColorProvider
	{
		Color GetColor(ColorType type);
	}
}