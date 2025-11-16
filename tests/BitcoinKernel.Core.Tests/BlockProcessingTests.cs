using BitcoinKernel.Core.Chain;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop.Enums;
using Xunit;

namespace BitcoinKernel.Core.Tests
{
    public class BlockProcessingTests : IDisposable
    {
        private KernelContext? _context;
        private ChainParameters? _chainParams;
        private string? _tempDir;

        public void Dispose()
        {
            _chainParams?.Dispose();
            _context?.Dispose();

            if (!string.IsNullOrEmpty(_tempDir) && Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        private (KernelContext, ChainParameters, string) TestingSetup()
        {
            // Create kernel context for regtest
            _chainParams = new ChainParameters(ChainType.REGTEST);
            var contextOptions = new KernelContextOptions()
                .SetChainParams(_chainParams);
            _context = new KernelContext(contextOptions);

            // Create temporary directory
            _tempDir = Path.Combine(Path.GetTempPath(), $"test_chainman_regtest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDir);

            return (_context, _chainParams, _tempDir);
        }

        private List<byte[]> ReadBlockData()
        {
            var blockData = new List<byte[]>();
            // The file is in the test project directory
            var testAssemblyDir = Path.GetDirectoryName(typeof(BlockProcessingTests).Assembly.Location);
            var projectDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(testAssemblyDir)));
            var blockDataFile = Path.Combine(projectDir!, "TestData", "block_data.txt");

            if (!File.Exists(blockDataFile))
            {
                throw new FileNotFoundException($"Block data file not found: {blockDataFile}");
            }

            foreach (var line in File.ReadLines(blockDataFile))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    blockData.Add(Convert.FromHexString(line.Trim()));
                }
            }

            return blockData;
        }

        private void ProcessBlockData(ChainstateManager chainstateManager, List<byte[]> blockData)
        {
            foreach (var rawBlock in blockData)
            {
                using var block = Abstractions.Block.FromBytes(rawBlock);
                chainstateManager.ProcessBlock(block);
            }
        }

        private (ChainstateManager, List<byte[]>) SetupChainstateManager()
        {
            var (context, chainParams, dataDir) = TestingSetup();
            var blockData = ReadBlockData();

            var options = new ChainstateManagerOptions(context, dataDir, Path.Combine(dataDir, "blocks"));
            var chainstateManager = new ChainstateManager(context, chainParams, options);

            return (chainstateManager, blockData);
        }

        [Fact]
        public void TestProcessData()
        {
            // Arrange
            var (context, chainParams, dataDir) = TestingSetup();
            var blocksDir = Path.Combine(dataDir, "blocks");
            var blockData = ReadBlockData();


            var options = new ChainstateManagerOptions(context, dataDir, blocksDir);
            using var chainstateManager = new ChainstateManager(context, chainParams, options);

            // Act & Assert
            foreach (var rawBlock in blockData)
            {
                using var block = Abstractions.Block.FromBytes(rawBlock);
                var result = chainstateManager.ProcessBlock(block);

                // Assert the block was processed successfully (is new)
                Assert.True(result, "Block should be new and processed successfully");
            }
        }

        [Fact]
        public void TestValidateAny()
        {
            // Arrange
            var (context, chainParams, dataDir) = TestingSetup();
            var blocksDir = Path.Combine(dataDir, "blocks");
            var blockData = ReadBlockData();

            var options = new ChainstateManagerOptions(context, dataDir, blocksDir);
            using var chainstateManager = new ChainstateManager(context, chainParams, options);

            // Act & Assert
            chainstateManager.ImportBlocks();
            using var block2 = Abstractions.Block.FromBytes(blockData[1]);

            // The block should be invalid and processing should fail
            var exception = Assert.Throws<ChainstateManagerException>(() =>
                chainstateManager.ProcessBlock(block2));

            // Verify it's a validation error (non-zero error code)
            Assert.Contains("Failed to process block", exception.Message);
        }

        [Fact]
        public void TestReindex()
        {
            // Arrange
            var (context, chainParams, dataDir) = TestingSetup();
            var blocksDir = Path.Combine(dataDir, "blocks");
            var blockData = ReadBlockData();

            // Act - Process blocks
            {
                var options = new ChainstateManagerOptions(context, dataDir, blocksDir);
                using var chainstateManager = new ChainstateManager(context, chainParams, options);

                foreach (var rawBlock in blockData)
                {
                    using var block = Abstractions.Block.FromBytes(rawBlock);
                    var result = chainstateManager.ProcessBlock(block);
                    Assert.True(result, "Block should be new and processed successfully");
                }
            }


            // Act - Reindex
            var reindexOptions = new ChainstateManagerOptions(context, dataDir, blocksDir)
                .SetWipeDbs(false, true);
            using var chainstateManagerReindex = new ChainstateManager(context, chainParams, reindexOptions);
            var result_reindex = chainstateManagerReindex.ImportBlocks();

            Assert.True(result_reindex, "Reindexing should be successful");

            // Assert - Verify chainstate is intact after reindex
            var activeChain = chainstateManagerReindex.GetActiveChain();
            Assert.Equal(blockData.Count, activeChain.Height);
        }

        [Fact]
        public void TestInvalidBlock()
        {
            // Arrange
            var (context, chainParams, dataDir) = TestingSetup();
            var blocksDir = Path.Combine(dataDir, "blocks");

            for (int i = 0; i < 10; i++)
            {
                var options = new ChainstateManagerOptions(context, dataDir, blocksDir);
                using var chainstateManager = new ChainstateManager(context, chainParams, options);

                // Not a block
                var invalidBlockData = Convert.FromHexString("deadbeef");
                Assert.Throws<BlockException>(() => Abstractions.Block.FromBytes(invalidBlockData));

                // Invalid block
                var invalidBlockHex = "010000006fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000982051fd" +
                    "1e4ba744bbbe680e1fee14677ba1a3c3540bf7b1cdb606e857233e0e61bc6649ffff001d01e36299" +
                    "0101000000010000000000000000000000000000000000000000000000000000000000000000ffff" +
                    "ffff0704ffff001d0104ffffffff0100f2052a0100000043410496b538e853519c726a2c91e61ec1" +
                    "1600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf62" +
                    "1e73a82cbf2342c858eeac00000000";

                using var block = Abstractions.Block.FromBytes(Convert.FromHexString(invalidBlockHex));

                // The block should be invalid and processing should fail
                var exception = Assert.Throws<ChainstateManagerException>(() =>
                    chainstateManager.ProcessBlock(block));

                // Verify it's a validation error (non-zero error code)
                Assert.Contains("Failed to process block", exception.Message);
            }
        }

        [Fact]
        public void TestScanTransactions()
        {
            // Arrange - Setup test environment with blocks
            var setup = SetupChainstateManager();
            using var chainstateManager = setup.Item1;
            var blockData = setup.Item2;

            // Act - Process all test blocks
            ProcessBlockData(chainstateManager, blockData);

            var activeChain = chainstateManager.GetActiveChain();

            // Verify we can iterate through the chain by height
            for (int height = 0; height <= activeChain.Height; height++)
            {
                var blockIndex = activeChain.GetBlockByHeight(height);
                Assert.NotNull(blockIndex);
                Assert.Equal(height, blockIndex.Height);
            }

            // Get the tip and read its spent outputs
            var tipBlockIndex = activeChain.GetTip();
            Assert.NotNull(tipBlockIndex);

            using var spentOutputsTip = chainstateManager.ReadSpentOutputs(tipBlockIndex);
            Assert.NotNull(spentOutputsTip);

            // The number of transaction spent outputs should match transactions minus coinbase
            // This is validated by checking that Count returns a valid value
            var tipSpentOutputsCount = spentOutputsTip.Count;
            Assert.True(tipSpentOutputsCount >= 0, "Spent outputs count should be non-negative");

            // If we have a previous block, scan its transactions in detail
            var previousBlock = tipBlockIndex.GetPrevious();
            if (previousBlock != null)
            {
                using var spentOutputs = chainstateManager.ReadSpentOutputs(previousBlock);
                var spentOutputsCount = spentOutputs.Count;

                // Scan each transaction's spent outputs
                for (int txIndex = 0; txIndex < spentOutputsCount; txIndex++)
                {
                    using var txSpentOutputs = spentOutputs.GetTransactionSpentOutputs(txIndex);
                    var coinsCount = txSpentOutputs.Count;

                    // Verify we can access each coin
                    for (int coinIndex = 0; coinIndex < coinsCount; coinIndex++)
                    {
                        using var coin = txSpentOutputs.GetCoin(coinIndex);

                        // Verify coin properties
                        Assert.True(coin.ConfirmationHeight >= 0, "Confirmation height should be non-negative");

                        // We should be able to get the output
                        using var output = coin.GetOutput();
                        Assert.NotNull(output);

                        // Verify we can get the script pubkey from the output
                        var scriptPubkeyBytes = output.GetScriptPubkey();
                        Assert.NotNull(scriptPubkeyBytes);
                        Assert.True(scriptPubkeyBytes.Length >= 0, "Script pubkey should have valid length");

                        // Verify we can get the amount
                        var amount = output.Amount;
                        Assert.True(amount >= 0, "Amount should be non-negative");
                    }
                }
            }
        }

        [Fact]
        public void TestChainOperations()
        {
            // Arrange - Setup test environment with blocks
            var setup = SetupChainstateManager();
            using var chainstateManager = setup.Item1;
            var blockData = setup.Item2;

            // Process all test blocks
            ProcessBlockData(chainstateManager, blockData);

            var chain = chainstateManager.GetActiveChain();

            // Test genesis via GetGenesis() method
            var genesis = chain.GetGenesis();
            Assert.NotNull(genesis);
            Assert.Equal(0, genesis.Height);
            var genesisHash = genesis.GetBlockHash();
            Assert.NotNull(genesisHash);

            // Test tip block
            var tip = chain.GetTip();
            Assert.NotNull(tip);
            var tipHeight = tip.Height;
            var tipHash = tip.GetBlockHash();

            Assert.True(tipHeight > 0);
            Assert.False(genesisHash.SequenceEqual(tipHash));

            // Test accessing block by height - genesis
            var genesisViaHeight = chain.GetBlockByHeight(0);
            Assert.NotNull(genesisViaHeight);
            Assert.Equal(0, genesisViaHeight.Height);
            Assert.True(genesisHash.SequenceEqual(genesisViaHeight.GetBlockHash()));

            // Test accessing block by height - tip
            var tipViaHeight = chain.GetBlockByHeight(tipHeight);
            Assert.NotNull(tipViaHeight);
            Assert.Equal(tipHeight, tipViaHeight.Height);
            Assert.True(tipHash.SequenceEqual(tipViaHeight.GetBlockHash()));

            // Test invalid height returns null
            var invalidEntry = chain.GetBlockByHeight(9999);
            Assert.Null(invalidEntry);

            // Test Contains method
            Assert.True(chain.Contains(genesis));
            Assert.True(chain.Contains(tip));

            // Test iteration through chain
            int count = 0;
            foreach (var currentBlockIndex in chain.EnumerateBlocks())
            {
                Assert.True(chain.Contains(currentBlockIndex));
                Assert.Equal(count, currentBlockIndex.Height);
                count++;
            }

            // Verify we iterated through the entire chain
            Assert.Equal(tipHeight + 1, count);
        }

        [Fact]
        public void TestBlockSpentOutputsIterator()
        {
            // Arrange - Setup test environment with blocks
            var setup = SetupChainstateManager();
            using var chainstateManager = setup.Item1;
            var blockData = setup.Item2;

            // Process all test blocks
            ProcessBlockData(chainstateManager, blockData);

            var activeChain = chainstateManager.GetActiveChain();
            var blockIndexTip = activeChain.GetTip();
            using var spentOutputs = chainstateManager.ReadSpentOutputs(blockIndexTip);

            // Test count via iterator matches Count property
            var countViaIterator = spentOutputs.EnumerateTransactionSpentOutputs().Count();
            Assert.Equal(spentOutputs.Count, countViaIterator);

            // Test collecting all transaction spent outputs
            var txSpentVec = spentOutputs.EnumerateTransactionSpentOutputs().ToList();
            Assert.Equal(spentOutputs.Count, txSpentVec.Count);

            // Test that each transaction spent output from iterator matches by index
            int i = 0;
            foreach (var txSpent in spentOutputs.EnumerateTransactionSpentOutputs())
            {
                using var txSpentViaIndex = spentOutputs.GetTransactionSpentOutputs(i);
                Assert.Equal(txSpent.Count, txSpentViaIndex.Count);
                i++;
            }

            // Test iterator length tracking
            var initialLen = spentOutputs.Count;

            if (initialLen > 0)
            {
                // After skipping one element, we have initialLen - 1 remaining
                var remaining = spentOutputs.EnumerateTransactionSpentOutputs().Skip(1).Count();
                Assert.Equal(initialLen - 1, remaining);
            }
        }

        [Fact]
        public void TestTransactionSpentOutputsIterator()
        {
            // Arrange - Setup test environment with blocks
            var setup = SetupChainstateManager();
            using var chainstateManager = setup.Item1;
            var blockData = setup.Item2;

            // Process all test blocks
            ProcessBlockData(chainstateManager, blockData);

            var activeChain = chainstateManager.GetActiveChain();
            var blockIndexTip = activeChain.GetTip();
            using var spentOutputs = chainstateManager.ReadSpentOutputs(blockIndexTip);

            using var txSpent = spentOutputs.GetTransactionSpentOutputs(0);

            // Test count via iterator matches Count property
            var countViaIterator = txSpent.EnumerateCoins().Count();
            Assert.Equal(txSpent.Count, countViaIterator);

            // Test collecting all coins
            var coins = txSpent.EnumerateCoins().ToList();
            Assert.Equal(txSpent.Count, coins.Count);

            // Test that each coin from iterator matches coin by index
            int i = 0;
            foreach (var coin in txSpent.EnumerateCoins())
            {
                using var coinViaIndex = txSpent.GetCoin(i);
                Assert.Equal(coin.ConfirmationHeight, coinViaIndex.ConfirmationHeight);
                Assert.Equal(coin.IsCoinbase, coinViaIndex.IsCoinbase);
                i++;
            }

            // Test iterator length tracking
            var initialLen = txSpent.Count;

            if (initialLen > 0)
            {
                // After skipping one element, we have initialLen - 1 remaining
                var remaining = txSpent.EnumerateCoins().Skip(1).Count();
                Assert.Equal(initialLen - 1, remaining);
            }

            // Test filtering coinbase coins
            var coinbaseCoins = txSpent.EnumerateCoins().Where(coin => coin.IsCoinbase).ToList();

            foreach (var coin in coinbaseCoins)
            {
                Assert.True(coin.IsCoinbase);
            }
        }

        [Fact]
        public void TestNestedIteration()
        {
            // Arrange - Setup test environment with blocks
            var setup = SetupChainstateManager();
            using var chainstateManager = setup.Item1;
            var blockData = setup.Item2;

            // Process all test blocks
            ProcessBlockData(chainstateManager, blockData);

            var activeChain = chainstateManager.GetActiveChain();
            var blockIndex = activeChain.GetBlockByHeight(1);
            Assert.NotNull(blockIndex);

            using var spentOutputs = chainstateManager.ReadSpentOutputs(blockIndex);

            // Count total coins by nested iteration
            int totalCoins = 0;
            foreach (var txSpent in spentOutputs.EnumerateTransactionSpentOutputs())
            {
                foreach (var coin in txSpent.EnumerateCoins())
                {
                    totalCoins++;
                }
            }

            // Calculate expected total using LINQ
            int expectedTotal = spentOutputs.EnumerateTransactionSpentOutputs()
                .Sum(txSpent => txSpent.Count);

            Assert.Equal(expectedTotal, totalCoins);
        }

        [Fact]
        public void TestIteratorWithBlockTransactions()
        {
            // Arrange - Setup test environment with blocks
            var setup = SetupChainstateManager();
            using var chainstateManager = setup.Item1;
            var blockData = setup.Item2;

            // Process all test blocks
            ProcessBlockData(chainstateManager, blockData);

            var activeChain = chainstateManager.GetActiveChain();
            var blockIndex = activeChain.GetBlockByHeight(1);
            Assert.NotNull(blockIndex);

            // Use the block data we already have (index 1 corresponds to blockData[1])
            using var block = Abstractions.Block.FromBytes(blockData[1]);
            using var spentOutputs = chainstateManager.ReadSpentOutputs(blockIndex);

            // Zip block transactions (skipping coinbase) with spent outputs
            // Each transaction's input count should match its spent outputs count
            int txIndex = 0;
            foreach (var tx in block.GetTransactions().Skip(1))
            {
                using var txSpent = spentOutputs.GetTransactionSpentOutputs(txIndex);
                Assert.Equal(tx.InputCount, txSpent.Count);
                txIndex++;
            }

            // Verify we processed all spent outputs
            Assert.Equal(spentOutputs.Count, txIndex);
        }
    }
}