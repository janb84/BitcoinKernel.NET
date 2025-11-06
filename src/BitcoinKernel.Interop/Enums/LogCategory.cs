namespace BitcoinKernel.Interop.Enums
{
    /// <summary>
    /// A collection of logging categories that may be encountered by kernel code.
    /// </summary>
    public enum LogCategory : byte
    {
        /// <summary>
        /// All categories.
        /// </summary>
        All = 0,

        /// <summary>
        /// Benchmark logging.
        /// </summary>
        Bench = 1,

        /// <summary>
        /// Block storage operations.
        /// </summary>
        BlockStorage = 2,

        /// <summary>
        /// Coin database operations.
        /// </summary>
        CoinDb = 3,

        /// <summary>
        /// LevelDB operations.
        /// </summary>
        LevelDb = 4,

        /// <summary>
        /// Memory pool operations.
        /// </summary>
        Mempool = 5,

        /// <summary>
        /// Pruning operations.
        /// </summary>
        Prune = 6,

        /// <summary>
        /// Random number generation.
        /// </summary>
        Rand = 7,

        /// <summary>
        /// Reindexing operations.
        /// </summary>
        Reindex = 8,

        /// <summary>
        /// Validation operations.
        /// </summary>
        Validation = 9,

        /// <summary>
        /// Kernel operations.
        /// </summary>
        Kernel = 10
    }
}
