using System;
using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Managed wrapper for a Bitcoin transaction input.
/// </summary>
public sealed class TxIn : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal TxIn(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle;
        _ownsHandle = ownsHandle;
    }

    /// <summary>
    /// Gets the nSequence value of this input.
    /// </summary>
    public uint Sequence => NativeMethods.TransactionInputGetSequence(_handle);

    /// <summary>
    /// Gets the out point of this input. The returned out point is not owned and
    /// depends on the lifetime of the transaction.
    /// </summary>
    public IntPtr GetOutPoint()
    {
        IntPtr outPointPtr = NativeMethods.TransactionInputGetOutPoint(_handle);
        if (outPointPtr == IntPtr.Zero)
            throw new TransactionException("Failed to get out point from transaction input");

        return outPointPtr;
    }

    /// <summary>
    /// Creates a copy of this transaction input.
    /// </summary>
    public TxIn Copy()
    {
        IntPtr copyHandle = NativeMethods.TransactionInputCopy(_handle);
        if (copyHandle == IntPtr.Zero)
            throw new TransactionException("Failed to copy transaction input");

        return new TxIn(copyHandle, ownsHandle: true);
    }

    internal IntPtr Handle => _handle;

    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero && _ownsHandle)
        {
            NativeMethods.TransactionInputDestroy(_handle);
            _handle = IntPtr.Zero;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~TxIn()
    {
        Dispose();
    }
}
