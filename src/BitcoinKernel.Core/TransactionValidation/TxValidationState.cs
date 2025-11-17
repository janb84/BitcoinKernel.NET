using BitcoinKernel.Interop;
using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Core.TransactionValidation;

/// <summary>
/// Represents the validation state of a transaction after consensus checks.
/// </summary>
public sealed class TxValidationState : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    internal TxValidationState(IntPtr handle)
    {
        _handle = handle != IntPtr.Zero
            ? handle
            : throw new ArgumentException("Invalid validation state handle", nameof(handle));
    }

    /// <summary>
    /// Gets the validation mode (VALID, INVALID, or INTERNAL_ERROR).
    /// </summary>
    public ValidationMode Mode
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.TxValidationStateGetValidationMode(_handle);
        }
    }

    /// <summary>
    /// Gets the detailed validation result indicating why the transaction was invalid.
    /// </summary>
    public TxValidationResult Result
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.TxValidationStateGetTxValidationResult(_handle);
        }
    }

    /// <summary>
    /// Returns true if the transaction is valid.
    /// </summary>
    public bool IsValid => Mode == ValidationMode.VALID;

    /// <summary>
    /// Returns true if the transaction is invalid.
    /// </summary>
    public bool IsInvalid => Mode == ValidationMode.INVALID;

    /// <summary>
    /// Returns true if an internal error occurred during validation.
    /// </summary>
    public bool IsError => Mode == ValidationMode.INTERNAL_ERROR;

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TxValidationState));
    }

    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            NativeMethods.TxValidationStateDestroy(_handle);
            _handle = IntPtr.Zero;
            _disposed = true;
        }
    }
}
