using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;
using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Chain;

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

    private ChainParameters(IntPtr handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Creates signet chain parameters with a custom challenge script.
    /// Blocks must satisfy the challenge to be valid.
    /// </summary>
    /// <param name="challenge">The raw signet challenge script bytes.</param>
    public static ChainParameters CreateSignet(byte[] challenge)
    {
        ArgumentNullException.ThrowIfNull(challenge);

        var handle = NativeMethods.ChainParametersCreateSignet(challenge, (nuint)challenge.Length);
        if (handle == IntPtr.Zero)
            throw new ChainParametersException(ChainType.SIGNET, "Failed to create signet chain parameters from the given challenge.");

        return new ChainParameters(handle);
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