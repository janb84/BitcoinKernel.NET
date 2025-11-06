using System;
using BitcoinKernel.Core.ScriptVerification;
using BitcoinKernel.Core.Abstractions;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Core.Chain;
using BitcoinKernel.Interop.Enums;
using Xunit;

namespace BitcoinKernel.Core.Tests;

public class ScriptVerificationFixture : IDisposable
{
    public KernelContext Context { get; }
    public ChainParameters ChainParams { get; }

    public ScriptVerificationFixture()
    {
        // Initialize kernel context once for all script verification tests
        ChainParams = new ChainParameters(ChainType.REGTEST);
        var contextOptions = new KernelContextOptions()
            .SetChainParams(ChainParams);
        Context = new KernelContext(contextOptions);
    }

    public void Dispose()
    {
        // Match disposal order with BlockProcessingTests
        ChainParams?.Dispose();
        Context?.Dispose();
    }
}

public class ScriptVerificationTests : IClassFixture<ScriptVerificationFixture>
{
    private readonly ScriptVerificationFixture _fixture;

    public ScriptVerificationTests(ScriptVerificationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ScriptVerifyTest_OldStyleTransaction()
    {
        VerifyTest(
        "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ac",
        "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700",
        0, 0, expectSuccess: true);
    }

    [Fact]
    public void ScriptVerifyTest_P2SHTransaction()
    {
        VerifyTest(
        "a91434c06f8c87e355e123bdc6dda4ffabc64b6989ef87",
        "01000000000101d9fd94d0ff0026d307c994d0003180a5f248146efb6371d040c5973f5f66d9df0400000017160014b31b31a6cb654cfab3c50567bcf124f48a0beaecffffffff012cbd1c000000000017a914233b74bf0823fa58bbbd26dfc3bb4ae715547167870247304402206f60569cac136c114a58aedd80f6fa1c51b49093e7af883e605c212bdafcd8d202200e91a55f408a021ad2631bc29a67bd6915b2d7e9ef0265627eabd7f7234455f6012103e7e802f50344303c76d12c089c8724c1b230e3b745693bbe16aad536293d15e300000000",
        1900000, 0, expectSuccess: true);
    }

    [Fact]
    public void ScriptVerifyTest_NativeSegwitTransaction()
    {
        VerifyTest(
        "0020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d",
        "010000000001011f97548fbbe7a0db7588a66e18d803d0089315aa7d4cc28360b6ec50ef36718a0100000000ffffffff02df1776000000000017a9146c002a686959067f4866b8fb493ad7970290ab728757d29f0000000000220020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d04004730440220565d170eed95ff95027a69b313758450ba84a01224e1f7f130dda46e94d13f8602207bdd20e307f062594022f12ed5017bbf4a055a06aea91c10110a0e3bb23117fc014730440220647d2dc5b15f60bc37dc42618a370b2a1490293f9e5c8464f53ec4fe1dfe067302203598773895b4b16d37485cbe21b337f4e4b650739880098c592553add7dd4355016952210375e00eb72e29da82b89367947f29ef34afb75e8654f6ea368e0acdfd92976b7c2103a1b26313f430c4b15bb1fdce663207659d8cac749a0e53d70eff01874496feff2103c96d495bfdd5ba4145e3e046fee45e84a8a48ad05bd8dbb395c011a32cf9f88053ae00000000",
        18393430, 0, expectSuccess: true);
    }

    [Fact]
    public void ScriptVerifyTest_OldStyleTransaction_InvalidSignature()
    {
        VerifyTest(
        "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ff",
        "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700",
        0, 0, expectSuccess: false);
    }

    [Fact]
    public void ScriptVerifyTest_P2SHTransaction_WrongAmount()
    {
        VerifyTest(
        "a91434c06f8c87e355e123bdc6dda4ffabc64b6989ef87",
        "01000000000101d9fd94d0ff0026d307c994d0003180a5f248146efb6371d040c5973f5f66d9df0400000017160014b31b31a6cb654cfab3c50567bcf124f48a0beaecffffffff012cbd1c000000000017a914233b74bf0823fa58bbbd26dfc3bb4ae715547167870247304402206f60569cac136c114a58aedd80f6fa1c51b49093e7af883e605c212bdafcd8d202200e91a55f408a021ad2631bc29a67bd6915b2d7e9ef0265627eabd7f7234455f6012103e7e802f50344303c76d12c089c8724c1b230e3b745693bbe16aad536293d15e300000000",
        900000, 0, expectSuccess: false);
    }

    [Fact]
    public void ScriptVerifyTest_NativeSegwitTransaction_InvalidSegwit()
    {
        VerifyTest(
        "0020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58f",
        "010000000001011f97548fbbe7a0db7588a66e18d803d0089315aa7d4cc28360b6ec50ef36718a0100000000ffffffff02df1776000000000017a9146c002a686959067f4866b8fb493ad7970290ab728757d29f0000000000220020701a8d401c84fb13e6baf169d59684e17abd9fa216c8cc5b9fc63d622ff8c58d04004730440220565d170eed95ff95027a69b313758450ba84a01224e1f7f130dda46e94d13f8602207bdd20e307f062594022f12ed5017bbf4a055a06aea91c10110a0e3bb23117fc014730440220647d2dc5b15f60bc37dc42618a370b2a1490293f9e5c8464f53ec4fe1dfe067302203598773895b4b16d37485cbe21b337f4e4b650739880098c592553add7dd4355016952210375e00eb72e29da82b89367947f29ef34afb75e8654f6ea368e0acdfd92976b7c2103a1b26313f430c4b15bb1fdce663207659d8cac749a0e53d70eff01874496feff2103c96d495bfdd5ba4145e3e046fee45e84a8a48ad05bd8dbb395c011a32cf9f88053ae00000000",
        18393430, 0, expectSuccess: false);
    }

    private void VerifyTest(string scriptPubkeyHex, string transactionHex, long amount, int inputIndex, bool expectSuccess)
    {
        // Create array of spent output hex strings (all the same for this test)
        int inputCount = Transaction.FromHex(transactionHex).InputCount;
        List<TxOut> spentOutputs =
        [
            TxOut.Create(
                ScriptPubKey.FromHex(scriptPubkeyHex),
                amount: amount)
        ];


        try
        {
            bool verificationSuccess = ScriptVerifier.VerifyScript(
                ScriptPubKey.FromHex(scriptPubkeyHex),
                amount,
                Transaction.FromHex(transactionHex),
                (uint)inputIndex,
                spentOutputs,
                ScriptVerificationFlags.AllPreTaproot);

            Assert.Equal(expectSuccess, verificationSuccess);

        }
        catch (ScriptVerificationException ex)
        {
            if (expectSuccess)
            {
                Assert.Fail("Expected script verification to succeed but it failed");
            }
            else
            {
                Assert.Contains("Script verification failed", ex.Message);
            }
        }
    }


    [Fact]
    public void TestVerifyInputValidation_InputIndexOutOfBounds()
    {
        string scriptHex = "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ac";
        ScriptPubKey scriptPubKey = ScriptPubKey.FromHex(scriptHex);
        string txHex = "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700";
        Transaction transaction = Transaction.FromHex(txHex);
        List<TxOut> dummyOutput =
        [
            TxOut.Create(
                ScriptPubKey.FromHex(scriptHex),
                amount: 100000)
        ];

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ScriptVerifier.VerifyScript(
                scriptPubKey,
                100000,
                transaction,
                inputIndex: 999,
                dummyOutput,
                flags: ScriptVerificationFlags.AllPreTaproot));

        Assert.Contains("Input index 999 is out of bounds", ex.Message);
    }


    [Fact]
    public void TestVerifyInputValidation_SpentOutputsMismatch()
    {
        string scriptHex = "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ac";
        ScriptPubKey scriptPubKey = ScriptPubKey.FromHex(scriptHex);
        string txHex = "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700";
        Transaction transaction = Transaction.FromHex(txHex);
        List<TxOut> wrongSpentOutputs =
        [
            TxOut.Create(
                ScriptPubKey.FromHex(scriptHex),
                amount: 0),
            TxOut.Create(
                ScriptPubKey.FromHex(scriptHex),
                amount: 0)
        ];

        var ex = Assert.Throws<ScriptVerificationException>(() =>
            ScriptVerifier.VerifyScript(
                scriptPubKey,
                0,
                transaction,
                inputIndex: 0,
                wrongSpentOutputs,
                flags: ScriptVerificationFlags.All));
        Assert.Equal(ScriptVerifyStatus.ERROR_SPENT_OUTPUTS_MISMATCH, ex.Status);
    }


    [Fact]
    public void TestVerifyInputValidation_InvalidFlags()
    {
        string scriptHex = "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ac";
        ScriptPubKey scriptPubKey = ScriptPubKey.FromHex(scriptHex);
        string txHex = "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700";
        Transaction transaction = Transaction.FromHex(txHex);
        List<TxOut> dummyOutput =
        [
            TxOut.Create(
                ScriptPubKey.FromHex(scriptHex),
                amount: 100000)
        ];

        var ex = Assert.Throws<ScriptVerificationException>(() =>
            ScriptVerifier.VerifyScript(
                scriptPubKey,
                100000,
                transaction,
                inputIndex: 0,
                dummyOutput,
                flags: (ScriptVerificationFlags)0xFFFFFFFF));
        Assert.Equal(ScriptVerifyStatus.ERROR_INVALID_FLAGS, ex.Status);
    }

    [Fact]
    public void TestVerifyInputValidation_InvalidFlagsCombination()
    {
        string scriptHex = "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ac";
        ScriptPubKey scriptPubKey = ScriptPubKey.FromHex(scriptHex);
        string txHex = "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700";
        Transaction transaction = Transaction.FromHex(txHex);
        List<TxOut> dummyOutput =
        [
            TxOut.Create(
                ScriptPubKey.FromHex(scriptHex),
                amount: 100000)
        ];

        // Witness without P2SH
        var ex = Assert.Throws<ScriptVerificationException>(() =>
            ScriptVerifier.VerifyScript(
           scriptPubKey,
            100000,
            transaction,
            inputIndex: 0,
            dummyOutput,
            flags: ScriptVerificationFlags.Witness));
        Assert.Equal(ScriptVerifyStatus.ERROR_INVALID_FLAGS_COMBINATION, ex.Status);
    }

    [Fact]
    public void TestVerifyInputValidation_SpentOutputsRequired()
    {
        string scriptHex = "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ac";
        ScriptPubKey scriptPubKey = ScriptPubKey.FromHex(scriptHex);
        string txHex = "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe09ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb088ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700";
        Transaction transaction = Transaction.FromHex(txHex);
        List<TxOut> dummyOutput =
        [
            TxOut.Create(
                ScriptPubKey.FromHex(scriptHex),
                amount: 100000)
        ];

        // Taproot flag set but no spent outputs
        List<TxOut> emptySpentOutputs = Array.Empty<TxOut>().ToList();
        var ex = Assert.Throws<ScriptVerificationException>(() =>
            ScriptVerifier.VerifyScript(
           scriptPubKey,
            100000,
            transaction,
            inputIndex: 0,
            emptySpentOutputs,
            ScriptVerificationFlags.Taproot));
        Assert.Equal(ScriptVerifyStatus.ERROR_SPENT_OUTPUTS_REQUIRED, ex.Status);
    }
}
