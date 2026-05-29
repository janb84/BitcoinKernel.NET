namespace BitcoinKernel.Interop.Enums;

/// <summary>
/// A granular "reason" why a transaction was invalid.
/// </summary>
public enum TxValidationResult : uint
{
    /// <summary>
    /// Initial value. Tx has not yet been rejected.
    /// </summary>
    UNSET = 0,

    /// <summary>
    /// Invalid by consensus rules.
    /// </summary>
    CONSENSUS = 1,

    /// <summary>
    /// Inputs (covered by txid) failed policy rules.
    /// </summary>
    INPUTS_NOT_STANDARD = 2,

    /// <summary>
    /// Otherwise didn't meet local policy rules.
    /// </summary>
    NOT_STANDARD = 3,

    /// <summary>
    /// Transaction was missing some of its inputs.
    /// </summary>
    MISSING_INPUTS = 4,

    /// <summary>
    /// Transaction spends a coinbase too early, or violates locktime/sequence locks.
    /// </summary>
    PREMATURE_SPEND = 5,

    /// <summary>
    /// Witness may have been malleated or is prior to SegWit activation.
    /// </summary>
    WITNESS_MUTATED = 6,

    /// <summary>
    /// Transaction is missing a witness.
    /// </summary>
    WITNESS_STRIPPED = 7,

    /// <summary>
    /// Tx already in mempool or conflicts with a tx in the chain.
    /// </summary>
    CONFLICT = 8,

    /// <summary>
    /// Violated mempool's fee/size/descendant/RBF/etc limits.
    /// </summary>
    MEMPOOL_POLICY = 9,

    /// <summary>
    /// This node does not have a mempool so can't validate the transaction.
    /// </summary>
    NO_MEMPOOL = 10,

    /// <summary>
    /// Fails some policy, but might be acceptable if submitted in a (different) package.
    /// </summary>
    RECONSIDERABLE = 11,

    /// <summary>
    /// Transaction was not validated because package failed.
    /// </summary>
    UNKNOWN = 12
}
