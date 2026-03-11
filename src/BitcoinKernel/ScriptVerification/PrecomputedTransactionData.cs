using BitcoinKernel.Primitives;
using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.ScriptVerification;

/// <summary>
/// Holds precomputed transaction data used to accelerate repeated script verification
/// across multiple inputs of the same transaction.
/// Required when <c>btck_ScriptVerificationFlags_TAPROOT</c> is set.
/// </summary>
public sealed class PrecomputedTransactionData : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates precomputed transaction data for the given transaction.
    /// </summary>
    /// <param name="transaction">The transaction being verified.</param>
    /// <param name="spentOutputs">
    /// The outputs being spent by the transaction inputs. Required when the TAPROOT
    /// verification flag is set. Must match the transaction input count if provided.
    /// </param>
    public PrecomputedTransactionData(Transaction transaction, IReadOnlyList<TxOut>? spentOutputs = null)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        IntPtr[] handles = spentOutputs is { Count: > 0 }
            ? spentOutputs.Select(o => o.Handle).ToArray()
            : Array.Empty<IntPtr>();

        _handle = NativeMethods.PrecomputedTransactionDataCreate(
            transaction.Handle,
            handles,
            (nuint)handles.Length);

        if (_handle == IntPtr.Zero)
            throw new KernelException("Failed to create precomputed transaction data");
    }

    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            NativeMethods.PrecomputedTransactionDataDestroy(_handle);
            _handle = IntPtr.Zero;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~PrecomputedTransactionData() => Dispose();

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PrecomputedTransactionData));
    }
}
