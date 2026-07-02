using System.Runtime.InteropServices;
using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Represents a transaction id (txid).
/// </summary>
public sealed class Txid : IDisposable, IEquatable<Txid>
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal Txid(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle != IntPtr.Zero
            ? handle
            : throw new ArgumentException("Invalid txid handle", nameof(handle));
        _ownsHandle = ownsHandle;
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
    /// Converts the txid to a 32-byte array.
    /// </summary>
    public byte[] ToBytes()
    {
        ThrowIfDisposed();
        var bytes = new byte[32];
        NativeMethods.TxidToBytes(_handle, bytes);
        return bytes;
    }

    /// <summary>
    /// Creates an owned copy of this txid.
    /// </summary>
    public Txid Copy()
    {
        ThrowIfDisposed();
        var copy = NativeMethods.TxidCopy(_handle);
        if (copy == IntPtr.Zero)
            throw new TransactionException("Failed to copy txid");

        return new Txid(copy);
    }

    /// <summary>
    /// Determines whether this txid equals another.
    /// </summary>
    public bool Equals(Txid? other)
    {
        if (other is null) return false;
        ThrowIfDisposed();
        return NativeMethods.TxidEquals(_handle, other.Handle) != 0;
    }

    public override bool Equals(object? obj) => obj is Txid other && Equals(other);

    public override int GetHashCode() => Convert.ToHexString(ToBytes()).GetHashCode();

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Txid));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero && _ownsHandle)
            {
                NativeMethods.TxidDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~Txid()
    {
        if (_ownsHandle)
            Dispose();
    }
}
