using _Main.Scripts.ShredderSystem;

namespace _Main.Scripts.BlockSystem.Abstractions
{
	/// <summary>
	/// Abstraction for block destruction behavior.
	/// Block depends on this destruction process.
	/// </summary>
	public interface IBlockDestructionStrategy
	{
		void DestroyBlock(Block block, Shredder shredder);
	}
}