using System;
using System.Runtime.InteropServices;
using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Managed wrapper for Bitcoin transactions with automatic memory management.
/// Provides high-level APIs for transaction operations.
/// </summary>
public sealed class Transaction : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    /// <summary>
    /// Creates a transaction from serialized data.
    /// </summary>
    /// <param name="rawTransaction">The serialized transaction bytes.</param>
    /// <exception cref="ArgumentNullException">Thrown when rawTransaction is null.</exception>
    /// <exception cref="TransactionException">Thrown when transaction creation fails.</exception>
    public Transaction(byte[] rawTransaction)
    {
        ArgumentNullException.ThrowIfNull(rawTransaction);

        unsafe
        {
            fixed (byte* ptr = rawTransaction)
            {
                _handle = NativeMethods.TransactionCreate((IntPtr)ptr, (nuint)rawTransaction.Length);
                if (_handle == IntPtr.Zero)
                    throw new TransactionException("Failed to create transaction from serialized data");
            }
        }
        _ownsHandle = true;
    }

    public static Transaction FromBytes(byte[] rawTransaction)
    {
        return new Transaction(rawTransaction);
    }

    /// <summary>
    /// Creates a transaction from a hex string.
    /// </summary>
    /// <param name="hexString">The transaction as a hex string.</param>
    /// <exception cref="ArgumentNullException">Thrown when hexString is null or empty.</exception>
    /// <exception cref="TransactionException">Thrown when transaction creation fails.</exception>
    public Transaction(string hexString)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(hexString);

        var bytes = Convert.FromHexString(hexString);
        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                _handle = NativeMethods.TransactionCreate((IntPtr)ptr, (nuint)bytes.Length);
                if (_handle == IntPtr.Zero)
                    throw new TransactionException("Failed to create transaction from hex string");
            }
        }
        _ownsHandle = true;
    }

    public static Transaction FromHex(string hexString)
    {
        return new Transaction(hexString);
    }


    internal Transaction(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle;
        _ownsHandle = ownsHandle;
    }

    /// <summary>
    /// Gets the number of inputs in this transaction.
    /// </summary>
    /// <returns>The number of inputs as an integer.</returns>
    public int InputCount => (int)NativeMethods.TransactionCountInputs(_handle);

    /// <summary>
    /// Gets the number of outputs in this transaction.
    /// </summary>
    /// <returns>The number of outputs as an integer.</returns>
    public int OutputCount => (int)NativeMethods.TransactionCountOutputs(_handle);

    /// <summary>
    /// Gets the transaction ID (txid) as a non-owning <see cref="Txid"/> object whose
    /// lifetime is tied to this transaction.
    /// </summary>
    public Txid GetTxid()
    {
        IntPtr txidPtr = NativeMethods.TransactionGetTxid(_handle);
        if (txidPtr == IntPtr.Zero)
            throw new TransactionException("Failed to get transaction ID");

        return new Txid(txidPtr, ownsHandle: false);
    }

    /// <summary>
    /// Gets the transaction ID (txid) as a 32-byte array.
    /// </summary>
    /// <returns>The transaction ID as a byte array.</returns>
    public byte[] GetTxidBytes()
    {
        using var txid = GetTxid();
        return txid.ToBytes();
    }

    /// <summary>
    /// Gets the transaction ID (txid) as a hex string.
    /// </summary>
    /// <returns>The transaction ID as a hex string.</returns>
    public string GetTxidHex()
    {
        return Convert.ToHexString(GetTxidBytes()).ToLowerInvariant();
    }

    /// <summary>
    /// Serializes this transaction to bytes.
    /// </summary>
    public byte[] ToBytes()
    {
        var bytes = new List<byte>();
        NativeMethods.WriteBytes writer = (data, len, _) =>
        {
            var buffer = new byte[len];
            Marshal.Copy(data, buffer, 0, (int)len);
            bytes.AddRange(buffer);
            return 0;
        };

        int result = NativeMethods.TransactionToBytes(_handle, writer, IntPtr.Zero);
        if (result != 0)
            throw new TransactionException("Failed to serialize transaction");

        return bytes.ToArray();
    }

    /// <summary>
    /// Gets the transaction input at the specified index as a non-owning
    /// <see cref="TransactionInput"/> whose lifetime is tied to this transaction.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    /// <exception cref="TransactionException">Thrown when input retrieval fails.</exception>
    public TransactionInput GetInputAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, InputCount);

        IntPtr inputPtr = NativeMethods.TransactionGetInputAt(_handle, (nuint)index);
        if (inputPtr == IntPtr.Zero)
            throw new TransactionException($"Failed to get input at index {index}");

        return new TransactionInput(inputPtr, ownsHandle: false);
    }

    /// <returns>The TxOut at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    /// <exception cref="TransactionException">Thrown when output retrieval fails.</exception>
    public TxOut GetOutputAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, OutputCount);

        IntPtr outputPtr = NativeMethods.TransactionGetOutputAt(_handle, (nuint)index);
        if (outputPtr == IntPtr.Zero)
            throw new TransactionException($"Failed to get output at index {index}");

        return new TxOut(outputPtr, ownsHandle: false);
    }

    /// <summary>
    /// Creates a copy of this transaction.
    /// </summary>
    /// <returns>A new Transaction instance.</returns>
    public Transaction Copy()
    {
        IntPtr copyHandle = NativeMethods.TransactionCopy(_handle);
        if (copyHandle == IntPtr.Zero)
            throw new TransactionException("Failed to copy transaction");

        return new Transaction(copyHandle);
    }

    internal IntPtr Handle => _handle;

    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            if (_ownsHandle)
            {
                NativeMethods.TransactionDestroy(_handle);
            }
            _handle = IntPtr.Zero;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~Transaction()
    {
        Dispose();
    }
}

