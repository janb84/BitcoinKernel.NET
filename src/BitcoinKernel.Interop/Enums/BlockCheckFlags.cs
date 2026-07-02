namespace BitcoinKernel.Interop.Enums;

/// <summary>
/// Flags controlling optional context-free block checks performed by
/// btck_block_check. The base checks (size limits, coinbase structure,
/// transaction checks, sigop limits) always run; these flags toggle the
/// optional proof-of-work and merkle-root checks.
/// </summary>
[Flags]
public enum BlockCheckFlags : uint
{
    /// <summary>
    /// Run the base context-free block checks only.
    /// </summary>
    Base = 0,

    /// <summary>
    /// Run CheckProofOfWork via CheckBlockHeader.
    /// </summary>
    Pow = 1U << 0,

    /// <summary>
    /// Verify merkle root (and mutation detection).
    /// </summary>
    Merkle = 1U << 1,

    /// <summary>
    /// Enable all optional context-free block checks.
    /// </summary>
    All = Pow | Merkle
}
