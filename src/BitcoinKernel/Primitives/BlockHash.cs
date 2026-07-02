using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;
/// <summary>
/// Represents a block hash.
/// </summary>
public sealed class BlockHash : IDisposable, IEquatable<BlockHash>
{
    private IntPtr _handle;
    private bool _disposed;

    internal BlockHash(IntPtr handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Creates a block hash from 32-byte array.
    /// </summary>
    public static BlockHash FromBytes(byte[] hash)
    {
        ArgumentNullException.ThrowIfNull(hash, nameof(hash));
        if (hash.Length != 32) throw new ArgumentException("Block hash must be 32 bytes", nameof(hash));

        IntPtr hashPtr;
        unsafe
        {
            fixed (byte* ptr = hash)
            {
                hashPtr = NativeMethods.BlockHashCreate(ptr);
            }
        }

        if (hashPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to create block hash");
        }

        return new BlockHash(hashPtr);
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
    /// Converts the block hash to a 32-byte array.
    /// </summary>
    public byte[] ToBytes()
    {
        ThrowIfDisposed();
        var bytes = new byte[32];
        NativeMethods.BlockHashToBytes(_handle, bytes);
        return bytes;
    }

    /// <summary>
    /// Creates an owned copy of this block hash.
    /// </summary>
    public BlockHash Copy()
    {
        ThrowIfDisposed();
        var copy = NativeMethods.BlockHashCopy(_handle);
        if (copy == IntPtr.Zero)
            throw new BlockException("Failed to copy block hash");

        return new BlockHash(copy);
    }

    /// <summary>
    /// Determines whether this block hash equals another.
    /// </summary>
    public bool Equals(BlockHash? other)
    {
        if (other is null) return false;
        ThrowIfDisposed();
        return NativeMethods.BlockHashEquals(_handle, other.Handle) != 0;
    }

    public override bool Equals(object? obj) => obj is BlockHash other && Equals(other);

    public override int GetHashCode() => Convert.ToHexString(ToBytes()).GetHashCode();

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BlockHash));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.BlockHashDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~BlockHash() => Dispose();
}