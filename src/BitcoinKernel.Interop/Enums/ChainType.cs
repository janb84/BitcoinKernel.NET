namespace BitcoinKernel.Interop.Enums;

// btck_ChainType is a uint8_t in bitcoinkernel.h.
public enum ChainType : byte
{
    MAINNET = 0,
    TESTNET = 1,
    TESTNET_4 = 2,
    SIGNET = 3,
    REGTEST = 4
}
