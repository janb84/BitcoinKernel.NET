using BitcoinKernel.Core;
using BitcoinKernel.Core.Abstractions;
using BitcoinKernel.Core.BlockProcessing;
using BitcoinKernel.Core.Chain;
using BitcoinKernel.Core.ScriptVerification;
using BitcoinKernel.Interop.Enums;
using System.IO;


namespace BitcoinKernel
{
    /// <summary>
    /// Simplified facade for common Bitcoin Kernel operations.
    /// Provides a fluent, easy-to-use API for typical scenarios.
    /// </summary>
    public sealed class KernelLibrary : IDisposable
    {
        private readonly KernelContext _context;
        private readonly ChainParameters _chainParams;
        private ChainstateManager? _chainstateManager;
        private LoggingConnection? _loggingConnection;
        private BlockProcessor? _blockProcessor;
        private bool _disposed;

        /// <summary>
        /// Gets the block processor for validation and processing operations.
        /// </summary>
        private BlockProcessor Processor
        {
            get
            {
                ThrowIfDisposed();
                return _blockProcessor ??= new BlockProcessor(Chainstate);
            }
        }

        private KernelLibrary(
            KernelContext context,
            ChainParameters chainParams)
        {
            _context = context;
            _chainParams = chainParams;
        }

        /// <summary>
        /// Creates a new kernel library instance with fluent builder.
        /// </summary>
        public static KernelBuilder Create() => new KernelBuilder();

        /// <summary>
        /// Gets the active chainstate manager (if initialized).
        /// </summary>
        public ChainstateManager Chainstate
        {
            get
            {
                ThrowIfDisposed();
                if (_chainstateManager == null)
                    throw new InvalidOperationException("Chainstate not initialized. Call InitializeChainstate() first.");
                return _chainstateManager;
            }
        }

        /// <summary>
        /// Initializes the chainstate manager with the specified data directory.
        /// Internal use only - chainstate is automatically initialized by the builder.
        /// </summary>
        internal KernelLibrary InitializeChainstate(
            string dataDirectory,
            string? blocksDirectory = null,
            int workerThreads = 4)
        {
            ThrowIfDisposed();

            // Use dataDirectory/blocks if blocks directory not specified
            string blocksDir = blocksDirectory ?? System.IO.Path.Combine(dataDirectory, "blocks");

            var options = new ChainstateManagerOptions(_context, dataDirectory, blocksDir)
                .SetWorkerThreads(workerThreads);

            _chainstateManager = new ChainstateManager(_context, _chainParams, options);

            // Reset block processor when chainstate changes
            _blockProcessor = null;

            return this;
        }


        public KernelLibrary EnableLogging(Action<string, string, int> callback)
        {
            ThrowIfDisposed();
            _loggingConnection?.Dispose();
            _loggingConnection = new LoggingConnection(callback);
            return this;
        }

        /// <summary>
        /// Processes a block through validation and potentially adds it to the chain.
        /// </summary>
        /// <param name="blockData">The serialized block data.</param>
        /// <returns>True if the block was successfully processed and was new.</returns>
        public bool ProcessBlock(byte[] blockData)
        {
            ThrowIfDisposed();
            if (blockData == null) throw new ArgumentNullException(nameof(blockData));
            if (blockData.Length == 0) throw new ArgumentException("Block data cannot be empty", nameof(blockData));

            var result = Processor.ProcessBlock(blockData);
            return result.Success && result.IsNewBlock;
        }

        /// <summary>
        /// Processes a block from hex string through validation and potentially adds it to the chain.
        /// </summary>
        /// <param name="blockHex">The block data as a hex string.</param>
        /// <returns>True if the block was successfully processed and was new.</returns>
        public bool ProcessBlock(string blockHex)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(blockHex))
                throw new ArgumentException("Block hex cannot be null or empty", nameof(blockHex));

            byte[] blockData = Convert.FromHexString(blockHex);
            return ProcessBlock(blockData);
        }

        /// <summary>
        /// Verifies a script pubkey against a transaction input.
        /// This is a high-level convenience method that handles object creation from hex strings.
        /// </summary>
        /// <param name="scriptPubKeyHex">The hex representation of the output script to verify against.</param>
        /// <param name="amount">The amount of the output being spent.</param>
        /// <param name="transactionHex">The hex representation of the transaction containing the input to verify.</param>
        /// <param name="inputIndex">The index of the transaction input to verify.</param>
        /// <param name="spentOutputsHex">A list of hex strings, where each string is a spent output.</param>
        /// <param name="flags">Script verification flags.</param>
        /// <returns>True if the script is valid, otherwise false.</returns>
        public bool VerifyScript(
            string scriptPubKeyHex,
            long amount,
            string transactionHex,
            uint inputIndex,
            List<string> spentOutputsHex,
            ScriptVerificationFlags flags = ScriptVerificationFlags.All)
        {
            using var scriptPubKey = ScriptPubKey.FromHex(scriptPubKeyHex);
            using var transaction = Transaction.FromHex(transactionHex);
            var spentOutputs = spentOutputsHex.Select(hex =>
            {
                // This is a simplification. We assume the spent output is just a script pubkey.
                // A proper implementation would need to decode the full TxOut structure.
                // For now, we create a TxOut with the script and a default amount.
                return new TxOut(ScriptPubKey.FromHex(hex), 0);
            }).ToList();

            try
            {
                return ScriptVerifier.VerifyScript(scriptPubKey, amount, transaction, inputIndex, spentOutputs, flags);
            }
            finally
            {
                foreach (var txOut in spentOutputs)
                {
                    txOut.Dispose();
                }
            }
        }

        public bool ImportBlocks(string blockFilePath)
        {
            ThrowIfDisposed();
            if (_chainstateManager == null)
                throw new InvalidOperationException("Chainstate not initialized");

            return _chainstateManager.ImportBlocks(new[] { blockFilePath });
        }

        /// <summary>
        /// Gets the current chain height (number of blocks).
        /// </summary>
        public int GetChainHeight()
        {
            ThrowIfDisposed();
            return Chainstate.GetActiveChain().Height;
        }

        /// <summary>
        /// Gets the hash of the chain tip (best block).
        /// </summary>
        public byte[] GetChainTipHash()
        {
            ThrowIfDisposed();
            var tip = Chainstate.GetActiveChain().GetTip();
            return tip.GetBlockHash();
        }

        /// <summary>
        /// Gets the hash of the genesis block.
        /// </summary>
        public byte[] GetGenesisBlockHash()
        {
            ThrowIfDisposed();
            var genesis = Chainstate.GetActiveChain().GetGenesis();
            return genesis.GetBlockHash();
        }

        /// <summary>
        /// Gets a block hash by height.
        /// </summary>
        /// <param name="height">The block height.</param>
        /// <returns>The block hash, or null if height is invalid.</returns>
        public byte[]? GetBlockHash(int height)
        {
            ThrowIfDisposed();
            try
            {
                var chain = Chainstate.GetActiveChain();
                var blockIndex = chain.GetBlockByHeight(height);
                return blockIndex?.GetBlockHash();
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the spent outputs (UTXOs) for a block at the specified height.
        /// </summary>
        /// <param name="height">The block height.</param>
        /// <returns>The spent outputs for the block, or null if height is invalid.</returns>
        public BlockSpentOutputs? GetSpentOutputs(int height)
        {
            ThrowIfDisposed();
            try
            {
                var chain = Chainstate.GetActiveChain();
                var blockIndex = chain.GetBlockByHeight(height);
                if (blockIndex == null) return null;
                return Chainstate.ReadSpentOutputs(blockIndex);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Enumerates all block hashes from genesis to tip.
        /// </summary>
        public IEnumerable<byte[]> EnumerateBlockHashes()
        {
            ThrowIfDisposed();
            var chain = Chainstate.GetActiveChain();
            foreach (var blockIndex in chain.EnumerateBlocks())
            {
                yield return blockIndex.GetBlockHash();
            }
        }

        /// <summary>
        /// Gets basic information about a block at the specified height.
        /// </summary>
        /// <param name="height">The block height.</param>
        /// <returns>Block information, or null if height is invalid.</returns>
        public BlockInfo? GetBlockInfo(int height)
        {
            ThrowIfDisposed();
            try
            {
                var chain = Chainstate.GetActiveChain();
                var blockIndex = chain.GetBlockByHeight(height);
                if (blockIndex == null) return null;

                return new BlockInfo
                {
                    Height = blockIndex.Height,
                    Hash = blockIndex.GetBlockHash(),
                    PreviousHash = blockIndex.GetPrevious()?.GetBlockHash()
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets block tree entry (metadata) by block hash.
        /// </summary>
        /// <param name="blockHash">The hash of the block (32 bytes).</param>
        /// <returns>The block tree entry, or null if not found.</returns>
        public Core.BlockProcessing.BlockTreeEntry? GetBlockTreeEntry(byte[] blockHash)
        {
            ThrowIfDisposed();
            if (blockHash == null) throw new ArgumentNullException(nameof(blockHash));
            if (blockHash.Length != 32) throw new ArgumentException("Block hash must be 32 bytes", nameof(blockHash));

            return Processor.GetBlockTreeEntry(blockHash);
        }

        /// <summary>
        /// Gets block tree entry (metadata) by block hash from hex string.
        /// </summary>
        /// <param name="blockHashHex">The hash of the block as a hex string.</param>
        /// <returns>The block tree entry, or null if not found.</returns>
        public Core.BlockProcessing.BlockTreeEntry? GetBlockTreeEntry(string blockHashHex)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(blockHashHex))
                throw new ArgumentException("Block hash hex cannot be null or empty", nameof(blockHashHex));

            try
            {
                byte[] blockHash = Convert.FromHexString(blockHashHex);
                return GetBlockTreeEntry(blockHash);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"Invalid hex string: {ex.Message}", nameof(blockHashHex), ex);
            }
        }

        /// <summary>
        /// Represents basic block information.
        /// </summary>
        public class BlockInfo
        {
            public int Height { get; set; }
            public byte[] Hash { get; set; } = Array.Empty<byte>();
            public byte[]? PreviousHash { get; set; }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(KernelLibrary));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _chainstateManager?.Dispose();
                _loggingConnection?.Dispose();
                _chainParams?.Dispose();
                _context?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~KernelLibrary() => Dispose();

        /// <summary>
        /// Fluent builder for KernelLibrary.
        /// </summary>
        public sealed class KernelBuilder
        {
            private ChainType _chainType = ChainType.MAINNET;
            private Action<string, string, int>? _loggingCallback;
            private int _workerThreads = 4;
            private string? _dataDirectory;
            private string? _blocksDirectory;
            private bool _wipeBlockTree;
            private bool _wipeChainstate;

            internal KernelBuilder() { }

            /// <summary>
            /// Sets the chain type (Mainnet, Testnet, Testnet_4, Regtest, Signet).
            /// </summary>
            private KernelBuilder ForChain(ChainType chainType)
            {
                _chainType = chainType;
                return this;
            }

            /// <summary>
            /// Configures mainnet.
            /// </summary>
            public KernelBuilder ForMainnet() => ForChain(ChainType.MAINNET);

            /// <summary>
            /// Configures testnet.
            /// </summary>
            public KernelBuilder ForTestnet() => ForChain(ChainType.TESTNET);

            /// <summary>
            /// Configures testnet_4.
            /// </summary>
            public KernelBuilder ForTestnet4() => ForChain(ChainType.TESTNET_4);

            /// <summary>
            /// Configures regtest.
            /// </summary>
            public KernelBuilder ForRegtest() => ForChain(ChainType.REGTEST);

            /// <summary>
            /// Configures signet.
            /// </summary>
            public KernelBuilder ForSignet() => ForChain(ChainType.SIGNET);

            /// <summary>
            /// Sets up logging with a callback.
            /// </summary>
            public KernelBuilder WithLogging(Action<string, string, int> callback)
            {
                _loggingCallback = callback;
                return this;
            }

            /// <summary>
            /// Sets the number of worker threads for validation.
            /// </summary>
            /// <param name="threads">Number of worker threads (must be at least 1).</param>
            public KernelBuilder WithWorkerThreads(int threads)
            {
                if (threads < 1)
                    throw new ArgumentOutOfRangeException(nameof(threads), "Worker threads must be at least 1");

                _workerThreads = threads;
                return this;
            }

            /// <summary>
            /// Sets custom data and blocks directories.
            /// </summary>
            /// <param name="dataDirectory">The data directory path.</param>
            /// <param name="blocksDirectory">The blocks directory path.</param>
            public KernelBuilder WithDirectories(string dataDirectory, string blocksDirectory)
            {
                if (string.IsNullOrWhiteSpace(dataDirectory))
                    throw new ArgumentException("Data directory cannot be null or empty", nameof(dataDirectory));
                if (string.IsNullOrWhiteSpace(blocksDirectory))
                    throw new ArgumentException("Blocks directory cannot be null or empty", nameof(blocksDirectory));

                _dataDirectory = dataDirectory;
                _blocksDirectory = blocksDirectory;
                return this;
            }

            /// <summary>
            /// Enables database wiping on startup (useful for testing).
            /// </summary>
            public KernelBuilder WithWipeDatabases(bool wipeBlockTree = false, bool wipeChainstate = false)
            {
                _wipeBlockTree = wipeBlockTree;
                _wipeChainstate = wipeChainstate;
                return this;
            }

            /// <summary>
            /// Builds the KernelLibrary instance.
            /// </summary>
            public KernelLibrary Build()
            {
                var chainParams = new ChainParameters(_chainType);

                var contextOptions = new KernelContextOptions()
                    .SetChainParams(chainParams);

                var context = new KernelContext(contextOptions);

                var library = new KernelLibrary(context, chainParams);

                if (_loggingCallback != null)
                {
                    library.EnableLogging(_loggingCallback);
                }

                // Initialize chainstate with builder options
                string dataDir = _dataDirectory ?? Path.Combine(Path.GetTempPath(), $"bitcoinkernel_{Guid.NewGuid()}");
                string blocksDir = _blocksDirectory ?? Path.Combine(dataDir, "blocks");

                var options = new ChainstateManagerOptions(context, dataDir, blocksDir)
                    .SetWorkerThreads(_workerThreads);

                if (_wipeBlockTree || _wipeChainstate)
                {
                    options.SetWipeDbs(_wipeBlockTree, _wipeChainstate);
                }

                library.InitializeChainstate(dataDir, blocksDir, _workerThreads);

                return library;
            }
        }

        /// <summary>
        /// Validates a block without adding it to the chain.
        /// </summary>
        /// <param name="blockData">The block data to validate.</param>
        /// <returns>True if the block is valid, false otherwise.</returns>
        public bool ValidateBlock(byte[] blockData)
        {
            ThrowIfDisposed();

            if (blockData == null || blockData.Length < 80)
                return false;

            try
            {
                using var block = Block.FromBytes(blockData);
                var result = Processor.ValidateBlock(block);
                return result.IsValid;
            }
            catch
            {
                // If parsing or validation fails, the block is invalid
                return false;
            }
        }

        /// <summary>
        /// Validates a block from hex string without adding it to the chain.
        /// </summary>
        /// <param name="blockHex">The block data as a hex string.</param>
        /// <returns>True if the block is valid, false otherwise.</returns>
        public bool ValidateBlock(string blockHex)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(blockHex))
                return false;

            try
            {
                byte[] blockData = Convert.FromHexString(blockHex);
                return ValidateBlock(blockData);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a block and returns detailed validation results.
        /// </summary>
        /// <param name="blockData">The block data to validate.</param>
        /// <returns>Detailed validation result.</returns>
        public Core.BlockProcessing.BlockValidationResult ValidateBlockDetailed(byte[] blockData)
        {
            ThrowIfDisposed();

            if (blockData == null || blockData.Length < 80)
            {
                return new Core.BlockProcessing.BlockValidationResult(
                    isValid: false,
                    mode: ValidationMode.INVALID,
                    errorMessage: "Block data is null or too small");
            }

            try
            {
                using var block = Block.FromBytes(blockData);
                return Processor.ValidateBlock(block);
            }
            catch (Exception ex)
            {
                return new Core.BlockProcessing.BlockValidationResult(
                    isValid: false,
                    mode: ValidationMode.INTERNAL_ERROR,
                    errorMessage: ex.Message);
            }
        }

        /// <summary>
        /// Validates a block from hex string and returns detailed validation results.
        /// </summary>
        /// <param name="blockHex">The block data as a hex string.</param>
        /// <returns>Detailed validation result.</returns>
        public Core.BlockProcessing.BlockValidationResult ValidateBlockDetailed(string blockHex)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(blockHex))
            {
                return new Core.BlockProcessing.BlockValidationResult(
                    isValid: false,
                    mode: ValidationMode.INVALID,
                    errorMessage: "Block hex cannot be null or empty");
            }

            try
            {
                byte[] blockData = Convert.FromHexString(blockHex);
                return ValidateBlockDetailed(blockData);
            }
            catch (Exception ex)
            {
                return new Core.BlockProcessing.BlockValidationResult(
                    isValid: false,
                    mode: ValidationMode.INTERNAL_ERROR,
                    errorMessage: $"Failed to parse hex string: {ex.Message}");
            }
        }
    }

}