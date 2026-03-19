using BitcoinKernel.Primitives;
using Xunit;

namespace BitcoinKernel.Tests;

public class TransactionTests
{
    // Legacy P2PKH transaction — version 2, locktime 510826, sequence 0xFFFFFFFE
    private const string LegacyTxHex =
        "02000000013f7cebd65c27431a90bba7f796914fe8cc2ddfc3f2cbd6f7e5f2fc854534da95" +
        "000000006b483045022100de1ac3bcdfb0332207c4a91f3832bd2c2915840165f876ab47c5" +
        "f8996b971c3602201c6c053d750fadde599e6f5c4e1963df0f01fc0d97815e8157e3d59fe0" +
        "9ca30d012103699b464d1d8bc9e47d4fb1cdaa89a1c5783d68363c4dbc4b524ed3d857148617" +
        "feffffff02836d3c01000000001976a914fc25d6d5c94003bf5b0c7b640a248e2c637fcfb0" +
        "88ac7ada8202000000001976a914fbed3d9b11183209a57999d54d59f67c019e756c88ac6acb0700";

    // Segwit P2SH transaction — version 1, locktime 0, sequence 0xFFFFFFFF
    private const string SegwitTxHex =
        "01000000000101d9fd94d0ff0026d307c994d0003180a5f248146efb6371d040c5973f5f66d9" +
        "df0400000017160014b31b31a6cb654cfab3c50567bcf124f48a0beaecffffffff012cbd1c00" +
        "0000000017a914233b74bf0823fa58bbbd26dfc3bb4ae715547167870247304402206f60569c" +
        "ac136c114a58aedd80f6fa1c51b49093e7af883e605c212bdafcd8d202200e91a55f408a021a" +
        "d2631bc29a67bd6915b2d7e9ef0265627eabd7f7234455f6012103e7e802f50344303c76d12c" +
        "089c8724c1b230e3b745693bbe16aad536293d15e300000000";

    [Fact]
    public void LockTime_LegacyTransaction_ReturnsCorrectValue()
    {
        using var tx = Transaction.FromHex(LegacyTxHex);
        Assert.Equal(510826u, tx.LockTime);
    }

    [Fact]
    public void LockTime_SegwitTransaction_ReturnsZero()
    {
        using var tx = Transaction.FromHex(SegwitTxHex);
        Assert.Equal(0u, tx.LockTime);
    }

    [Fact]
    public void GetInputAt_ReturnsTxIn()
    {
        using var tx = Transaction.FromHex(LegacyTxHex);
        using var input = tx.GetInputAt(0);
        Assert.NotNull(input);
        Assert.IsType<TxIn>(input);
    }

    [Fact]
    public void GetInputAt_OutOfRange_Throws()
    {
        using var tx = Transaction.FromHex(LegacyTxHex);
        Assert.Throws<ArgumentOutOfRangeException>(() => tx.GetInputAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => tx.GetInputAt(tx.InputCount));
    }

    [Fact]
    public void TxIn_Sequence_LegacyTransaction_ReturnsCorrectValue()
    {
        using var tx = Transaction.FromHex(LegacyTxHex);
        using var input = tx.GetInputAt(0);
        // Sequence 0xFFFFFFFE = 4294967294 (RBF signalling)
        Assert.Equal(0xFFFFFFFEu, input.Sequence);
    }

    [Fact]
    public void TxIn_Sequence_SegwitTransaction_ReturnsMaxValue()
    {
        using var tx = Transaction.FromHex(SegwitTxHex);
        using var input = tx.GetInputAt(0);
        // Sequence 0xFFFFFFFF = final (no RBF, no relative locktime)
        Assert.Equal(0xFFFFFFFFu, input.Sequence);
    }

    [Fact]
    public void TxIn_GetOutPoint_ReturnsNonNullPointer()
    {
        using var tx = Transaction.FromHex(LegacyTxHex);
        using var input = tx.GetInputAt(0);
        var outPoint = input.GetOutPoint();
        Assert.NotEqual(IntPtr.Zero, outPoint);
    }

    [Fact]
    public void TxIn_Copy_IsIndependent()
    {
        using var tx = Transaction.FromHex(LegacyTxHex);
        using var input = tx.GetInputAt(0);
        using var copy = input.Copy();

        Assert.NotNull(copy);
        Assert.Equal(input.Sequence, copy.Sequence);
    }
}
