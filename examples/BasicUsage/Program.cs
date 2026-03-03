using BitcoinKernel;
using BitcoinKernel.Chain;
using BitcoinKernel.Interop.Enums;

namespace BasicUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Bitcoin Kernel Basic Usage Example ===\n");
            FullChainstateExample();
        }

        static void FullChainstateExample()
        {
            Console.WriteLine("Creating kernel...");

            var dataDir = "/tmp/regtest-data2";
            var blocksDir = "/tmp/regtest-data/blocks2";

            using var logging = new LoggingConnection((category, message, level) =>
            {
                if (level <= (int)LogLevel.INFO)
                    Console.WriteLine($"   [{category}] {message}");
            });

            using var chainParams = new ChainParameters(ChainType.MAINNET);
            using var contextOptions = new KernelContextOptions().SetChainParams(chainParams);
            using var context = new KernelContext(contextOptions);
            using var options = new ChainstateManagerOptions(context, dataDir, blocksDir)
                .SetWorkerThreads(2);
            using var chainstate = new ChainstateManager(context, chainParams, options);

            Console.WriteLine("   Kernel created successfully!");

            try
            {
                var chain = chainstate.GetActiveChain();
                Console.WriteLine($"   Chain height: {chain.Height}");
                Console.WriteLine($"   Genesis hash: {Convert.ToHexString(chain.GetGenesis().GetBlockHash())}");

                if (chain.Height > 0)
                {
                    var tip = chain.GetTip();
                    Console.WriteLine($"   Tip hash: {Convert.ToHexString(tip.GetBlockHash())}");

                    var genesis = chain.GetBlockByHeight(0);
                    if (genesis != null)
                        Console.WriteLine($"   Block 0 hash: {Convert.ToHexString(genesis.GetBlockHash())}");
                }

                Console.WriteLine("   Chain queries working");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
