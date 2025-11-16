using BitcoinKernel.Core.Abstractions;
using Xunit;

namespace BitcoinKernel.Core.Tests
{
    public class BlockTests
    {
        private List<byte[]> ReadBlockData()
        {
            var blockData = new List<byte[]>();
            var testAssemblyDir = Path.GetDirectoryName(typeof(BlockTests).Assembly.Location);
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

        [Fact]
        public void TestBlockTransactionsIterator()
        {
            var blockData = ReadBlockData();

            using var block = Block.FromBytes(blockData[5]);

            // Test that iterator count matches transaction count
            var txCountViaIterator = block.GetTransactions().Count();
            Assert.Equal(block.TransactionCount, txCountViaIterator);

            // Test collecting all transactions
            var txs = block.GetTransactions().ToList();
            Assert.Equal(block.TransactionCount, txs.Count);

            // Test that each transaction from iterator matches transaction by index
            int i = 0;
            foreach (var tx in block.GetTransactions())
            {
                using var txViaIndex = block.GetTransaction(i);
                Assert.NotNull(txViaIndex);
                Assert.Equal(tx.InputCount, txViaIndex.InputCount);
                Assert.Equal(tx.OutputCount, txViaIndex.OutputCount);
                i++;
            }

            // Test iterator length decreases as we consume it
            var iter = block.GetTransactions().GetEnumerator();
            var initialLen = block.TransactionCount;

            // Move to first element
            Assert.True(iter.MoveNext());
            // After consuming one element, we have initialLen - 1 remaining
            var remaining = block.GetTransactions().Skip(1).Count();
            Assert.Equal(initialLen - 1, remaining);

            // Test skipping coinbase transaction
            var nonCoinbaseTxs = block.GetTransactions().Skip(1).ToList();
            Assert.Equal(block.TransactionCount - 1, nonCoinbaseTxs.Count);
        }
    }
}
