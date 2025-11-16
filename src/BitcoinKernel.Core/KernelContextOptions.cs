using BitcoinKernel.Core.Chain;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core;
/// <summary>
/// Options for creating a kernel context.
/// </summary>
public sealed class KernelContextOptions : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    public KernelContextOptions()
    {
        _handle = NativeMethods.ContextOptionsCreate();
        if (_handle == IntPtr.Zero)
            throw new KernelException("Failed to create context options");
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
    /// Sets the chain parameters for this context.
    /// </summary>
    public KernelContextOptions SetChainParams(ChainParameters chainParams)
    {
        ThrowIfDisposed();

        if (chainParams == null)
            throw new ArgumentNullException(nameof(chainParams));

        var chainParamsHandle = chainParams.Handle;
        if (chainParamsHandle == IntPtr.Zero)
            throw new KernelException("Chain parameters handle is invalid (null)");

        if (_handle == IntPtr.Zero)
            throw new KernelException("Context options handle is invalid (null)");

        NativeMethods.ContextOptionsSetChainParams(_handle, chainParamsHandle);

        return this;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(KernelContextOptions));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.ContextOptionsDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~KernelContextOptions() => Dispose();
}