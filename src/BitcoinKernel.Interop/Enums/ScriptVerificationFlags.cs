namespace BitcoinKernel.Interop.Enums
{
    /// <summary>
    /// Script verification flags that may be composed with each other.
    /// </summary>
    [Flags]
    public enum ScriptVerificationFlags : uint
    {
        /// <summary>
        /// No script verification flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Evaluate P2SH (BIP16) subscripts.
        /// </summary>
        P2SH = 1U << 0,

        /// <summary>
        /// Enforce strict DER (BIP66) compliance.
        /// </summary>
        DerSig = 1U << 2,

        /// <summary>
        /// Enforce NULLDUMMY (BIP147).
        /// </summary>
        NullDummy = 1U << 4,

        /// <summary>
        /// Enable CHECKLOCKTIMEVERIFY (BIP65).
        /// </summary>
        CheckLockTimeVerify = 1U << 9,

        /// <summary>
        /// Enable CHECKSEQUENCEVERIFY (BIP112).
        /// </summary>
        CheckSequenceVerify = 1U << 10,

        /// <summary>
        /// Enable WITNESS (BIP141).
        /// </summary>
        Witness = 1U << 11,

        /// <summary>
        /// Enable TAPROOT (BIPs 341 & 342).
        /// </summary>
        Taproot = 1U << 17,

        /// <summary>
        /// All standard script verification flags.
        /// </summary>
        All = P2SH | DerSig | NullDummy | CheckLockTimeVerify | CheckSequenceVerify | Witness | Taproot,

        /// <summary>
        ///  All script verification flags pre taproot(P2SH + Witness).
        /// </summary>
        AllPreTaproot = P2SH | DerSig | NullDummy | CheckLockTimeVerify | CheckSequenceVerify | Witness
    }
}