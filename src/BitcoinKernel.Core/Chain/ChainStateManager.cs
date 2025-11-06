using System;
using System.Runtime.InteropServices;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;
using BitcoinKernel.Core.Abstractions;

namespace BitcoinKernel.Core.Chain;

/// <summary>
/// Manages the blockchain state and validation operations.
/// </summary>
public sealed class ChainstateManager : IDisposable
{
    private IntPtr _handle;
    private readonly KernelContext _context;
    private bool _disposed;

    private readonly ChainstateManagerOptions _chainstateManagerOptions;

    /// <summary>
    /// Creates a new chainstate manager.
    /// </summary>
    public ChainstateManager(
        KernelContext context,
        ChainParameters chainParams,
        ChainstateManagerOptions options)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (chainParams == null) throw new ArgumentNullException(nameof(chainParams));
        if (options == null) throw new ArgumentNullException(nameof(options));

        _context = context;
        _chainstateManagerOptions = options;
        _handle = NativeMethods.ChainstateManagerCreate(options.Handle);

        if (_handle == IntPtr.Zero)
        {
            throw new ChainstateManagerException("Failed to create chainstate manager. Check the log messages for more details.");
        }
    }

    /// <summary>
    /// Gets the native handle.
    /// </summary>
    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    /// <summary>
    /// Gets the active chain.
    /// </summary>
    public Abstractions.Chain GetActiveChain()
    {
        ThrowIfDisposed();
        IntPtr chainPtr = NativeMethods.ChainstateManagerGetActiveChain(_handle);
        if (chainPtr == IntPtr.Zero)
            throw new KernelException("Failed to get active chain");

        return new Abstractions.Chain(chainPtr);
    }

    /// <summary>
    /// Processes a block through validation.
    /// </summary>
    public bool ProcessBlock(Block block)
    {
        ThrowIfDisposed();
        if (block == null) throw new ArgumentNullException(nameof(block));

        int newBlock;
        int result = NativeMethods.ChainstateManagerProcessBlock(
            _handle,
            block.Handle,
            out newBlock);

        if (result != 0)
        {
            throw new ChainstateManagerException($"Failed to process block (error code: {result})");
        }

        return newBlock != 0;
    }

    /// <summary>
    /// Imports blocks from an array of block files.
    /// </summary>
    /// <param name="blockFilePaths">Array of paths to block files to import.</param>
    /// <returns>True if import was successful, false otherwise.</returns>
    public bool ImportBlocks(string[] blockFilePaths)
    {
        ThrowIfDisposed();
        if (blockFilePaths == null) throw new ArgumentNullException(nameof(blockFilePaths));
        if (blockFilePaths.Length == 0) throw new ArgumentException("At least one block file path must be provided", nameof(blockFilePaths));

        // Prepare the string data for marshalling
        IntPtr[] stringPointers = new IntPtr[blockFilePaths.Length];
        nuint[] stringLengths = new nuint[blockFilePaths.Length];

        try
        {
            // Pin each string and get its pointer and length
            for (int i = 0; i < blockFilePaths.Length; i++)
            {
                if (string.IsNullOrEmpty(blockFilePaths[i]))
                    throw new ArgumentException($"Block file path at index {i} cannot be null or empty", nameof(blockFilePaths));

                unsafe
                {
                    fixed (char* strPtr = blockFilePaths[i])
                    {
                        // Convert UTF-16 string to UTF-8 bytes
                        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(blockFilePaths[i]);
                        stringPointers[i] = Marshal.AllocHGlobal(utf8Bytes.Length + 1);
                        Marshal.Copy(utf8Bytes, 0, stringPointers[i], utf8Bytes.Length);
                        Marshal.WriteByte(stringPointers[i], utf8Bytes.Length, 0); // Null terminator
                        stringLengths[i] = (nuint)utf8Bytes.Length;
                    }
                }
            }

            // Call the native method
            int result = NativeMethods.ChainstateManagerImportBlocks(
                _handle,
                stringPointers,
                stringLengths,
                (nuint)blockFilePaths.Length);

            return result == 0; // 0 = success
        }
        finally
        {
            // Free allocated memory
            foreach (var ptr in stringPointers)
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
    }

    /// <summary>
    /// Imports blocks from the configured blocks directory.
    /// </summary>
    /// <returns>True if import was successful, false otherwise.</returns>
    public bool ImportBlocks()
    {
        return ImportBlocks([_chainstateManagerOptions.BlocksDirectory]);
    }

    /// <summary>
    /// Reads the spent outputs (coins) for a specific block.
    /// </summary>
    /// <param name="blockIndex">The block index to read spent outputs for.</param>
    /// <returns>A BlockSpentOutputs object containing all spent outputs in the block.</returns>
    public BlockSpentOutputs ReadSpentOutputs(BlockIndex blockIndex)
    {
        ThrowIfDisposed();
        if (blockIndex == null) throw new ArgumentNullException(nameof(blockIndex));

        var spentOutputsPtr = NativeMethods.BlockSpentOutputsRead(_handle, blockIndex.Handle);
        if (spentOutputsPtr == IntPtr.Zero)
        {
            throw new ChainstateManagerException($"Failed to read spent outputs for block at height {blockIndex.Height}");
        }

        return new BlockSpentOutputs(spentOutputsPtr);
    }

    private string GetErrorMessage(IntPtr error)
    {
        // Note: Error objects don't exist in the API - errors are communicated through return values
        // and validation callbacks. This is a placeholder for future implementation.
        return "Validation error - check validation callbacks for details";
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChainstateManager));
    }

     public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.ChainstateManagerDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~ChainstateManager() => Dispose();

}

/// <summary>
/// Options for creating a chainstate manager.
/// </summary>
public sealed class ChainstateManagerOptions : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    private readonly string _dataDirectory;
    private readonly string _blocksDirectory;

    /// <summary>
    /// Creates chainstate manager options with data and blocks directories.
    /// </summary>
    public ChainstateManagerOptions(KernelContext context, string dataDirectory, string blocksDirectory)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (string.IsNullOrEmpty(dataDirectory)) throw new ArgumentNullException(nameof(dataDirectory));
        if (string.IsNullOrEmpty(blocksDirectory)) throw new ArgumentNullException(nameof(blocksDirectory));

        _dataDirectory = dataDirectory;
        _blocksDirectory = blocksDirectory;

        _handle = NativeMethods.ChainstateManagerOptionsCreate(
            context.Handle,
            dataDirectory,
            (nuint)System.Text.Encoding.UTF8.GetByteCount(dataDirectory),
            blocksDirectory,
            (nuint)System.Text.Encoding.UTF8.GetByteCount(blocksDirectory));

        if (_handle == IntPtr.Zero)
            throw new KernelException("Failed to create chainstate manager options");
    }

    public string DataDirectory => _dataDirectory;

    public string BlocksDirectory => _blocksDirectory;

    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    /// <summary>
    /// Sets the number of worker threads for script verification.
    /// </summary>
    public ChainstateManagerOptions SetWorkerThreads(int workerThreads)
    {
        ThrowIfDisposed();
        if (workerThreads < 0 || workerThreads > 15)
            throw new ArgumentOutOfRangeException(nameof(workerThreads), "Worker threads must be between 0 and 15");

        NativeMethods.ChainstateManagerOptionsSetWorkerThreads(_handle, workerThreads);
        return this;
    }

    /// <summary>
    /// Sets whether to wipe the databases on load.
    /// </summary>
    public ChainstateManagerOptions SetWipeDbs(bool wipeBlockTreeDb, bool wipeChainstate)
    {
        ThrowIfDisposed();
        int result = NativeMethods.ChainstateManagerOptionsSetWipeDbs(
            _handle,
            wipeBlockTreeDb ? 1 : 0,
            wipeChainstate ? 1 : 0);

        if (result != 0)
            throw new KernelException($"Failed to set wipe dbs (error code: {result})");

        return this;
    }

    /// <summary>
    /// Sets block tree database to use in-memory storage.
    /// </summary>
    public ChainstateManagerOptions SetBlockTreeDbInMemory(bool inMemory)
    {
        ThrowIfDisposed();
        NativeMethods.ChainstateManagerOptionsUpdateBlockTreeDbInMemory(_handle, inMemory ? 1 : 0);
        return this;
    }

    /// <summary>
    /// Sets chainstate database to use in-memory storage.
    /// </summary>
    public ChainstateManagerOptions SetChainstateDbInMemory(bool inMemory)
    {
        ThrowIfDisposed();
        NativeMethods.ChainstateManagerOptionsUpdateChainstateDbInMemory(_handle, inMemory ? 1 : 0);
        return this;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChainstateManagerOptions));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.ChainstateManagerOptionsDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~ChainstateManagerOptions() => Dispose();
}

