using BitcoinKernel.Core.Abstractions;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Core.TransactionValidation;

/// <summary>
/// Provides transaction validation functionality for consensus checks.
/// </summary>
public static class TransactionValidator
{
    /// <summary>
    /// Checks if a transaction is valid according to consensus rules.
    /// This is more efficient than CheckTransaction() when you only need a boolean result.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <returns>True if the transaction is valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when transaction is null.</exception>
    public static bool IsValid(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        return NativeMethods.CheckTransaction(transaction.Handle, out nint statePtr) == 1;
    }

    /// <summary>
    /// Performs context-free consensus checks on a transaction and returns detailed validation state.
    /// Use this when you need to know why a transaction is invalid.
    /// For simple validation, use IsValid() instead.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <returns>A TxValidationState containing the validation result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when transaction is null.</exception>
    /// <exception cref="TransactionException">Thrown when validation fails to execute.</exception>
    public static TxValidationState CheckTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        IntPtr statePtr;
        int result = NativeMethods.CheckTransaction(transaction.Handle, out statePtr);

        if (statePtr == IntPtr.Zero)
            throw new TransactionException("Failed to create validation state");

        return new TxValidationState(statePtr);
    }
}
