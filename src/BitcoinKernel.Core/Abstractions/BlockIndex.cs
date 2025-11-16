using BitcoinKernel.Interop;

namespace BitcoinKernel.Core.Abstractions;

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
    /// Gets the block hash.
    /// </summary>
    public byte[] GetBlockHash()
    {
        var hashPtr = NativeMethods.BlockTreeEntryGetBlockHash(_handle);
        if (hashPtr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get block hash");

        // The hash pointer is owned by the block tree entry, so we just read the bytes
        // without wrapping it in a BlockHash that would try to destroy it
        var bytes = new byte[32];
        NativeMethods.BlockHashToBytes(hashPtr, bytes);
        return bytes;
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
}