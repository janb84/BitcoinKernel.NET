using System;
using BitcoinKernel;

namespace FacadeExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Bitcoin Kernel Basic Builder Example ===\n");

            FullChainstateExample();
            
        }

        static void FullChainstateExample()
        {
            Console.WriteLine("2. Full Chainstate Example:");

            Console.WriteLine("   Creating builder...");
            var builder = KernelLibrary.Create()
                .ForMainnet()
                .WithWorkerThreads(2)
                .WithDirectories("/tmp/regtest-data2", "/tmp/regtest-data/blocks2");
            
            Console.WriteLine("   Configuring logging...");
            builder = builder.WithLogging((category, message, level) =>
            {
                if (level <= (int)BitcoinKernel.Interop.Enums.LogLevel.INFO) // Only INFO and above
                    Console.WriteLine($"   [{category}] {message}");
            });
            
            Console.WriteLine("   Building kernel...");
            using var kernel = builder.Build();
            
            Console.WriteLine("   Kernel built successfully!");
            Console.WriteLine("   ✓ Chainstate initialized automatically");

            // Process blocks
            try
            {

                Console.WriteLine("   ✓ Ready to process blocks");

                // Show new query methods
                Console.WriteLine($"   Chain height: {kernel.GetChainHeight()}");
                Console.WriteLine($"   Genesis hash: {Convert.ToHexString(kernel.GetGenesisBlockHash())}");
                
                if (kernel.GetChainHeight() > 0)
                {
                    var tipHash = kernel.GetChainTipHash();
                    Console.WriteLine($"   Tip hash: {Convert.ToHexString(tipHash)}");
                    
                    var blockInfo = kernel.GetBlockInfo(0);
                    if (blockInfo != null)
                    {
                        Console.WriteLine($"   Block 0 hash: {Convert.ToHexString(blockInfo.Hash)}");
                    }
                }

                Console.WriteLine("   ✓ Chain queries working");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Error: {ex.Message}");
            }

            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            
            kernel.Dispose();
            Console.WriteLine("   Kernel disposed.");
        }
    }
}