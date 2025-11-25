using BitcoinKernel.Core.Abstractions;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core.BlockProcessing;

/// <summary>
/// Represents an entry in the block tree (block index).
/// </summary>
public sealed class BlockTreeEntry : IEquatable<BlockTreeEntry>
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

    /// <summary>
    /// Determines whether two block tree entries are equal.
    /// Two block tree entries are equal when they point to the same block.
    /// </summary>
    public bool Equals(BlockTreeEntry? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return NativeMethods.BlockTreeEntryEquals(_handle, other._handle) == 1;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as BlockTreeEntry);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // Use the block hash bytes to compute hash code
        // Read directly from native without wrapping in a BlockHash that would dispose
        var hashPtr = NativeMethods.BlockTreeEntryGetBlockHash(_handle);
        if (hashPtr == IntPtr.Zero)
        {
            return 0;
        }
        var hashBytes = new byte[32];
        NativeMethods.BlockHashToBytes(hashPtr, hashBytes);
        return BitConverter.ToInt32(hashBytes, 0);
    }

    /// <summary>
    /// Determines whether two block tree entries are equal.
    /// </summary>
    public static bool operator ==(BlockTreeEntry? left, BlockTreeEntry? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two block tree entries are not equal.
    /// </summary>
    public static bool operator !=(BlockTreeEntry? left, BlockTreeEntry? right) => !(left == right);
}