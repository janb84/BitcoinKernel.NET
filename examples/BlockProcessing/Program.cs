using System.IO;
using BitcoinKernel;
using BitcoinKernel.Primatives;
using BitcoinKernel.Chain;
using BitcoinKernel.Interop.Enums;

namespace BlockProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bitcoin Kernel Block Processing Example");
            Console.WriteLine("=====================================");

            var dataDir = Path.Combine(Path.GetTempPath(), $"bitcoinkernel_{Guid.NewGuid()}");
            var blocksDir = Path.Combine(dataDir, "blocks");

            try
            {
                using var logging = new LoggingConnection((category, message, level) =>
                {
                    if (level <= 2)
                        Console.WriteLine($"[{category}] {message}");
                });

                using var chainParams = new ChainParameters(ChainType.MAINNET);
                using var contextOptions = new KernelContextOptions().SetChainParams(chainParams);
                using var context = new KernelContext(contextOptions);
                using var options = new ChainstateManagerOptions(context, dataDir, blocksDir);
                using var chainstate = new ChainstateManager(context, chainParams, options);

                Console.WriteLine("Created kernel for mainnet");
                Console.WriteLine("Chainstate initialized");

                byte[] sampleBlockData;
                try
                {
                    sampleBlockData = CreateSampleBlock();
                    Console.WriteLine("Created sample block data");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Block creation failed: {ex.Message}");
                    Console.WriteLine("This is expected for simplified block data.");
                    return;
                }

                DisplayBlockInfo(sampleBlockData);

                Console.WriteLine("\nProcessing block...");
                try
                {
                    using var block = Block.FromBytes(sampleBlockData);
                    bool isNew = chainstate.ProcessBlock(block);

                    if (isNew)
                    {
                        var activeChain = chainstate.GetActiveChain();
                        Console.WriteLine($"Block processed! Chain height: {activeChain.Height}");
                        var tip = activeChain.GetTip();
                        Console.WriteLine($"  - Tip: {BitConverter.ToString(tip.GetBlockHash()).Replace("-", "")}");
                    }
                    else
                    {
                        Console.WriteLine("Block processing failed - expected for invalid block data");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Block processing error: {ex.Message}");
                    Console.WriteLine("This is expected for simplified/invalid block data.");
                }

                Console.WriteLine("\nBlock processing example completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }

        private static byte[] CreateSampleBlock()
        {
            byte[] blockData = new byte[80];

            // Version: 1 (little endian)
            BitConverter.GetBytes(1).CopyTo(blockData, 0);

            // Previous block hash: all zeros (indices 4-35)
            // Merkle root: all zeros (indices 36-67)

            // Timestamp: current Unix timestamp
            uint timestamp = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            BitConverter.GetBytes(timestamp).CopyTo(blockData, 68);

            // Bits: 0x1d00ffff (Bitcoin mainnet difficulty)
            BitConverter.GetBytes(0x1d00ffffu).CopyTo(blockData, 72);

            // Nonce: 0 (indices 76-79)

            return blockData;
        }

        private static void DisplayBlockInfo(byte[] blockData)
        {
            Console.WriteLine("\nBlock Information:");
            Console.WriteLine("-----------------");

            try
            {
                Console.WriteLine($"Block Size: {blockData.Length} bytes");
                Console.WriteLine($"Block Data (first 32 bytes): {BitConverter.ToString(blockData.Take(32).ToArray()).Replace("-", " ")}");

                uint version = BitConverter.ToUInt32(blockData, 0);
                Console.WriteLine($"Version: {version}");

                uint timestamp = BitConverter.ToUInt32(blockData, 68);
                DateTime blockTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                Console.WriteLine($"Timestamp: {timestamp} ({blockTime:yyyy-MM-dd HH:mm:ss UTC})");

                uint bits = BitConverter.ToUInt32(blockData, 72);
                Console.WriteLine($"Bits: 0x{bits:X8}");

                uint nonce = BitConverter.ToUInt32(blockData, 76);
                Console.WriteLine($"Nonce: {nonce}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing block info: {ex.Message}");
            }
        }
    }
}
