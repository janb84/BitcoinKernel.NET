using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core.Abstractions;

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
    /// Gets the block hash of this header.
    /// </summary>
    public byte[] GetHash()
    {
        ThrowIfDisposed();
        var hashPtr = NativeMethods.BlockHeaderGetHash(_handle);
        if (hashPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to get block hash from header");
        }

        using var blockHash = new BlockHash(hashPtr);
        return blockHash.ToBytes();
    }

    /// <summary>
    /// Gets the previous block hash from this header.
    /// </summary>
    public byte[] GetPrevHash()
    {
        ThrowIfDisposed();
        var hashPtr = NativeMethods.BlockHeaderGetPrevHash(_handle);
        if (hashPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to get previous block hash from header");
        }

        // The hash pointer is unowned and only valid for the lifetime of the header
        var bytes = new byte[32];
        NativeMethods.BlockHashToBytes(hashPtr, bytes);
        return bytes;
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
