using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Represents a block header containing metadata about a block.
/// </summary>
public sealed class BlockHeader : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal BlockHeader(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle != IntPtr.Zero
            ? handle
            : throw new ArgumentException("Invalid block header handle", nameof(handle));
        _ownsHandle = ownsHandle;
    }

    /// <summary>
    /// Creates a block header from raw serialized data (80 bytes).
    /// </summary>
    public static BlockHeader FromBytes(byte[] rawHeaderData)
    {
        ArgumentNullException.ThrowIfNull(rawHeaderData, nameof(rawHeaderData));
        if (rawHeaderData.Length != 80)
            throw new ArgumentException("Block header must be exactly 80 bytes", nameof(rawHeaderData));

        IntPtr headerPtr = NativeMethods.BlockHeaderCreate(rawHeaderData, (UIntPtr)rawHeaderData.Length);

        if (headerPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to create block header from raw data");
        }

        return new BlockHeader(headerPtr);
    }

    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    /// <summary>
    /// Gets the block hash of this header as a 32-byte array.
    /// </summary>
    public byte[] GetHash()
    {
        using var blockHash = GetBlockHash();
        return blockHash.ToBytes();
    }

    /// <summary>
    /// Gets the previous block hash from this header as a 32-byte array.
    /// </summary>
    public byte[] GetPrevHash()
    {
        using var prevHash = GetPrevBlockHash();
        return prevHash.ToBytes();
    }

    /// <summary>
    /// Gets the block hash of this header as an owned <see cref="BlockHash"/> object.
    /// </summary>
    public BlockHash GetBlockHash()
    {
        ThrowIfDisposed();
        var hashPtr = NativeMethods.BlockHeaderGetHash(_handle);
        if (hashPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to get block hash from header");
        }

        return new BlockHash(hashPtr);
    }

    /// <summary>
    /// Gets the previous block hash of this header as an owned <see cref="BlockHash"/> object.
    /// </summary>
    public BlockHash GetPrevBlockHash()
    {
        ThrowIfDisposed();
        var hashPtr = NativeMethods.BlockHeaderGetPrevHash(_handle);
        if (hashPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to get previous block hash from header");
        }

        // The returned pointer is unowned (tied to the header lifetime), so copy it
        // into an independently owned block hash.
        var copy = NativeMethods.BlockHashCopy(hashPtr);
        if (copy == IntPtr.Zero)
        {
            throw new BlockException("Failed to copy previous block hash");
        }

        return new BlockHash(copy);
    }

    /// <summary>
    /// Serializes this header to its 80-byte representation.
    /// </summary>
    public byte[] ToBytes()
    {
        ThrowIfDisposed();
        var bytes = new byte[80];
        int result = NativeMethods.BlockHeaderToBytes(_handle, bytes);
        if (result != 0)
        {
            throw new BlockException("Failed to serialize block header");
        }

        return bytes;
    }

    /// <summary>
    /// Creates an owned copy of this block header.
    /// </summary>
    public BlockHeader Copy()
    {
        ThrowIfDisposed();
        var copy = NativeMethods.BlockHeaderCopy(_handle);
        if (copy == IntPtr.Zero)
        {
            throw new BlockException("Failed to copy block header");
        }

        return new BlockHeader(copy);
    }

    /// <summary>
    /// Gets the timestamp from this header (Unix epoch seconds).
    /// </summary>
    public uint Timestamp
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.BlockHeaderGetTimestamp(_handle);
        }
    }

    /// <summary>
    /// Gets the nBits difficulty target from this header (compact format).
    /// </summary>
    public uint Bits
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.BlockHeaderGetBits(_handle);
        }
    }

    /// <summary>
    /// Gets the version from this header.
    /// </summary>
    public int Version
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.BlockHeaderGetVersion(_handle);
        }
    }

    /// <summary>
    /// Gets the nonce from this header.
    /// </summary>
    public uint Nonce
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.BlockHeaderGetNonce(_handle);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BlockHeader));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero && _ownsHandle)
            {
                NativeMethods.BlockHeaderDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~BlockHeader()
    {
        if (_ownsHandle)
            Dispose();
    }
}
