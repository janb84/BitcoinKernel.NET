namespace BitcoinKernel.Core.Abstractions
{
    using BitcoinKernel.Interop;
    using System;

    /// <summary>
    /// Represents the spent outputs for all transactions in a block.
    /// </summary>
    public class BlockSpentOutputs : IDisposable
    {
        private IntPtr _handle;
        private bool _disposed;

        internal BlockSpentOutputs(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException("Invalid block spent outputs handle", nameof(handle));
            }

            _handle = handle;
        }

        /// <summary>
        /// Gets the number of transactions with spent outputs in this block.
        /// This excludes the coinbase transaction.
        /// </summary>
        public int Count
        {
            get
            {
                ThrowIfDisposed();
                return (int)NativeMethods.BlockSpentOutputsCount(_handle);
            }
        }

        /// <summary>
        /// Gets the spent outputs for a transaction at the specified index.
        /// The returned object is only valid for the lifetime of this BlockSpentOutputs.
        /// </summary>
        /// <param name="transactionIndex">The zero-based index of the transaction (excluding coinbase).</param>
        /// <returns>A TransactionSpentOutputs object for the specified transaction.</returns>
        public TransactionSpentOutputs GetTransactionSpentOutputs(int transactionIndex)
        {
            ThrowIfDisposed();

            if (transactionIndex < 0 || transactionIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(transactionIndex));
            }

            var txSpentOutputsPtr = NativeMethods.BlockSpentOutputsGetTransactionSpentOutputsAt(
                _handle, (nuint)transactionIndex);

            if (txSpentOutputsPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Failed to get transaction spent outputs at index {transactionIndex}");
            }

            // Don't own the handle - it's only valid for the lifetime of BlockSpentOutputs
            return new TransactionSpentOutputs(txSpentOutputsPtr, ownsHandle: false);
        }

        /// <summary>
        /// Enumerates all transaction spent outputs in the block.
        /// </summary>
        /// <returns>An enumerable of TransactionSpentOutputs for each transaction (excluding coinbase).</returns>
        public IEnumerable<TransactionSpentOutputs> EnumerateTransactionSpentOutputs()
        {
            ThrowIfDisposed();

            int count = Count;
            for (int i = 0; i < count; i++)
            {
                yield return GetTransactionSpentOutputs(i);
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
                throw new ObjectDisposedException(nameof(BlockSpentOutputs));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_handle != IntPtr.Zero)
                {
                    NativeMethods.BlockSpentOutputsDestroy(_handle);
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

        ~BlockSpentOutputs()
        {
            Dispose(false);
        }
    }
}
