using BitcoinKernel.Chain;
using Xunit;

namespace BitcoinKernel.Tests;

public class ChainParametersTests
{
    // 1-of-2 multisig challenge used by the default public signet.
    private const string DefaultSignetChallengeHex =
        "512103ad5e0edad18cb1f0fc0d28a3d4f1f3e445640337489abb10404f2d1e086be430210359ef5021964fe22d6f8e05b2463c9540ce96883fe3b278760f048f5189f2e6c452ae";

    [Fact]
    public void CreateSignet_WithChallenge_CanBackAContext()
    {
        using var chainParams = ChainParameters.CreateSignet(Convert.FromHexString(DefaultSignetChallengeHex));
        using var options = new KernelContextOptions().SetChainParams(chainParams);
        using var context = new KernelContext(options);

        Assert.NotNull(context);
    }

    // The kernel does not parse the challenge when building the parameters; it only
    // matters once a block's signet solution is checked against it.
    [Theory]
    [InlineData("")]
    [InlineData("ff")]
    public void CreateSignet_WithEmptyOrNonScriptChallenge_Succeeds(string challengeHex)
    {
        using var chainParams = ChainParameters.CreateSignet(Convert.FromHexString(challengeHex));

        Assert.NotNull(chainParams);
    }

    [Fact]
    public void CreateSignet_WithNullChallenge_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ChainParameters.CreateSignet(null!));
    }
}
