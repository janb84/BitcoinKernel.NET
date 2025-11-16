using BitcoinKernel.Interop;
using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Core.Chain;

public sealed class ChainParameters : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    public ChainParameters(ChainType chainType)
    {
        _handle = NativeMethods.ChainParametersCreate(chainType);

        if (_handle == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to create chain parameters for chain type: {chainType}. The native library may not be loaded correctly.");
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
            throw new ObjectDisposedException(nameof(ChainParameters));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.ChainParametersDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~ChainParameters() => Dispose();
}