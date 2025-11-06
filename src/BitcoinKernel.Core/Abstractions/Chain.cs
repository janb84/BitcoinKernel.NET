

using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core.Abstractions;


/// <summary>
/// Represents the active blockchain.
/// </summary>
public sealed class Chain
{
    private readonly IntPtr _handle;

    internal Chain(IntPtr handle)
    {
        _handle = handle != IntPtr.Zero
            ? handle
            : throw new ArgumentException("Invalid chain handle", nameof(handle));
    }

    /// <summary>
    /// Gets the height of the chain.
    /// </summary>
    public int Height => NativeMethods.ChainGetHeight(_handle);

    /// <summary>
    /// Gets the tip of the chain.
    /// </summary>
    public BlockIndex GetTip()
    {
        IntPtr tipPtr = NativeMethods.ChainGetTip(_handle);
        if (tipPtr == IntPtr.Zero)
            throw new KernelException("Failed to get chain tip");

        return new BlockIndex(tipPtr, ownsHandle: false);
    }

    /// <summary>
    /// Gets a block index by height.
    /// </summary>
    public BlockIndex? GetBlockByHeight(int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(height, nameof(height));

        IntPtr blockPtr = NativeMethods.ChainGetByHeight(_handle, height);
        return blockPtr != IntPtr.Zero
            ? new BlockIndex(blockPtr, ownsHandle: false)
            : null;
    }

    /// <summary>
    /// Gets the genesis block.
    /// </summary>
    public BlockIndex GetGenesis()
    {
        IntPtr genesisPtr = NativeMethods.ChainGetGenesis(_handle);
        if (genesisPtr == IntPtr.Zero)
            throw new KernelException("Failed to get genesis block");

        return new BlockIndex(genesisPtr, ownsHandle: false);
    }

    /// <summary>
    /// Checks if a block index is part of this chain.
    /// </summary>
    public bool Contains(BlockIndex blockIndex)
    {
        ArgumentNullException.ThrowIfNull(blockIndex);

        return NativeMethods.ChainContains(_handle, blockIndex.Handle) != 0;
    }

    /// <summary>
    /// Enumerates all blocks in the chain from genesis to tip.
    /// </summary>
    public IEnumerable<BlockIndex> EnumerateBlocks()
    {
        for (int height = 0; height <= Height; height++)
        {
            var block = GetBlockByHeight(height);
            if (block != null)
            {
                yield return block;
            }
        }
    }
}