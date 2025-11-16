using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Core.Exceptions;

/// <summary>
/// Base exception for all Bitcoin Kernel-related errors.
/// </summary>
public class KernelException : Exception
{
    public KernelException() : base() { }

    public KernelException(string message) : base(message) { }

    public KernelException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when kernel context initialization fails.
/// </summary>
public class KernelContextException : KernelException
{
    public KernelContextException() : base() { }

    public KernelContextException(string message) : base(message) { }

    public KernelContextException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when script verification fails.
/// </summary>
public class ScriptVerificationException : KernelException
{
    public ScriptVerifyStatus Status { get; }

    public ScriptVerificationException(ScriptVerifyStatus status)
        : base($"Script verification failed with status: {status}")
    {
        Status = status;
    }

    public ScriptVerificationException(ScriptVerifyStatus status, string message)
        : base(message)
    {
        Status = status;
    }

    public ScriptVerificationException(ScriptVerifyStatus status, string message, Exception innerException)
        : base(message, innerException)
    {
        Status = status;
    }
}

/// <summary>
/// Exception thrown when block validation fails.
/// </summary>
public class BlockValidationException : KernelException
{
    public BlockValidationResult Result { get; }
    public ValidationMode Mode { get; }

    public BlockValidationException(BlockValidationResult result, ValidationMode mode)
        : base($"Block validation failed: {result} (Mode: {mode})")
    {
        Result = result;
        Mode = mode;
    }

    public BlockValidationException(BlockValidationResult result, ValidationMode mode, string message)
        : base(message)
    {
        Result = result;
        Mode = mode;
    }

    public BlockValidationException(BlockValidationResult result, ValidationMode mode, string message, Exception innerException)
        : base(message, innerException)
    {
        Result = result;
        Mode = mode;
    }
}

/// <summary>
/// Exception thrown when chainstate manager operations fail.
/// </summary>
public class ChainstateManagerException : KernelException
{
    public ChainstateManagerException() : base() { }

    public ChainstateManagerException(string message) : base(message) { }

    public ChainstateManagerException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when logging operations fail.
/// </summary>
public class LoggingException : KernelException
{
    public LoggingException() : base() { }

    public LoggingException(string message) : base(message) { }

    public LoggingException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when chain parameter operations fail.
/// </summary>
public class ChainParametersException : KernelException
{
    public ChainType? ChainType { get; }

    public ChainParametersException() : base() { }

    public ChainParametersException(string message) : base(message) { }

    public ChainParametersException(ChainType chainType, string message)
        : base(message)
    {
        ChainType = chainType;
    }

    public ChainParametersException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a fatal error occurs in the kernel.
/// This typically indicates a system error that requires halting operations.
/// </summary>
public class KernelFatalException : KernelException
{
    public KernelFatalException() : base() { }

    public KernelFatalException(string message)
        : base($"Fatal kernel error: {message}") { }

    public KernelFatalException(string message, Exception innerException)
        : base($"Fatal kernel error: {message}", innerException) { }
}

/// <summary>
/// Exception thrown when a flush error occurs during database operations.
/// </summary>
public class KernelFlushException : KernelException
{
    public KernelFlushException() : base() { }

    public KernelFlushException(string message)
        : base($"Kernel flush error: {message}") { }

    public KernelFlushException(string message, Exception innerException)
        : base($"Kernel flush error: {message}", innerException) { }
}

/// <summary>
/// Exception thrown when transaction operations fail.
/// </summary>
public class TransactionException : KernelException
{
    public TransactionException() : base() { }

    public TransactionException(string message) : base(message) { }

    public TransactionException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when block operations fail (reading, parsing, etc.).
/// </summary>
public class BlockException : KernelException
{
    public BlockException() : base() { }

    public BlockException(string message) : base(message) { }

    public BlockException(string message, Exception innerException)
        : base(message, innerException) { }
}
