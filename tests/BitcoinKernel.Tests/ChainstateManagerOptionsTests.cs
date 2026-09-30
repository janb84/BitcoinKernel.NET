using BitcoinKernel.Chain;
using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop.Enums;
using Xunit;

namespace BitcoinKernel.Tests;

public class ChainstateManagerOptionsTests : IDisposable
{
    private const ulong MiB = 1024 * 1024;

    private readonly ChainParameters _chainParams;
    private readonly KernelContextOptions _contextOptions;
    private readonly KernelContext _context;
    private readonly string _dataDir;

    public ChainstateManagerOptionsTests()
    {
        _chainParams = new ChainParameters(ChainType.REGTEST);
        _contextOptions = new KernelContextOptions().SetChainParams(_chainParams);
        _context = new KernelContext(_contextOptions);
        _dataDir = Path.Combine(Path.GetTempPath(), $"btck_options_test_{Guid.NewGuid()}");
    }

    public void Dispose()
    {
        _context.Dispose();
        _contextOptions.Dispose();
        _chainParams.Dispose();
        if (Directory.Exists(_dataDir))
            Directory.Delete(_dataDir, recursive: true);
    }

    private ChainstateManagerOptions CreateOptions() =>
        new(_context, _dataDir, Path.Combine(_dataDir, "blocks"));

    [Fact]
    public void SetDatabaseCacheBytes_ValidSize_ReturnsSameInstance()
    {
        using var options = CreateOptions();

        Assert.Same(options, options.SetDatabaseCacheBytes(64 * MiB));
    }

    [Fact]
    public void SetDatabaseCacheBytes_AtMinimum_IsAccepted()
    {
        using var options = CreateOptions();

        options.SetDatabaseCacheBytes(4 * MiB);
    }

    [Fact]
    public void SetDatabaseCacheBytes_OptionsCreateChainstateManager()
    {
        using var options = CreateOptions()
            .SetDatabaseCacheBytes(16 * MiB)
            .SetBlockTreeDbInMemory(true)
            .SetChainstateDbInMemory(true);
        using var manager = new ChainstateManager(_context, _chainParams, options);

        Assert.Equal(0, manager.GetActiveChain().Height);
    }

    [Fact]
    public void SetDatabaseCacheBytes_BelowMinimum_Throws()
    {
        using var options = CreateOptions();

        Assert.Throws<KernelException>(() => options.SetDatabaseCacheBytes(4 * MiB - 1));
    }
}
