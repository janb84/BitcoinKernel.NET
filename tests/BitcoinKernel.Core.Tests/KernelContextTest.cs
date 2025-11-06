using BitcoinKernel.Core.Exceptions;
using Xunit;

namespace BitcoinKernel.Core.Tests;

public class KernelContextTest
{
    [Fact]
    public void Constructor_WithoutOptions_CreatesContext()
    {
        var context = new KernelContext();
        Assert.NotNull(context);
        context.Dispose();
    }

    [Fact]
    public void Constructor_WithOptions_CreatesContext()
    {
        var options = new KernelContextOptions();
        var context = new KernelContext(options);
        Assert.NotNull(context);
        context.Dispose();
        options.Dispose();
    }

    [Fact]
    public void Constructor_WithNullOptions_CreatesContext()
    {
        var context = new KernelContext(null);
        Assert.NotNull(context);
        context.Dispose();
    }

}