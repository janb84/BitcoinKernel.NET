using BitcoinKernel;
using BitcoinKernel.Primitives;
using BitcoinKernel.BlockProcessing;
using BitcoinKernel.Chain;
using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop.Enums;
using Xunit;

namespace BitcoinKernel.Tests;

public class BlockHeaderTests : IDisposable
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

    private void SetupContext()
    {
        _chainParams = new ChainParameters(ChainType.REGTEST);
        var contextOptions = new KernelContextOptions().SetChainParams(_chainParams);
        _context = new KernelContext(contextOptions);

        _tempDir = Path.Combine(Path.GetTempPath(), $"test_blockheader_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        var options = new ChainstateManagerOptions(_context, _tempDir, Path.Combine(_tempDir, "blocks"));
        _chainstateManager = new ChainstateManager(_context, _chainParams, options);
        _blockProcessor = new BlockProcessor(_chainstateManager);
    }

    private void SetupWithBlocks()
    {
        SetupContext();

        // Process test blocks
        foreach (var rawBlock in ReadBlockData())
        {
            using var block = Block.FromBytes(rawBlock);
            _chainstateManager!.ProcessBlock(block);
        }
    }

    private static List<byte[]> ReadBlockData()
    {
        var blockData = new List<byte[]>();
        var testAssemblyDir = Path.GetDirectoryName(typeof(BlockHeaderTests).Assembly.Location);
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
    public void FromBytes_ValidHeader_ShouldCreateBlockHeader()
    {
        // Get the first 80 bytes from a block (the header)
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        using var header = BlockHeader.FromBytes(headerBytes);

        Assert.NotNull(header);
    }

    [Fact]
    public void FromBytes_NullData_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BlockHeader.FromBytes(null!));
    }

    [Fact]
    public void FromBytes_InvalidLength_ShouldThrowArgumentException()
    {
        var invalidBytes = new byte[79]; // Wrong length

        var exception = Assert.Throws<ArgumentException>(() => BlockHeader.FromBytes(invalidBytes));
        Assert.Contains("80 bytes", exception.Message);
    }

    [Fact]
    public void GetHash_ShouldReturnValidHash()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        using var header = BlockHeader.FromBytes(headerBytes);
        var hash = header.GetHash();

        Assert.NotNull(hash);
        Assert.Equal(32, hash.Length);
    }

    [Fact]
    public void GetPrevHash_ShouldReturnValidHash()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[1].Take(80).ToArray(); // Second block has a previous hash

        using var header = BlockHeader.FromBytes(headerBytes);
        var prevHash = header.GetPrevHash();

        Assert.NotNull(prevHash);
        Assert.Equal(32, prevHash.Length);
    }

    [Fact]
    public void Timestamp_ShouldReturnValidValue()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        using var header = BlockHeader.FromBytes(headerBytes);
        var timestamp = header.Timestamp;

        Assert.True(timestamp > 0);
    }

    [Fact]
    public void Bits_ShouldReturnValidValue()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        using var header = BlockHeader.FromBytes(headerBytes);
        var bits = header.Bits;

        Assert.True(bits > 0);
    }

    [Fact]
    public void Version_ShouldReturnValidValue()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        using var header = BlockHeader.FromBytes(headerBytes);
        var version = header.Version;

        Assert.True(version > 0);
    }

    [Fact]
    public void Nonce_ShouldReturnValidValue()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        using var header = BlockHeader.FromBytes(headerBytes);
        var nonce = header.Nonce;

        // Nonce can be any value including 0
        Assert.True(nonce >= 0);
    }

    [Fact]
    public void Block_GetHeader_ShouldReturnValidHeader()
    {
        var blockData = ReadBlockData();

        using var block = Block.FromBytes(blockData[0]);
        using var header = block.GetHeader();

        Assert.NotNull(header);
        Assert.True(header.Timestamp > 0);
        Assert.True(header.Version > 0);
    }

    [Fact]
    public void Block_GetHeader_HashShouldMatchBlockHash()
    {
        var blockData = ReadBlockData();

        using var block = Block.FromBytes(blockData[0]);
        var blockHash = block.GetHash();

        using var header = block.GetHeader();
        var headerHash = header.GetHash();

        Assert.Equal(blockHash, headerHash);
    }

    [Fact]
    public void BlockIndex_GetBlockHeader_ShouldReturnValidHeader()
    {
        SetupWithBlocks();

        var chain = _chainstateManager!.GetActiveChain();
        var tip = chain.GetTip();

        using var header = tip.GetBlockHeader();

        Assert.NotNull(header);
        Assert.True(header.Timestamp > 0);
        Assert.True(header.Version > 0);
    }

    [Fact]
    public void BlockIndex_GetBlockHeader_HashShouldMatchIndexHash()
    {
        SetupWithBlocks();

        var chain = _chainstateManager!.GetActiveChain();
        var tip = chain.GetTip();
        var tipHash = tip.GetBlockHash();

        using var header = tip.GetBlockHeader();
        var headerHash = header.GetHash();

        Assert.Equal(tipHash, headerHash);
    }

    [Fact]
    public void ChainstateManager_GetBestBlockIndex_ShouldReturnTip()
    {
        SetupWithBlocks();

        var bestIndex = _chainstateManager!.GetBestBlockIndex();
        var chain = _chainstateManager.GetActiveChain();
        var tip = chain.GetTip();

        Assert.Equal(tip.Height, bestIndex.Height);
        Assert.Equal(tip.GetBlockHash(), bestIndex.GetBlockHash());
    }

    [Fact]
    public void ChainstateManager_ProcessBlockHeader_ValidHeader_ShouldSucceed()
    {
        SetupContext();

        // Process the first block to establish genesis
        var blockData = ReadBlockData();
        using var genesisBlock = Block.FromBytes(blockData[0]);
        _chainstateManager!.ProcessBlock(genesisBlock);

        // Get the second block's header and process it
        var secondBlockHeaderBytes = blockData[1].Take(80).ToArray();
        using var secondHeader = BlockHeader.FromBytes(secondBlockHeaderBytes);

        var success = _chainstateManager.ProcessBlockHeader(secondHeader, out var validationState);

        Assert.True(success);
        Assert.Equal(ValidationMode.VALID, validationState.ValidationMode);

        validationState.Dispose();
    }

    [Fact]
    public void ChainstateManager_ProcessBlockHeader_InvalidHeader_ShouldFail()
    {
        SetupContext();

        // Create an invalid header (all zeros won't be valid)
        var invalidHeaderBytes = new byte[80];
        using var invalidHeader = BlockHeader.FromBytes(invalidHeaderBytes);

        var success = _chainstateManager!.ProcessBlockHeader(invalidHeader, out var validationState);

        Assert.False(success);
        Assert.NotEqual(ValidationMode.VALID, validationState.ValidationMode);

        validationState.Dispose();
    }

    [Fact]
    public void Dispose_ShouldAllowMultipleCalls()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        var header = BlockHeader.FromBytes(headerBytes);

        header.Dispose();
        header.Dispose(); // Should not throw
    }

    [Fact]
    public void AccessAfterDispose_ShouldThrowObjectDisposedException()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        var header = BlockHeader.FromBytes(headerBytes);
        header.Dispose();

        Assert.Throws<ObjectDisposedException>(() => header.Timestamp);
    }

    [Fact]
    public void GetHash_AfterDispose_ShouldThrowObjectDisposedException()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        var header = BlockHeader.FromBytes(headerBytes);
        header.Dispose();

        Assert.Throws<ObjectDisposedException>(() => header.GetHash());
    }

    [Fact]
    public void MultipleHeaders_FromSameBlock_ShouldHaveSameProperties()
    {
        var blockData = ReadBlockData();
        var headerBytes = blockData[0].Take(80).ToArray();

        using var header1 = BlockHeader.FromBytes(headerBytes);
        using var header2 = BlockHeader.FromBytes(headerBytes);

        Assert.Equal(header1.Version, header2.Version);
        Assert.Equal(header1.Timestamp, header2.Timestamp);
        Assert.Equal(header1.Bits, header2.Bits);
        Assert.Equal(header1.Nonce, header2.Nonce);
        Assert.Equal(header1.GetHash(), header2.GetHash());
    }

    [Fact]
    public void BlockHeader_PrevHashForGenesisBlock_ShouldBeNonNull()
    {
        var blockData = ReadBlockData();
        var genesisHeaderBytes = blockData[0].Take(80).ToArray();

        using var header = BlockHeader.FromBytes(genesisHeaderBytes);
        var prevHash = header.GetPrevHash();

        // Genesis block's previous hash should be readable (32 bytes)
        // Note: Regtest genesis may not have all zeros like mainnet
        Assert.NotNull(prevHash);
        Assert.Equal(32, prevHash.Length);
    }

    [Fact]
    public void BlockHeader_ChainedHeaders_PrevHashShouldMatchPreviousBlockHash()
    {
        var blockData = ReadBlockData();

        // Get first two blocks
        var firstHeaderBytes = blockData[0].Take(80).ToArray();
        var secondHeaderBytes = blockData[1].Take(80).ToArray();

        using var firstHeader = BlockHeader.FromBytes(firstHeaderBytes);
        using var secondHeader = BlockHeader.FromBytes(secondHeaderBytes);

        var firstBlockHash = firstHeader.GetHash();
        var secondPrevHash = secondHeader.GetPrevHash();

        // Second block's prevHash should match first block's hash
        Assert.Equal(firstBlockHash, secondPrevHash);
    }
}
