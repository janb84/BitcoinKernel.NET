
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core.Abstractions;

/// <summary>
/// Represents a block in the blockchain.
/// </summary>
public sealed class Block : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    internal Block(IntPtr handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Creates a block from raw serialized data.
    /// </summary>
    public static Block FromBytes(byte[] rawBlockData)
    {
        ArgumentNullException.ThrowIfNull(rawBlockData,nameof(rawBlockData));
        if (rawBlockData.Length == 0) throw new ArgumentException("Block data cannot be empty", nameof(rawBlockData));

        IntPtr blockPtr = NativeMethods.BlockCreate(rawBlockData, (UIntPtr)rawBlockData.Length);

        if (blockPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to create block from raw data");
        }

        return new Block(blockPtr);
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
    /// Gets the number of transactions in this block.
    /// </summary>
    public int TransactionCount
    {
        get
        {
            ThrowIfDisposed();
            return (int)NativeMethods.BlockCountTransactions(_handle);
        }
    }

    /// <summary>
    /// Gets the block hash.
    /// </summary>
    public byte[] GetHash()
    {
        ThrowIfDisposed();
        var hashPtr = NativeMethods.BlockGetHash(_handle);
        if (hashPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to get block hash");
        }

        using var blockHash = new BlockHash(hashPtr);
        return blockHash.ToBytes();
    }

    /// <summary>
    /// Serializes the block to bytes.
    /// </summary>
    public byte[] ToBytes()
    {
        ThrowIfDisposed();
        var bytes = new System.Collections.Generic.List<byte>();

        NativeMethods.BlockToBytes(_handle, (data, size, userData) =>
        {
            unsafe
            {
                var span = new ReadOnlySpan<byte>((byte*)data, (int)size);
                foreach (var b in span)
                {
                    bytes.Add(b);
                }
            }
            return 0;
        }, IntPtr.Zero);

        return bytes.ToArray();
    }

    /// <summary>
    /// Gets a transaction by index.
    /// </summary>
    /// <param name="index">The zero-based index of the transaction.</param>
    /// <returns>The transaction at the specified index, or null if the index is out of range.</returns>
    public Transaction? GetTransaction(int index)
    {
        ThrowIfDisposed();
        
        if (index < 0 || index >= TransactionCount)
            return null;

        var txPtr = NativeMethods.BlockGetTransactionAt(_handle, (nuint)index);
        if (txPtr == IntPtr.Zero)
            return null;

        return new Transaction(txPtr, ownsHandle: false);
    }

    /// <summary>
    /// Enumerates all transactions in the block.
    /// </summary>
    /// <returns>An enumerable of transactions in the block.</returns>
    public IEnumerable<Transaction> GetTransactions()
    {
        ThrowIfDisposed();
        
        int count = TransactionCount;
        for (int i = 0; i < count; i++)
        {
            var tx = GetTransaction(i);
            if (tx != null)
            {
                yield return tx;
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Block));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.BlockDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~Block() => Dispose();
}