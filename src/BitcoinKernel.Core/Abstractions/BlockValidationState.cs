using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;
using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Core.Abstractions;

/// <summary>
/// Represents the validation state of a block.
/// </summary>
public sealed class BlockValidationState : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new block validation state.
    /// </summary>
    public BlockValidationState()
    {
        _handle = NativeMethods.BlockValidationStateCreate();
        if (_handle == IntPtr.Zero)
        {
            throw new BlockException("Failed to create block validation state");
        }
    }

    internal BlockValidationState(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid block validation state handle", nameof(handle));
        }

        _handle = handle;
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
    /// Gets the validation mode (valid, invalid, or internal error).
    /// </summary>
    public ValidationMode ValidationMode
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.BlockValidationStateGetValidationMode(_handle);
        }
    }

    /// <summary>
    /// Gets the block validation result (detailed error reason if invalid).
    /// </summary>
    public Interop.Enums.BlockValidationResult ValidationResult
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.BlockValidationStateGetBlockValidationResult(_handle);
        }
    }

    /// <summary>
    /// Creates a copy of this block validation state.
    /// </summary>
    public BlockValidationState Copy()
    {
        ThrowIfDisposed();
        var copyPtr = NativeMethods.BlockValidationStateCopy(_handle);
        if (copyPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to copy block validation state");
        }
        return new BlockValidationState(copyPtr);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BlockValidationState));
    }


    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.BlockValidationStateDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~BlockValidationState() => Dispose();
}
