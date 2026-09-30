using BitcoinKernel.Chain;
using BitcoinKernel.Interop.Enums;
using BitcoinKernel.Primitives;
using Xunit;

namespace BitcoinKernel.Tests;

/// <summary>
/// Mock time is process-wide, so these tests must not overlap with any other
/// test that validates blocks or headers.
/// </summary>
[CollectionDefinition(nameof(MockTimeCollection), DisableParallelization = true)]
public class MockTimeCollection;

[Collection(nameof(MockTimeCollection))]
public class MockTimeTests
{
    // Before the 2024-04-27 timestamp of the first test block.
    private const long BeforeTestBlocks = 1_700_000_000;

    [Fact]
    public void SetAndReset_ValidTimestamp_Succeeds()
    {
        try
        {
            MockTime.Set(BeforeTestBlocks);
            MockTime.Set(DateTimeOffset.FromUnixTimeSeconds(uint.MaxValue));
        }
        finally
        {
            MockTime.Reset();
        }
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData((long)uint.MaxValue + 1)]
    public void Set_OutOfRange_Throws(long unixSeconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MockTime.Set(unixSeconds));
    }

    [Fact]
    public void Set_BeforeHeaderTimestamp_RejectsHeaderAsTimeFuture()
    {
        var dataDir = Path.Combine(Path.GetTempPath(), $"test_mocktime_{Guid.NewGuid()}");
        try
        {
            using var chainParams = new ChainParameters(ChainType.REGTEST);
            using var contextOptions = new KernelContextOptions().SetChainParams(chainParams);
            using var context = new KernelContext(contextOptions);
            using var options = new ChainstateManagerOptions(context, dataDir, Path.Combine(dataDir, "blocks"));
            using var manager = new ChainstateManager(context, chainParams, options);
            using var header = BlockHeader.FromBytes(ReadFirstBlock().Take(80).ToArray());

            MockTime.Set(BeforeTestBlocks);
            Assert.False(manager.ProcessBlockHeader(header, out var rejected));
            using (rejected)
            {
                Assert.Equal(BlockValidationResult.TIME_FUTURE, rejected.ValidationResult);
            }

            MockTime.Reset();
            Assert.True(manager.ProcessBlockHeader(header, out var accepted));
            accepted.Dispose();
        }
        finally
        {
            MockTime.Reset();
            if (Directory.Exists(dataDir))
                Directory.Delete(dataDir, recursive: true);
        }
    }

    private static byte[] ReadFirstBlock()
    {
        var testAssemblyDir = Path.GetDirectoryName(typeof(MockTimeTests).Assembly.Location);
        var projectDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(testAssemblyDir)));
        var blockDataFile = Path.Combine(projectDir!, "TestData", "block_data.txt");

        return Convert.FromHexString(File.ReadLines(blockDataFile).First(l => !string.IsNullOrWhiteSpace(l)).Trim());
    }
}
