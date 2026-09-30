using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Represents a transaction input.
/// </summary>
public sealed class TransactionInput : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal TransactionInput(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle != IntPtr.Zero
            ? handle
            : throw new ArgumentException("Invalid transaction input handle", nameof(handle));
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
    /// Gets the nSequence value of this input.
    /// </summary>
    public uint Sequence
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.TransactionInputGetSequence(_handle);
        }
    }

    /// <summary>
    /// Gets the out point spent by this input. The returned out point is a
    /// non-owning view whose lifetime is tied to this input.
    /// </summary>
    public OutPoint GetOutPoint()
    {
        ThrowIfDisposed();
        var outPointPtr = NativeMethods.TransactionInputGetOutPoint(_handle);
        if (outPointPtr == IntPtr.Zero)
            throw new TransactionException("Failed to get out point from transaction input");

        return new OutPoint(outPointPtr, ownsHandle: false);
    }

    /// <summary>
    /// Gets the witness stack of this input. The returned stack is a
    /// non-owning view whose lifetime is tied to this input.
    /// </summary>
    public WitnessStack GetWitnessStack()
    {
        ThrowIfDisposed();
        var witnessPtr = NativeMethods.TransactionInputGetWitnessStack(_handle);
        if (witnessPtr == IntPtr.Zero)
            throw new TransactionException("Failed to get witness stack from transaction input");

        return new WitnessStack(witnessPtr, ownsHandle: false);
    }

    /// <summary>
    /// Gets the raw script sig bytes of this input. Empty for inputs that
    /// carry no script sig, such as native segwit spends.
    /// </summary>
    public byte[] GetScriptSig()
    {
        ThrowIfDisposed();
        var bytes = new List<byte>();
        NativeMethods.WriteBytes writer = (data, len, _) =>
        {
            // An empty script sig may arrive as a null pointer with len 0.
            if (len == 0)
                return 0;

            var buffer = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy(data, buffer, 0, (int)len);
            bytes.AddRange(buffer);
            return 0;
        };

        int result = NativeMethods.TransactionInputGetScriptSig(_handle, writer, IntPtr.Zero);
        if (result != 0)
            throw new TransactionException("Failed to serialize script sig");

        return bytes.ToArray();
    }

    /// <summary>
    /// Creates an owned copy of this transaction input.
    /// </summary>
    public TransactionInput Copy()
    {
        ThrowIfDisposed();
        var copy = NativeMethods.TransactionInputCopy(_handle);
        if (copy == IntPtr.Zero)
            throw new TransactionException("Failed to copy transaction input");

        return new TransactionInput(copy);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransactionInput));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero && _ownsHandle)
            {
                NativeMethods.TransactionInputDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~TransactionInput()
    {
        if (_ownsHandle)
            Dispose();
    }
}
