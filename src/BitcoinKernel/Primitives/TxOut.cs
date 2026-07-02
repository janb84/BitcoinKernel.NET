
using System.Runtime.InteropServices;
using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Represents a transaction output (TxOut) in a Bitcoin transaction.
/// </summary>

public class TxOut : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal TxOut(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle;
        _ownsHandle = ownsHandle;
    }

    public TxOut(ScriptPubKey scriptPubKey, long amount)
    {
        IntPtr handle = NativeMethods.TransactionOutputCreate(scriptPubKey.Handle, amount);
        if (handle == IntPtr.Zero)
            throw new TransactionException("Failed to create transaction output");

        _handle = handle;
        _ownsHandle = true;
    }

    /// <summary>
    /// Creates a transaction output from a script pubkey and amount.
    /// </summary>
    public static TxOut Create(IntPtr scriptPubkeyPtr, long amount)
    {
        IntPtr handle = NativeMethods.TransactionOutputCreate(scriptPubkeyPtr, amount);
        if (handle == IntPtr.Zero)
            throw new TransactionException("Failed to create transaction output");

        return new TxOut(handle, ownsHandle: true);
    }

    public static TxOut Create(ScriptPubKey scriptPubKey, long amount)
    {
        return Create(scriptPubKey.Handle, amount);
    }

    /// <summary>
    /// Gets the native handle (for internal use).
    /// </summary>
    internal IntPtr Handle => _handle;

    /// <summary>
    /// Gets the amount (value) of this output in satoshis.
    /// </summary>
    public long Amount => NativeMethods.TransactionOutputGetAmount(_handle);

    /// <summary>
    /// Gets the script pubkey pointer for this output.
    /// </summary>
    /// <returns>Pointer to the script pubkey.</returns>
    public IntPtr GetScriptPubkeyPtr()
    {
        IntPtr scriptPtr = NativeMethods.TransactionOutputGetScriptPubkey(_handle);
        if (scriptPtr == IntPtr.Zero)
            throw new TransactionException("Failed to get script pubkey from output");

        return scriptPtr;
    }

    /// <summary>
    /// Gets the script pubkey of this output as a non-owning <see cref="ScriptPubKey"/>
    /// object whose lifetime is tied to this output.
    /// </summary>
    public ScriptPubKey GetScriptPubkey()
    {
        return new ScriptPubKey(GetScriptPubkeyPtr(), ownsHandle: false);
    }

    /// <summary>
    /// Gets the script pubkey of this output as a byte array.
    /// </summary>
    /// <returns>The script pubkey bytes.</returns>
    public byte[] GetScriptPubkeyBytes()
    {
        using var scriptPubkey = GetScriptPubkey();
        return scriptPubkey.ToBytes();
    }

    /// <summary>
    /// Creates a copy of this output.
    /// </summary>
    /// <returns>A new TxOut instance.</returns>
    public TxOut Copy()
    {
        IntPtr copyHandle = NativeMethods.TransactionOutputCopy(_handle);
        if (copyHandle == IntPtr.Zero)
            throw new TransactionException("Failed to copy transaction output");

        return new TxOut(copyHandle, ownsHandle: true);
    }

    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero && _ownsHandle)
        {
            NativeMethods.TransactionOutputDestroy(_handle);
            _handle = IntPtr.Zero;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~TxOut()
    {
        Dispose();
    }
}
