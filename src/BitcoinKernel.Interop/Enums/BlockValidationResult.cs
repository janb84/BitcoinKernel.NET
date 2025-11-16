namespace BitcoinKernel.Interop.Enums;

/// <summary>
/// A granular "reason" why a block was invalid.
/// </summary>
public enum BlockValidationResult : uint
{
    /// <summary>
    /// Initial value. Block has not yet been rejected.
    /// </summary>
    UNSET = 0,

    /// <summary>
    /// Invalid by consensus rules (excluding any below reasons).
    /// </summary>
    CONSENSUS = 1,

    /// <summary>
    /// This block was cached as being invalid and we didn't store the reason why.
    /// </summary>
    CACHED_INVALID = 2,

    /// <summary>
    /// Invalid proof of work or time too old.
    /// </summary>
    INVALID_HEADER = 3,

    /// <summary>
    /// The block's data didn't match the data committed to by the PoW.
    /// </summary>
    MUTATED = 4,

    /// <summary>
    /// We don't have the previous block the checked one is built on.
    /// </summary>
    MISSING_PREV = 5,

    /// <summary>
    /// A block this one builds on is invalid.
    /// </summary>
    INVALID_PREV = 6,

    /// <summary>
    /// Block timestamp was > 2 hours in the future (or our clock is bad).
    /// </summary>
    TIME_FUTURE = 7,

    /// <summary>
    /// The block header may be on a too-little-work chain.
    /// </summary>
    HEADER_LOW_WORK = 8
}
