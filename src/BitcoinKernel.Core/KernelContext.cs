using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core;

/// <summary>
/// Represents a Bitcoin Kernel context - the main entry point for the library.
/// </summary>
public sealed class KernelContext : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new kernel context with the specified options.
    /// </summary>
    public KernelContext(KernelContextOptions? options = null)
    {
        IntPtr optionsHandle = options?.Handle ?? IntPtr.Zero;
        _handle = NativeMethods.ContextCreate(optionsHandle);

        if (_handle == IntPtr.Zero)
            throw new KernelException("Failed to create kernel context");
    }

    /// <summary>
    /// Gets the native handle.
    /// </summary>
    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    /// <summary>
    /// Interrupts long-running operations associated with this context.
    /// </summary>
    public void Interrupt()
    {
        ThrowIfDisposed();
        NativeMethods.ContextInterrupt(_handle);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(KernelContext));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.ContextDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~KernelContext() => Dispose();
}