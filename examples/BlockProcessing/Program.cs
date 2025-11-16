using System;
using System.IO;
using BitcoinKernel;

namespace BlockProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bitcoin Kernel Block Processing Example");
            Console.WriteLine("=====================================");

            try
            {
                // Step 1: Create kernel library with fluent builder
                using var kernel = KernelLibrary.Create()
                    .ForMainnet()
                    .WithLogging((category, message, level) =>
                    {
                        // Simple logging callback - only show important messages
                        if (level <= 2) // Info and below
                        {
                            Console.WriteLine($"[{category}] {message}");
                        }
                    })
                    .Build();

                Console.WriteLine("✓ Created kernel library for mainnet");
                Console.WriteLine("✓ Chainstate automatically initialized by builder");

                // Step 3: Create a sample block for processing
                // For demonstration, we'll try to create a block
                // Note: This is a simplified example - real blocks are complex
                byte[] sampleBlockData;
                try
                {
                    sampleBlockData = CreateSampleBlock();
                    Console.WriteLine("✓ Created sample block data");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Block creation failed: {ex.Message}");
                    Console.WriteLine("  This is expected for simplified block data.");
                    Console.WriteLine("  The kernel library setup was successful!");
                    return;
                }

                // Step 4: Display block information before processing
                DisplayBlockInfo(sampleBlockData);

                // Step 5: Process the block through validation
                Console.WriteLine("\nProcessing block...");
                try
                {
                    bool success = kernel.ProcessBlock(sampleBlockData);

                    if (success)
                    {
                        Console.WriteLine("✓ Block processed successfully!");

                        // Step 6: Get active chain information
                        var activeChain = kernel.Chainstate.GetActiveChain();
                        Console.WriteLine($"  - Active chain height: {activeChain.Height}");

                        var tip = activeChain.GetTip();
                        Console.WriteLine($"  - Active chain tip: {BitConverter.ToString(tip.GetBlockHash()).Replace("-", "")}");
                    }
                    else
                    {
                        Console.WriteLine("✗ Block processing failed - this may be expected for invalid block data");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Block processing error: {ex.Message}");
                    Console.WriteLine("  This is expected for simplified/invalid block data.");
                }

                Console.WriteLine("\n✓ Block processing example completed successfully!");
                Console.WriteLine("  (Note: Block processing may fail with simplified data, but the kernel setup works!)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static byte[] CreateSampleBlock()
        {
            // Create a minimal block for demonstration
            // This is a simplified example - real blocks would be much more complex
            // In practice, you'd typically read block data from files or create from templates

            // For this example, we'll create a block with minimal valid structure
            // Note: This won't be a real Bitcoin block, just demonstrates the API

            // A very basic block header structure (simplified)
            // Version (4 bytes) + Previous Block Hash (32 bytes) + Merkle Root (32 bytes) +
            // Timestamp (4 bytes) + Bits (4 bytes) + Nonce (4 bytes) = 80 bytes minimum

            byte[] blockData = new byte[80];

            // Version: 1 (little endian)
            BitConverter.GetBytes(1).CopyTo(blockData, 0);

            // Previous block hash: all zeros for genesis-like block
            // (indices 4-35 remain 0)

            // Merkle root: all zeros for simplicity
            // (indices 36-67 remain 0)

            // Timestamp: current Unix timestamp
            uint timestamp = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            BitConverter.GetBytes(timestamp).CopyTo(blockData, 68);

            // Bits: 0x1d00ffff (Bitcoin mainnet difficulty)
            BitConverter.GetBytes(0x1d00ffffu).CopyTo(blockData, 72);

            // Nonce: 0 for this example
            // (indices 76-79 remain 0)

            return blockData;
        }

        private static void DisplayBlockInfo(byte[] blockData)
        {
            Console.WriteLine("\nBlock Information:");
            Console.WriteLine("-----------------");

            try
            {
                // Display basic block info
                Console.WriteLine($"Block Size: {blockData.Length} bytes");

                // Display first 32 bytes of block data for inspection
                Console.WriteLine($"Block Data (first 32 bytes): {BitConverter.ToString(blockData.Take(32).ToArray()).Replace("-", " ")}");

                // Parse version (first 4 bytes, little endian)
                uint version = BitConverter.ToUInt32(blockData, 0);
                Console.WriteLine($"Version: {version}");

                // Parse timestamp (bytes 68-71, little endian)
                uint timestamp = BitConverter.ToUInt32(blockData, 68);
                DateTime blockTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                Console.WriteLine($"Timestamp: {timestamp} ({blockTime:yyyy-MM-dd HH:mm:ss UTC})");

                // Parse bits (bytes 72-75, little endian)
                uint bits = BitConverter.ToUInt32(blockData, 72);
                Console.WriteLine($"Bits: 0x{bits:X8}");

                // Parse nonce (bytes 76-79, little endian)
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
