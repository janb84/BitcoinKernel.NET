using System;
using BitcoinKernel.Core.Abstractions;
using BitcoinKernel.Core.Chain;

using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;
using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Core.BlockProcessing;

/// <summary>
/// Handles block processing operations including validation and chain integration.
/// </summary>
public sealed class BlockProcessor
{
    private readonly ChainstateManager _chainstateManager;

    /// <summary>
    /// Creates a new block processor for the given chainstate manager.
    /// </summary>
    public BlockProcessor(ChainstateManager chainstateManager)
    {
        _chainstateManager = chainstateManager ?? throw new ArgumentNullException(nameof(chainstateManager));
    }

    /// <summary>
    /// Processes and validates a block, potentially adding it to the chain.
    /// </summary>
    /// <param name="block">The block to process.</param>
    /// <returns>Block processing result with validation status and whether it was a new block.</returns>
    /// <exception cref="ArgumentNullException">If block is null.</exception>
    /// <exception cref="BlockException">If block processing encounters an error.</exception>
    public BlockProcessingResult ProcessBlock(Block block)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));

        int newBlock = 0;
        int result = NativeMethods.ChainstateManagerProcessBlock(
            _chainstateManager.Handle,
            block.Handle,
            out newBlock);

        if (result != 0)
        {
            throw new BlockException($"Failed to process block (error code: {result})");
        }

        return new BlockProcessingResult(
            success: true,
            isNewBlock: newBlock != 0);
    }

    /// <summary>
    /// Processes a block from raw serialized data.
    /// </summary>
    /// <param name="rawBlockData">The serialized block data.</param>
    /// <returns>Block processing result.</returns>
    /// <exception cref="ArgumentNullException">If rawBlockData is null.</exception>
    /// <exception cref="BlockException">If block creation or processing fails.</exception>
    public BlockProcessingResult ProcessBlock(byte[] rawBlockData)
    {
        if (rawBlockData == null) throw new ArgumentNullException(nameof(rawBlockData));
        if (rawBlockData.Length == 0) throw new ArgumentException("Block data cannot be empty", nameof(rawBlockData));

        using var block = Block.FromBytes(rawBlockData);
        return ProcessBlock(block);
    }

    /// <summary>
    /// Validates a block without adding it to the chain.
    /// </summary>
    /// <param name="block">The block to validate.</param>
    /// <returns>Validation result with details about any failures.</returns>
    public BlockValidationResult ValidateBlock(Block block)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));

        try
        {
            // Processing with check will validate without permanently adding
            int newBlock = 0;
            int result = NativeMethods.ChainstateManagerProcessBlock(
                _chainstateManager.Handle,
                block.Handle,
                out newBlock);

            if (result == 0)
            {
                return new BlockValidationResult(
                    isValid: true,
                    mode: ValidationMode.VALID);
            }
            else
            {
                return new BlockValidationResult(
                    isValid: false,
                    mode: ValidationMode.INVALID,
                    errorCode: result);
            }
        }
        catch (Exception ex)
        {
            return new BlockValidationResult(
                isValid: false,
                mode: ValidationMode.INTERNAL_ERROR,
                errorMessage: ex.Message);
        }
    }

    /// <summary>
    /// Reads a block from disk by its block tree entry.
    /// </summary>
    /// <param name="blockTreeEntry">The block tree entry pointing to the block.</param>
    /// <returns>The block read from disk.</returns>
    /// <exception cref="ArgumentNullException">If blockTreeEntry is null.</exception>
    /// <exception cref="BlockException">If reading the block fails.</exception>
    public Block ReadBlock(BlockTreeEntry blockTreeEntry)
    {
        if (blockTreeEntry == null) throw new ArgumentNullException(nameof(blockTreeEntry));

        var blockPtr = NativeMethods.BlockRead(_chainstateManager.Handle, blockTreeEntry.Handle);
        if (blockPtr == IntPtr.Zero)
        {
            throw new BlockException("Failed to read block from disk");
        }

        return new Block(blockPtr);
    }

    /// <summary>
    /// Retrieves a block tree entry by its hash.
    /// </summary>
    /// <param name="blockHash">The hash of the block to find.</param>
    /// <returns>The block tree entry, or null if not found.</returns>
    public BlockTreeEntry? GetBlockTreeEntry(byte[] blockHash)
    {
        if (blockHash == null) throw new ArgumentNullException(nameof(blockHash));
        if (blockHash.Length != 32) throw new ArgumentException("Block hash must be 32 bytes", nameof(blockHash));

        using var hash = BlockHash.FromBytes(blockHash);
        var entryPtr = NativeMethods.ChainstateManagerGetBlockTreeEntryByHash(
            _chainstateManager.Handle,
            hash.Handle);

        return entryPtr != IntPtr.Zero ? new BlockTreeEntry(entryPtr) : null;
    }
}

/// <summary>
/// Result of a block processing operation.
/// </summary>
public sealed class BlockProcessingResult
{
    /// <summary>
    /// Whether the block processing was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Whether this was a new block (not previously seen).
    /// </summary>
    public bool IsNewBlock { get; }

    /// <summary>
    /// Creates a new block processing result.
    /// </summary>
    public BlockProcessingResult(bool success, bool isNewBlock)
    {
        Success = success;
        IsNewBlock = isNewBlock;
    }
}






