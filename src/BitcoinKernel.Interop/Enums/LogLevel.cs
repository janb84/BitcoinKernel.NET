namespace BitcoinKernel.Interop.Enums;

// btck_LogLevel is a uint8_t in bitcoinkernel.h.
public enum LogLevel : byte
{
    TRACE = 0,
    DEBUG = 1,
    INFO = 2,
    WARN = 3,
    ERROR = 4,
    FATAL = 5,
    NONE = 6
}