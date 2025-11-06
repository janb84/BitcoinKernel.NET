using BitcoinKernel.Core.Abstractions;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core.BlockProcessing;

/// <summary>
/// Represents an entry in the block tree (block index).
/// </summary>
public sealed class BlockTreeEntry
{
    private readonly IntPtr _handle;

    internal BlockTreeEntry(IntPtr handle)
    {
        _handle = handle;
    }

    internal IntPtr Handle => _handle;

    /// <summary>
    /// Gets the block hash for this entry.
    /// </summary>
    public byte[] GetBlockHash()
    {
        var hashPtr = NativeMethods.BlockTreeEntryGetBlockHash(_handle);
        if (hashPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to get block hash from tree entry");
        }

        using var blockHash = new BlockHash(hashPtr);
        return blockHash.ToBytes();
    }

    /// <summary>
    /// Gets the previous block tree entry (parent block).
    /// </summary>
    public BlockTreeEntry? GetPrevious()
    {
        var prevPtr = NativeMethods.BlockTreeEntryGetPrevious(_handle);
        return prevPtr != IntPtr.Zero ? new BlockTreeEntry(prevPtr) : null;
    }

    /// <summary>
    /// Gets the block height.
    /// </summary>
    public int GetHeight()
    {
        return NativeMethods.BlockTreeEntryGetHeight(_handle);
    }
}