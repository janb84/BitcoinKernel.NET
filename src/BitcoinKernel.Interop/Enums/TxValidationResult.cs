namespace BitcoinKernel.Interop.Enums
{
    /// <summary>
    /// A granular "reason" why a transaction was invalid.
    /// Mirrors consensus/validation.h: enum class TxValidationResult.
    /// </summary>
    public enum TxValidationResult : uint
    {
        UNSET = 0,
        CONSENSUS = 1,
        INPUTS_NOT_STANDARD = 2,
        NOT_STANDARD = 3,
        MISSING_INPUTS = 4,
        PREMATURE_SPEND = 5,
        WITNESS_MUTATED = 6,
        WITNESS_STRIPPED = 7,
        CONFLICT = 8,
        MEMPOOL_POLICY = 9,
        NO_MEMPOOL = 10,
        RECONSIDERABLE = 11,
        UNKNOWN = 12
    }
}
