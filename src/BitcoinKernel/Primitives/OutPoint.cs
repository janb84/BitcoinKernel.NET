using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Represents a transaction out point (the txid and output index a transaction input spends).
/// </summary>
public sealed class OutPoint : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal OutPoint(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle != IntPtr.Zero
            ? handle
            : throw new ArgumentException("Invalid out point handle", nameof(handle));
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
    /// Gets the index of the referenced output.
    /// </summary>
    public uint Index
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.TransactionOutPointGetIndex(_handle);
        }
    }

    /// <summary>
    /// Gets the txid of the referenced transaction. The returned txid is a
    /// non-owning view whose lifetime is tied to this out point.
    /// </summary>
    public Txid GetTxid()
    {
        ThrowIfDisposed();
        var txidPtr = NativeMethods.TransactionOutPointGetTxid(_handle);
        if (txidPtr == IntPtr.Zero)
            throw new TransactionException("Failed to get txid from out point");

        return new Txid(txidPtr, ownsHandle: false);
    }

    /// <summary>
    /// Creates an owned copy of this out point.
    /// </summary>
    public OutPoint Copy()
    {
        ThrowIfDisposed();
        var copy = NativeMethods.TransactionOutPointCopy(_handle);
        if (copy == IntPtr.Zero)
            throw new TransactionException("Failed to copy out point");

        return new OutPoint(copy);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OutPoint));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero && _ownsHandle)
            {
                NativeMethods.TransactionOutPointDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~OutPoint()
    {
        if (_ownsHandle)
            Dispose();
    }
}
