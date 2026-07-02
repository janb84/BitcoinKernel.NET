using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Represents a block index entry in the block tree.
/// </summary>
public sealed class BlockIndex
{
    private readonly IntPtr _handle;
    private readonly bool _ownsHandle;

    internal BlockIndex(IntPtr handle, bool ownsHandle)
    {
        _handle = handle != IntPtr.Zero
            ? handle
            : throw new ArgumentException("Invalid block index handle", nameof(handle));
        _ownsHandle = ownsHandle;
    }

    internal IntPtr Handle => _handle;

    /// <summary>
    /// Gets the block height.
    /// </summary>
    public int Height => NativeMethods.BlockTreeEntryGetHeight(_handle);

    /// <summary>
    /// Gets the block hash of this entry as a 32-byte array.
    /// </summary>
    public byte[] GetHash()
    {
        using var hash = GetBlockHash();
        return hash.ToBytes();
    }

    /// <summary>
    /// Gets the block hash of this entry as an owned <see cref="BlockHash"/> object.
    /// </summary>
    public BlockHash GetBlockHash()
    {
        var hashPtr = NativeMethods.BlockTreeEntryGetBlockHash(_handle);
        if (hashPtr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get block hash");

        // The returned pointer is owned by the block tree entry, so copy it into an
        // independently owned block hash.
        var copy = NativeMethods.BlockHashCopy(hashPtr);
        if (copy == IntPtr.Zero)
            throw new InvalidOperationException("Failed to copy block hash");

        return new BlockHash(copy);
    }

    /// <summary>
    /// Gets the previous block index, or null if this is the genesis block.
    /// </summary>
    public BlockIndex? GetPrevious()
    {
        IntPtr prevPtr = NativeMethods.BlockTreeEntryGetPrevious(_handle);
        return prevPtr != IntPtr.Zero
            ? new BlockIndex(prevPtr, ownsHandle: false)
            : null;
    }

    /// <summary>
    /// Gets the block header associated with this block index.
    /// </summary>
    public BlockHeader GetBlockHeader()
    {
        var headerPtr = NativeMethods.BlockTreeEntryGetBlockHeader(_handle);
        if (headerPtr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get block header from block index");

        return new BlockHeader(headerPtr);
    }
}
