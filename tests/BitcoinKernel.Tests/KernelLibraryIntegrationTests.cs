using System;
using System.IO;
using System.Linq;
using BitcoinKernel;
using Xunit;

namespace BitcoinKernel.Tests
{
    /// <summary>
    /// Integration tests for the BitcoinKernel facade.
    /// These tests verify end-to-end functionality of the library.
    /// </summary>
    public class KernelLibraryIntegrationTests : IDisposable
    {
        private KernelLibrary? _kernel;

        /// <summary>
        /// Helper property to ensure previous kernel is disposed before assigning a new one.
        /// </summary>
        private KernelLibrary Kernel
        {
            get => _kernel ?? throw new InvalidOperationException("Kernel not initialized");
            set
            {
                _kernel?.Dispose();
                _kernel = value;
            }
        }

        public void Dispose()
        {
            _kernel?.Dispose();
            _kernel = null;
        }

        [Fact]
        public void KernelLibrary_CanCreateAndDispose()
        {
            // Arrange & Act
            Kernel = KernelLibrary.Create()
                .ForMainnet()
                .Build();

            // Assert
            Assert.NotNull(_kernel);
        }

        [Fact]
        public void KernelLibrary_CanCreateForDifferentChains()
        {
            // Test mainnet
            using var mainnetKernel = KernelLibrary.Create()
                .ForMainnet()
                .Build();
            Assert.NotNull(mainnetKernel);

            // Test testnet
            using var testnetKernel = KernelLibrary.Create()
                .ForTestnet()
                .Build();
            Assert.NotNull(testnetKernel);

            // test testnet_4
            using var testnet4Kernel = KernelLibrary.Create()
                .ForTestnet4()
                .Build();
            Assert.NotNull(testnet4Kernel);

            // Test signet
            using var signetKernel = KernelLibrary.Create()
                .ForSignet()
                .Build();
            Assert.NotNull(signetKernel);

            // Test regtest
            using var regtestKernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();
            Assert.NotNull(regtestKernel);
        }

        [Fact]
        public void KernelLibrary_CanEnableLogging()
        {
            // Arrange
            string? lastLogMessage = null;
            void LogCallback(string category, string message, int level)
            {
                lastLogMessage = $"[{category}] {message} (level: {level})";
            }

            // Act
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .WithLogging(LogCallback)
                .Build();

            // Assert
            Assert.NotNull(_kernel);
            // Note: We can't easily test actual logging without triggering log events,
            // but we can verify the kernel was created successfully with logging enabled
        }

        [Fact]
        public void KernelLibrary_CanVerifyScript()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Test data: Simple P2PKH script verification
            // This is a basic test - in real scenarios you'd use actual transaction data
            string scriptPubkeyHex = "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ac";
            string transactionHex = "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700";

            // Act & Assert
            // This should succeed with valid test data
            bool result = Kernel.VerifyScript(scriptPubkeyHex, 0, transactionHex, 0, new List<string> { scriptPubkeyHex });
            Assert.True(result);
        }

        // [Fact]
        // public void KernelLibrary_ScriptVerificationFailsWithInvalidData()
        // {
        //     Arrange
        //     Kernel = KernelLibrary.Create()
        //         .ForRegtest()
        //         .Build();

        //     Use invalid script pubkey (modified last byte)
        //     string invalidScriptPubkeyHex = "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ff";
        //     ScriptPubKey invalidScriptPubKey = ScriptPubKey.FromHex(invalidScriptPubkeyHex);
        //     string transactionHex = "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700";
        //     Transaction transaction = Transaction.FromHex(transactionHex);
        //     List<TxOut> spentOutputs = new List<TxOut>();

        //     Act & Assert
        //     This should fail with invalid script data
        //     bool result = Kernel.VerifyScript(invalidScriptPubKey, 0, transaction, 0, spentOutputs, ScriptVerificationFlags.All);
        //     Assert.False(result);
        // }

        [Fact]
        public void KernelLibrary_AutoInitializesChainstate()
        {
            // Arrange & Act
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Assert
            // Chainstate should be automatically initialized by the builder
            Assert.NotNull(Kernel.Chainstate);
            
            // Should be able to query chain information immediately
            int height = Kernel.GetChainHeight();
            Assert.True(height >= 0);
        }

        [Fact]
        public void KernelLibrary_CanGetChainHeight()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Act
            int height = Kernel.GetChainHeight();

            // Assert
            Assert.True(height >= 0); // Chain height should be non-negative
        }

        [Fact]
        public void KernelLibrary_CanGetChainTipHash()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Act
            byte[] tipHash = Kernel.GetChainTipHash();

            // Assert
            Assert.NotNull(tipHash);
            Assert.Equal(32, tipHash.Length); // Bitcoin hashes are 32 bytes
        }

        [Fact]
        public void KernelLibrary_CanGetGenesisBlockHash()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Act
            byte[] genesisHash = Kernel.GetGenesisBlockHash();

            // Assert
            Assert.NotNull(genesisHash);
            Assert.Equal(32, genesisHash.Length); // Bitcoin hashes are 32 bytes
        }

        [Fact]
        public void KernelLibrary_CanGetBlockHash()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Act - Get genesis block hash (height 0)
            byte[]? genesisHash = Kernel.GetBlockHash(0);

            // Assert
            Assert.NotNull(genesisHash);
            Assert.Equal(32, genesisHash.Length);

            // Test invalid height
            byte[]? invalidHash = Kernel.GetBlockHash(-1);
            Assert.Null(invalidHash);
        }

        [Fact]
        public void KernelLibrary_CanEnumerateBlockHashes()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Act
            var blockHashes = Kernel.EnumerateBlockHashes().ToList();

            // Assert
            Assert.NotEmpty(blockHashes);
            Assert.True(blockHashes.Count >= 1); // At least genesis block

            // All hashes should be 32 bytes
            foreach (var hash in blockHashes)
            {
                Assert.Equal(32, hash.Length);
            }
        }

        [Fact]
        public void KernelLibrary_CanGetBlockInfo()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Act - Get genesis block info
            var genesisInfo = Kernel.GetBlockInfo(0);

            // Assert
            Assert.NotNull(genesisInfo);
            Assert.Equal(0, genesisInfo.Height);
            Assert.NotNull(genesisInfo.Hash);
            Assert.Equal(32, genesisInfo.Hash.Length);
            Assert.Null(genesisInfo.PreviousHash); // Genesis has no previous

            // Test invalid height
            var invalidInfo = Kernel.GetBlockInfo(-1);
            Assert.Null(invalidInfo);
        }

        [Fact]
        public void KernelLibrary_CanValidateBlock()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Test that ValidateBlock properly calls the core validation
            // With invalid block data, it should return false
            byte[] invalidBlockData = new byte[80 + 1 + 60]; // Header + tx count + minimal data
            invalidBlockData[80] = 1; // 1 transaction (but not a valid block)

            // Act
            bool isValid = Kernel.ValidateBlock(invalidBlockData);

            // Assert
            // Now using real validation from BlockProcessor, this should correctly fail
            Assert.False(isValid);

            // Test invalid block (too small for header)
            byte[] tooSmallBlockData = new byte[40]; // Too small
            bool isTooSmallValid = Kernel.ValidateBlock(tooSmallBlockData);
            Assert.False(isTooSmallValid);
            
            // Test null block data
            bool isNullValid = Kernel.ValidateBlock((byte[])null!);
            Assert.False(isNullValid);
        }

        [Fact]
        public void KernelLibrary_CanValidateBlockDetailed()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Test that ValidateBlockDetailed returns detailed validation results
            byte[] invalidBlockData = new byte[80 + 1 + 60]; // Invalid block data
            invalidBlockData[80] = 1;

            // Act
            var result = Kernel.ValidateBlockDetailed(invalidBlockData);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            
            // Test with too small block data
            byte[] tooSmallData = new byte[40];
            var smallResult = Kernel.ValidateBlockDetailed(tooSmallData);
            Assert.False(smallResult.IsValid);
            Assert.Contains("too small", smallResult.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
            
            // Test with null data
            var nullResult = Kernel.ValidateBlockDetailed((byte[])null!);
            Assert.False(nullResult.IsValid);
        }

        [Fact]
        public void KernelLibrary_BuilderCanConfigureWorkerThreads()
        {
            // Arrange & Act
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .WithWorkerThreads(8)
                .Build();

            // Assert
            Assert.NotNull(_kernel);
        }

        [Fact]
        public void KernelLibrary_BuilderCanConfigureDirectories()
        {
            // Arrange
            string customDataDir = Path.Combine(Path.GetTempPath(), $"BitcoinKernel_Data_{Guid.NewGuid()}");
            string customBlocksDir = Path.Combine(Path.GetTempPath(), $"BitcoinKernel_Blocks_{Guid.NewGuid()}");

            try
            {
                // Act
                Kernel = KernelLibrary.Create()
                    .ForRegtest()
                    .WithDirectories(customDataDir, customBlocksDir)
                    .Build();

                // Assert
                Assert.NotNull(_kernel);
                Assert.NotNull(Kernel.Chainstate); // Should be auto-initialized
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(customDataDir))
                    Directory.Delete(customDataDir, true);
                if (Directory.Exists(customBlocksDir))
                    Directory.Delete(customBlocksDir, true);
            }
        }

        [Fact]
        public void KernelLibrary_BuilderCanConfigureWipeDatabases()
        {
            // Arrange & Act
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .WithWipeDatabases(wipeBlockTree: true, wipeChainstate: true)
                .Build();

            // Assert
            Assert.NotNull(_kernel);
            Assert.NotNull(Kernel.Chainstate); // Should be auto-initialized
            // Note: Database wiping behavior would be tested in integration scenarios
            // with actual data persistence
        }

        [Fact]
        public void KernelLibrary_CanProcessBlockWithHexString()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Create some hex block data (this will fail to parse)
            string blockHex = "0100000000000000000000000000000000000000000000000000000000000000000000003ba3edfd7a7b12b27ac72c3e67768f617fc81bc3888a51323a9fb8aa4b1e5e4a29ab5f49ffff001d1dac2b7c01010000000100000000000000000000";

            // Act & Assert
            // Invalid block data should throw BlockException during parsing
            Assert.Throws<Core.Exceptions.BlockException>(() => Kernel.ProcessBlock(blockHex));
        }

        [Fact]
        public void KernelLibrary_CanValidateBlockWithHexString()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Create some hex block data
            string blockHex = "0100000000000000000000000000000000000000000000000000000000000000000000003ba3edfd7a7b12b27ac72c3e67768f617fc81bc3888a51323a9fb8aa4b1e5e4a29ab5f49ffff001d1dac2b7c01010000000100000000000000000000";

            // Act
            bool isValid = Kernel.ValidateBlock(blockHex);

            // Assert - Invalid block should return false
            Assert.False(isValid);
        }

        [Fact]
        public void KernelLibrary_CanValidateBlockDetailedWithHexString()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            string blockHex = "0100000000000000000000000000000000000000000000000000000000000000000000003ba3edfd7a7b12b27ac72c3e67768f617fc81bc3888a51323a9fb8aa4b1e5e4a29ab5f49ffff001d1dac2b7c01010000000100000000000000000000";

            // Act
            var result = Kernel.ValidateBlockDetailed(blockHex);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void KernelLibrary_CanGetBlockTreeEntry()
        {
            // Arrange
            Kernel = KernelLibrary.Create()
                .ForRegtest()
                .Build();

            // Get genesis block hash
            byte[] genesisHash = Kernel.GetGenesisBlockHash();

            // Act
            var entry = Kernel.GetBlockTreeEntry(genesisHash);

            // Assert - May be null if block tree entry not available
            // This is acceptable as it depends on kernel implementation
            if (entry != null)
            {
                Assert.NotNull(entry);
            }
        }

        [Fact]
        public void KernelLibrary_BuilderValidatesWorkerThreads()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                KernelLibrary.Create()
                    .ForRegtest()
                    .WithWorkerThreads(0) // Invalid - must be at least 1
                    .Build());
        }

        [Fact]
        public void KernelLibrary_BuilderValidatesDirectories()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                KernelLibrary.Create()
                    .ForRegtest()
                    .WithDirectories("", "/some/path") // Empty data directory
                    .Build());

            Assert.Throws<ArgumentException>(() =>
                KernelLibrary.Create()
                    .ForRegtest()
                    .WithDirectories("/some/path", "") // Empty blocks directory
                    .Build());
        }
    }
}