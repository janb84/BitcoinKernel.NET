namespace BitcoinKernel.Core.Abstractions
{
    using BitcoinKernel.Interop;
    using System;

    /// <summary>
    /// Represents a coin (unspent transaction output) with confirmation height information.
    /// </summary>
    public class Coin : IDisposable
    {
        private IntPtr _handle;
        private bool _disposed;
        private readonly bool _ownsHandle;

        internal Coin(IntPtr handle, bool ownsHandle = true)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException("Invalid coin handle", nameof(handle));
            }

            _handle = handle;
            _ownsHandle = ownsHandle;
        }

        /// <summary>
        /// Gets the block height where the transaction that created this coin was included.
        /// </summary>
        public uint ConfirmationHeight
        {
            get
            {
                ThrowIfDisposed();
                return NativeMethods.CoinConfirmationHeight(_handle);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this coin is from a coinbase transaction.
        /// </summary>
        public bool IsCoinbase
        {
            get
            {
                ThrowIfDisposed();
                return NativeMethods.CoinIsCoinbase(_handle) != 0;
            }
        }

        /// <summary>
        /// Gets the transaction output of this coin.
        /// The returned object is only valid for the lifetime of this Coin.
        /// </summary>
        public TxOut GetOutput()
        {
            ThrowIfDisposed();
            var outputPtr = NativeMethods.CoinGetOutput(_handle);
            if (outputPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to get coin output");
            }

            // Don't own the handle - it's only valid for the lifetime of the Coin
            return new TxOut(outputPtr, ownsHandle: false);
        }

        /// <summary>
        /// Creates a copy of this coin.
        /// </summary>
        public Coin Copy()
        {
            ThrowIfDisposed();
            var copiedHandle = NativeMethods.CoinCopy(_handle);
            if (copiedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to copy coin");
            }

            return new Coin(copiedHandle, ownsHandle: true);
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
                throw new ObjectDisposedException(nameof(Coin));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_ownsHandle && _handle != IntPtr.Zero)
                {
                    NativeMethods.CoinDestroy(_handle);
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

        ~Coin()
        {
            Dispose(false);
        }
    }
}
