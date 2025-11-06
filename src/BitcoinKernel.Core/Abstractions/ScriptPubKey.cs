using System.Net;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core.Abstractions;

/// A single script pubkey containing spending conditions for a transaction output.
///
/// Script pubkeys can be created from raw script bytes or retrieved from existing
/// transaction outputs.
/// 

public sealed class ScriptPubKey : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal ScriptPubKey(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle;
        _ownsHandle = ownsHandle;
    }

    /// <summary>
    /// Creates a script pubkey from raw script bytes.
    /// </summary>
    /// <param name="scriptBytes">The raw script bytes.</param>
    /// <returns>A new ScriptPubKey instance.</returns>
    public static ScriptPubKey FromBytes(byte[] scriptBytes)
    {
        ArgumentNullException.ThrowIfNull(scriptBytes);

        unsafe
        {
            fixed (byte* ptr = scriptBytes)
            {
                IntPtr handle = NativeMethods.ScriptPubkeyCreate((IntPtr)ptr, (nuint)scriptBytes.Length);
                if (handle == IntPtr.Zero)
                    throw new TransactionException("Failed to create script pubkey from bytes");

                return new ScriptPubKey(handle, ownsHandle: true);
            }
        }
    }

    public static ScriptPubKey FromHex(string hexString)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(hexString);

        var bytes = Convert.FromHexString(hexString);
        return FromBytes(bytes);
    }



    internal IntPtr Handle => _handle;

    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            if (_ownsHandle)
            {
                NativeMethods.ScriptPubkeyDestroy(_handle);
            }
            _handle = IntPtr.Zero;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~ScriptPubKey()
    {
        Dispose();
    }
}