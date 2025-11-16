using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Core.BlockProcessing;

/// <summary>
/// Result of a block validation operation.
/// </summary>
public sealed class BlockValidationResult
{
    /// <summary>
    /// Whether the block is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// The validation mode (valid, invalid, or internal error).
    /// </summary>
    public ValidationMode Mode { get; }

    /// <summary>
    /// Error code if validation failed.
    /// </summary>
    public int? ErrorCode { get; }

    /// <summary>
    /// Error message if available.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Creates a new validation result.
    /// </summary>
    public BlockValidationResult(bool isValid, ValidationMode mode, int? errorCode = null, string? errorMessage = null)
    {
        IsValid = isValid;
        Mode = mode;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}