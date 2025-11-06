namespace BitcoinKernel.Core.Abstractions;

using BitcoinKernel.Interop;
using System;

/// <summary>
/// Represents the spent outputs (coins) of a transaction.
/// </summary>
public class TransactionSpentOutputs : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal TransactionSpentOutputs(IntPtr handle, bool ownsHandle = false)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid transaction spent outputs handle", nameof(handle));
        }

        _handle = handle;
        _ownsHandle = ownsHandle;
    }


    public int Count
    {
        get
        {
            ThrowIfDisposed();
            return (int)NativeMethods.TransactionSpentOutputsCount(_handle);
        }
    }

    public Coin GetCoin(int index)
    {
        ThrowIfDisposed();

        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var coinPtr = NativeMethods.TransactionSpentOutputsGetCoinAt(_handle, (nuint)index);
        if (coinPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to get coin at index {index}");
        }

        return new Coin(coinPtr, ownsHandle: false);
    }

    /// <summary>
    /// Enumerates all coins in the transaction spent outputs.
    /// </summary>
    /// <returns>An enumerable of Coin objects.</returns>
    public IEnumerable<Coin> EnumerateCoins()
    {
        ThrowIfDisposed();
        
        int count = Count;
        for (int i = 0; i < count; i++)
        {
            yield return GetCoin(i);
        }
    }

    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TransactionSpentOutputs));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_ownsHandle && _handle != IntPtr.Zero)
            {
                NativeMethods.TransactionSpentOutputsDestroy(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~TransactionSpentOutputs()
    {
        Dispose(false);
    }
}

