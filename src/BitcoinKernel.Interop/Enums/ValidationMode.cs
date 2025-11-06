namespace BitcoinKernel.Interop.Enums;

/// <summary>
/// Whether a validated data structure is valid, invalid, or an error was
/// encountered during processing.
/// </summary>
public enum ValidationMode : byte
{
    /// <summary>
    /// The validated data structure is valid.
    /// </summary>
    VALID = 0,
    
    /// <summary>
    /// The validated data structure is invalid.
    /// </summary>
    INVALID = 1,
    
    /// <summary>
    /// An internal error was encountered during validation.
    /// </summary>
    INTERNAL_ERROR = 2
}
