namespace BitcoinKernel.Interop.Enums
{
    /// <summary>
    /// Status codes that may be returned by script verification operations.
    /// </summary>
    public enum ScriptVerifyStatus : byte
    {
        /// <summary>
        /// Script verified successfully.
        /// </summary>
        OK = 0,
        
        /// <summary>
        /// The flags were combined in an invalid way.
        /// </summary>
        ERROR_INVALID_FLAGS_COMBINATION = 1,
        
        /// <summary>
        /// The taproot flag was set, so valid spent_outputs have to be provided.
        /// </summary>
        ERROR_SPENT_OUTPUTS_REQUIRED = 2,
        
        /// <summary>
        /// The input index is out of bounds for the transaction.
        /// </summary>
        ERROR_TX_INPUT_INDEX = 3,
        
        /// <summary>
        /// The number of spent outputs doesn't match the number of transaction inputs.
        /// </summary>
        ERROR_SPENT_OUTPUTS_MISMATCH = 4,
        
        /// <summary>
        /// The verification flags value is invalid.
        /// </summary>
        ERROR_INVALID_FLAGS = 5
    }
}