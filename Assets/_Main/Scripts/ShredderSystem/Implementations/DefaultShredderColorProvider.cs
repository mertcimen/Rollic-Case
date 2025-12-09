using _Main.Scripts.Container;
using _Main.Scripts.ShredderSystem.Abstractions;
using UnityEngine;

namespace _Main.Scripts.ShredderSystem.Implementations
{
	/// <summary>
	/// Default implementation of IShredderColorProvider.
	/// Maps ColorType to actual Unity Color values.
	/// </summary>
	public sealed class DefaultShredderColorProvider : IShredderColorProvider
	{
		public Color GetColor(ColorType type)
		{
			switch (type)
			{
				case ColorType._1Blue:
					return new Color(0.15f, 0.35f, 1f);

				case ColorType._2Green:
					return new Color(0.15f, 0.85f, 0.35f);

				case ColorType._3Orange:
					return new Color(1f, 0.55f, 0.15f);

				case ColorType._4Pink:
					return new Color(1f, 0.35f, 0.75f);

				case ColorType._5Purple:
					return new Color(0.55f, 0.25f, 0.75f);

				case ColorType._6Red:
					return new Color(1f, 0.2f, 0.25f);

				case ColorType._7Yellow:
					return new Color(1f, 0.9f, 0.15f);

				case ColorType._8Brown:
					return new Color(0.45f, 0.25f, 0.1f);

				case ColorType._9Turquoise:
					return new Color(0.1f, 0.85f, 0.85f);

				default:
					return Color.white;
			}
		}
	}
}