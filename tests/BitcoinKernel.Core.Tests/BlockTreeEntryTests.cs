using BitcoinKernel.Core.BlockProcessing;
using BitcoinKernel.Core.Chain;
using BitcoinKernel.Interop.Enums;


namespace BitcoinKernel.Core.Tests;

public class BlockTreeEntryTests : IDisposable
{
    private KernelContext? _context;
    private ChainParameters? _chainParams;
    private ChainstateManager? _chainstateManager;
    private BlockProcessor? _blockProcessor;
    private string? _tempDir;

    public void Dispose()
    {
        _chainstateManager?.Dispose();
        _chainParams?.Dispose();
        _context?.Dispose();

        if (!string.IsNullOrEmpty(_tempDir) && Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private void SetupWithBlocks()
    {
        _chainParams = new ChainParameters(ChainType.REGTEST);
        var contextOptions = new KernelContextOptions().SetChainParams(_chainParams);
        _context = new KernelContext(contextOptions);

        _tempDir = Path.Combine(Path.GetTempPath(), $"test_blocktreeentry_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        var options = new ChainstateManagerOptions(_context, _tempDir, Path.Combine(_tempDir, "blocks"));
        _chainstateManager = new ChainstateManager(_context, _chainParams, options);
        _blockProcessor = new BlockProcessor(_chainstateManager);

        // Process test blocks
        foreach (var rawBlock in ReadBlockData())
        {
            using var block = Abstractions.Block.FromBytes(rawBlock);
            _chainstateManager.ProcessBlock(block);
        }
    }

    private static List<byte[]> ReadBlockData()
    {
        var blockData = new List<byte[]>();
        var testAssemblyDir = Path.GetDirectoryName(typeof(BlockTreeEntryTests).Assembly.Location);
        var projectDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(testAssemblyDir)));
        var blockDataFile = Path.Combine(projectDir!, "TestData", "block_data.txt");

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
    public void Equals_SameBlock_ReturnsTrue()
    {
        SetupWithBlocks();
        var tipHash = _chainstateManager!.GetActiveChain().GetTip().GetBlockHash();

        var entry1 = _blockProcessor!.GetBlockTreeEntry(tipHash);
        var entry2 = _blockProcessor.GetBlockTreeEntry(tipHash);

        Assert.True(entry1!.Equals(entry2));
    }

    [Fact]
    public void Equals_DifferentBlocks_ReturnsFalse()
    {
        SetupWithBlocks();
        var chain = _chainstateManager!.GetActiveChain();

        var tipEntry = _blockProcessor!.GetBlockTreeEntry(chain.GetTip().GetBlockHash());
        var genesisEntry = _blockProcessor.GetBlockTreeEntry(chain.GetBlockByHeight(0)!.GetBlockHash());

        Assert.False(tipEntry!.Equals(genesisEntry));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        SetupWithBlocks();
        var tipHash = _chainstateManager!.GetActiveChain().GetTip().GetBlockHash();

        var entry = _blockProcessor!.GetBlockTreeEntry(tipHash);

        Assert.False(entry!.Equals(null));
    }

    [Fact]
    public void GetHashCode_EqualEntries_ReturnsSameHashCode()
    {
        SetupWithBlocks();
        var tipHash = _chainstateManager!.GetActiveChain().GetTip().GetBlockHash();

        var entry1 = _blockProcessor!.GetBlockTreeEntry(tipHash);
        var entry2 = _blockProcessor.GetBlockTreeEntry(tipHash);

        Assert.Equal(entry1!.GetHashCode(), entry2!.GetHashCode());
    }

    [Fact]
    public void OperatorEquals_SameBlock_ReturnsTrue()
    {
        SetupWithBlocks();
        var tipHash = _chainstateManager!.GetActiveChain().GetTip().GetBlockHash();

        var entry1 = _blockProcessor!.GetBlockTreeEntry(tipHash);
        var entry2 = _blockProcessor.GetBlockTreeEntry(tipHash);

        Assert.True(entry1 == entry2);
    }

    [Fact]
    public void OperatorNotEquals_DifferentBlocks_ReturnsTrue()
    {
        SetupWithBlocks();
        var chain = _chainstateManager!.GetActiveChain();

        var tipEntry = _blockProcessor!.GetBlockTreeEntry(chain.GetTip().GetBlockHash());
        var genesisEntry = _blockProcessor.GetBlockTreeEntry(chain.GetBlockByHeight(0)!.GetBlockHash());

        Assert.True(tipEntry != genesisEntry);
    }

    [Fact]
    public void GetPrevious_EqualEntries_ReturnEqualPrevious()
    {
        SetupWithBlocks();
        var tipHash = _chainstateManager!.GetActiveChain().GetTip().GetBlockHash();

        var entry1 = _blockProcessor!.GetBlockTreeEntry(tipHash);
        var entry2 = _blockProcessor.GetBlockTreeEntry(tipHash);

        var prev1 = entry1!.GetPrevious();
        var prev2 = entry2!.GetPrevious();

        Assert.True(prev1!.Equals(prev2));
    }
}
