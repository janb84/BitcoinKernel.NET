using System;
using System.Runtime.InteropServices;
using BitcoinKernel.Interop.Structs;
using BitcoinKernel.Interop.Helpers;
using BitcoinKernel.Interop.Enums;
using BitcoinKernel.Interop.Delegates;

namespace BitcoinKernel.Interop
{
    /// <summary>
    /// Low-level P/Invoke declarations for the Bitcoin Kernel C API.
    /// Maps directly to the bitcoinkernel.h C header.
    /// </summary>
    public static class NativeMethods
    {
        #region Library Configuration

        private const string LibName = "bitcoinkernel";

        // For loading platform-specific libraries
        static NativeMethods()
        {
            NativeLibraryLoader.EnsureLoaded();
        }

        #endregion

        #region Context Management

        /// <summary>
        /// Creates a new kernel context.
        /// </summary>
        /// <returns>Pointer to kernel context, or IntPtr.Zero on failure</returns>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_create")]
        public static extern IntPtr ContextCreate(IntPtr options);

        /// <summary>
        /// Destroys a kernel context and frees associated resources.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_destroy")]
        public static extern void ContextDestroy(IntPtr context);

        /// <summary>
        /// Interrupts long-running operations associated with this context.
        /// Returns 0 on success.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_interrupt")]
        public static extern int ContextInterrupt(IntPtr context);

        #endregion

        #region Context Options

        /// <summary>
        /// Creates context options with default values.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_options_create")]
        public static extern IntPtr ContextOptionsCreate();

        /// <summary>
        /// Destroys context options.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_options_destroy")]
        public static extern void ContextOptionsDestroy(IntPtr options);

        /// <summary>
        /// Sets the chain parameters for the context.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_options_set_chainparams")]
        public static extern void ContextOptionsSetChainParams(IntPtr options, IntPtr chain_params);

        /// <summary>
        /// Sets the notification callbacks for the context.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_options_set_notifications")]
        public static extern void ContextOptionsSetNotifications(IntPtr options, NotificationInterfaceCallbacks callbacks);

        /// <summary>
        /// Sets the validation interface for the context.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_options_set_validation_interface")]
        public static extern void ContextOptionsSetValidationInterface(IntPtr options, ValidationInterfaceCallbacks callbacks);

        #endregion

        #region Chain Parameters

        /// <summary>
        /// Creates chain parameters for the specified chain type.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chain_parameters_create")]
        public static extern IntPtr ChainParametersCreate(ChainType chain_type);

        /// <summary>
        /// Destroys chain parameters.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chain_parameters_destroy")]
        public static extern void ChainParametersDestroy(IntPtr chain_params);

        #endregion

        #region Chainstate Manager

        /// <summary>
        /// Creates a chainstate manager.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_create")]
        public static extern IntPtr ChainstateManagerCreate(IntPtr options);

        /// <summary>
        /// Destroys a chainstate manager.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_destroy")]
        public static extern void ChainstateManagerDestroy(IntPtr manager);

        /// <summary>
        /// Processes a block through validation.
        /// Returns 0 on success.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_process_block")]
        public static extern int ChainstateManagerProcessBlock(
            IntPtr manager,
            IntPtr block,
            ref int new_block);

        /// <summary>
        /// Gets a block tree entry by its block hash.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_get_block_tree_entry_by_hash")]
        public static extern IntPtr ChainstateManagerGetBlockTreeEntryByHash(
            IntPtr manager,
            IntPtr block_hash);

        /// <summary>
        /// Gets the active chain from the chainstate manager.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_get_active_chain")]
        public static extern IntPtr ChainstateManagerGetActiveChain(IntPtr manager);

        /// <summary>
        /// Imports blocks from an array of file paths.
        /// Returns 0 on success.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_import_blocks")]
        public static extern int ChainstateManagerImportBlocks(
            IntPtr manager,
            [MarshalAs(UnmanagedType.LPArray)] string[] block_file_paths_data,
            nuint[] block_file_paths_lens,
            nuint block_file_paths_data_len);

        #endregion

        #region Chainstate Manager Options

        /// <summary>
        /// Creates chainstate manager options.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_options_create")]
        public static extern IntPtr ChainstateManagerOptionsCreate(
            IntPtr context,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string data_directory,
            nuint data_directory_len,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string blocks_directory,
            nuint blocks_directory_len);

        /// <summary>
        /// Destroys chainstate manager options.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_options_destroy")]
        public static extern void ChainstateManagerOptionsDestroy(IntPtr options);

        /// <summary>
        /// Sets the number of worker threads for script verification.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_options_set_worker_threads_num")]
        public static extern void ChainstateManagerOptionsSetWorkerThreads(
            IntPtr options,
            int worker_threads);

        /// <summary>
        /// Sets whether to wipe the databases on load.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_options_set_wipe_dbs")]
        public static extern int ChainstateManagerOptionsSetWipeDbs(
            IntPtr options,
            int wipe_block_tree_db,
            int wipe_chainstate_db);

        /// <summary>
        /// Sets block tree db in memory.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_options_update_block_tree_db_in_memory")]
        public static extern void ChainstateManagerOptionsUpdateBlockTreeDbInMemory(
            IntPtr options,
            int block_tree_db_in_memory);

        /// <summary>
        /// Sets chainstate db in memory.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chainstate_manager_options_update_chainstate_db_in_memory")]
        public static extern void ChainstateManagerOptionsUpdateChainstateDbInMemory(
            IntPtr options,
            int chainstate_db_in_memory);

        #endregion

        #region Block Operations

        /// <summary>
        /// Creates a block from raw data.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_create")]
        public static extern IntPtr BlockCreate(
            byte[] raw_block,
            UIntPtr raw_block_len);

        /// <summary>
        /// Destroys a block.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_destroy")]
        public static extern void BlockDestroy(IntPtr block);

        /// <summary>
        /// Reads a block from disk by its block tree entry.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_read")]
        public static extern IntPtr BlockRead(
            IntPtr chainstate_manager,
            IntPtr block_tree_entry);

        /// <summary>
        /// Gets the number of transactions in a block.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_count_transactions")]
        public static extern nuint BlockCountTransactions(IntPtr block);

        /// <summary>
        /// Gets the block hash.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_get_hash")]
        public static extern IntPtr BlockGetHash(IntPtr block);

        /// <summary>
        /// Serializes the block to bytes.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_to_bytes")]
        public static extern int BlockToBytes(
            IntPtr block,
            WriteBytes writer,
            IntPtr user_data);

        /// <summary>
        /// Delegate for writing bytes during serialization.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int WriteBytes(IntPtr bytes, nuint size, IntPtr userdata);

        /// <summary>
        /// Gets the block hash from a block tree entry.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_tree_entry_get_block_hash")]
        public static extern IntPtr BlockTreeEntryGetBlockHash(IntPtr block_tree_entry);

        /// <summary>
        /// Gets the block height from a block tree entry.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_tree_entry_get_height")]
        public static extern int BlockTreeEntryGetHeight(IntPtr block_tree_entry);

        /// <summary>
        /// Gets the previous block tree entry from a block tree entry.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_tree_entry_get_previous")]
        public static extern IntPtr BlockTreeEntryGetPrevious(IntPtr block_tree_entry);



        #endregion

        #region BlockHash Operations

        /// <summary>
        /// Creates a block hash from 32 bytes.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_hash_create")]
        public static extern IntPtr BlockHashCreate(IntPtr hash);

        /// <summary>
        /// Creates a block hash from 32 bytes (unsafe version).
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_hash_create")]
        public static extern unsafe IntPtr BlockHashCreate(byte* hash);

        /// <summary>
        /// Serializes the block hash to bytes (32 bytes).
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_hash_to_bytes")]
        public static extern void BlockHashToBytes(IntPtr block_hash, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] byte[] output);

        /// <summary>
        /// Destroys a block hash.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_hash_destroy")]
        public static extern void BlockHashDestroy(IntPtr block_hash);

        #endregion



        #region Chain Operations

        /// <summary>
        /// Gets the height of the chain.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chain_get_height")]
        public static extern int ChainGetHeight(IntPtr chain);

        #endregion

        #region Transaction Operations

        /// <summary>
        /// Creates a transaction from serialized data.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_create")]
        public static extern IntPtr TransactionCreate(IntPtr raw_transaction, nuint raw_transaction_len);

        /// <summary>
        /// Copies a transaction (reference counted).
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_copy")]
        public static extern IntPtr TransactionCopy(IntPtr transaction);

        /// <summary>
        /// Serializes a transaction to bytes.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_to_bytes")]
        public static extern int TransactionToBytes(
            IntPtr transaction,
            WriteBytes writer,
            IntPtr user_data);

        /// <summary>
        /// Gets the transaction ID (txid).
        /// Returns a pointer to btck_Txid (not owned, lifetime depends on transaction).
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_get_txid")]
        public static extern IntPtr TransactionGetTxid(IntPtr transaction);

        /// <summary>
        /// Gets the number of outputs in a transaction.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_count_outputs")]
        public static extern nuint TransactionCountOutputs(IntPtr transaction);

        /// <summary>
        /// Gets the number of inputs in a transaction.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_count_inputs")]
        public static extern nuint TransactionCountInputs(IntPtr transaction);

        /// <summary>
        /// Gets a transaction output at the specified index.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_get_output_at")]
        public static extern IntPtr TransactionGetOutputAt(IntPtr transaction, nuint index);

        /// <summary>
        /// Gets a transaction input at the specified index.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_get_input_at")]
        public static extern IntPtr TransactionGetInputAt(IntPtr transaction, nuint index);

        /// <summary>
        /// Destroys a transaction.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_destroy")]
        public static extern void TransactionDestroy(IntPtr transaction);

        /// <summary>
        /// Run consensus/tx_check::CheckTransaction on a transaction and return the filled state.
        /// Returns 1 if valid, 0 if invalid.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_check_transaction")]
        public static extern int CheckTransaction(IntPtr tx, out IntPtr out_state);

        /// <summary>
        /// Gets the script pubkey from a transaction output.
        /// Returns a pointer to btck_ScriptPubkey.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_output_get_script_pubkey")]
        public static extern IntPtr TransactionOutputGetScriptPubkey(IntPtr output);

        /// <summary>
        /// Gets the value (amount) from a transaction output.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_output_get_amount")]
        public static extern long TransactionOutputGetAmount(IntPtr output);

        /// <summary>
        /// Copy a transaction output.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_output_copy")]
        public static extern IntPtr TransactionOutputCopy(IntPtr output);

        /// <summary>
        /// Destroys a transaction output.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_output_destroy")]
        public static extern void TransactionOutputDestroy(IntPtr output);

        #endregion

        #region TxValidationState Operations

        /// <summary>
        /// Gets the validation mode from a transaction validation state.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_tx_validation_state_get_validation_mode")]
        public static extern ValidationMode TxValidationStateGetValidationMode(IntPtr state);

        /// <summary>
        /// Gets the transaction validation result from a transaction validation state.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_tx_validation_state_get_tx_validation_result")]
        public static extern TxValidationResult TxValidationStateGetTxValidationResult(IntPtr state);

        /// <summary>
        /// Destroys a transaction validation state.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_tx_validation_state_destroy")]
        public static extern void TxValidationStateDestroy(IntPtr state);

        #endregion

        #region ScriptPubkey Operations

        /// <summary>
        /// Creates a script pubkey from serialized data.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_script_pubkey_create")]
        public static extern IntPtr ScriptPubkeyCreate(IntPtr script_pubkey_data, nuint script_pubkey_len);

        /// <summary>
        /// Copies a script pubkey.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_script_pubkey_copy")]
        public static extern IntPtr ScriptPubkeyCopy(IntPtr script_pubkey);

        /// <summary>
        /// Verifies a script pubkey.
        /// Returns 1 if valid, 0 otherwise.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_script_pubkey_verify")]
        public static extern int ScriptPubkeyVerify(
            IntPtr script_pubkey,
            long amount,
            IntPtr tx_to,
            IntPtr[] spent_outputs,
            nuint spent_outputs_len,
            uint input_index,
            uint flags,
            IntPtr status);

        /// <summary>
        /// Serializes a script pubkey to bytes.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_script_pubkey_to_bytes")]
        public static extern int ScriptPubkeyToBytes(
            IntPtr script_pubkey,
            WriteBytes writer,
            IntPtr user_data);

        /// <summary>
        /// Destroys a script pubkey.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_script_pubkey_destroy")]
        public static extern void ScriptPubkeyDestroy(IntPtr script_pubkey);

        #endregion

        #region TransactionOutput Operations (Additional)

        /// <summary>
        /// Creates a transaction output from a script pubkey and amount.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_output_create")]
        public static extern IntPtr TransactionOutputCreate(IntPtr script_pubkey, long amount);

        #endregion

        #region Logging

        /// <summary>
        /// Creates a logging connection with a callback.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_logging_connection_create")]
        public static extern IntPtr LoggingConnectionCreate(
            LoggingCallback callback,
            IntPtr user_data,
            DestroyCallback? user_data_destroy_callback);

        /// <summary>
        /// Destroys a logging connection.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_logging_connection_destroy")]
        public static extern void LoggingConnectionDestroy(IntPtr connection);

        /// <summary>
        /// Disables logging permanently.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_logging_disable")]
        public static extern void LoggingDisable();

        /// <summary>
        /// Sets the log level for a category.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_logging_set_level_category")]
        public static extern void LoggingSetLevelCategory(LogCategory category, LogLevel level);

        /// <summary>
        /// Sets logging options.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_logging_set_options")]
        public static extern void LoggingSetOptions(LoggingOptions options);

        /// <summary>
        /// Enables a log category.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_logging_enable_category")]
        public static extern void LoggingEnableCategory(LogCategory category);

        /// <summary>
        /// Disables a log category.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_logging_disable_category")]
        public static extern void LoggingDisableCategory(LogCategory category);

        #endregion

        #region Block Validation State

        /// <summary>
        /// Gets the validation mode from a block validation state.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_validation_state_get_validation_mode")]
        public static extern ValidationMode BlockValidationStateGetValidationMode(IntPtr validation_state);

        /// <summary>
        /// Gets the block validation result from a block validation state.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_validation_state_get_block_validation_result")]
        public static extern BlockValidationResult BlockValidationStateGetBlockValidationResult(IntPtr validation_state);

        #endregion

        #region Txid Operations

        /// <summary>
        /// Copies a txid.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_txid_copy")]
        public static extern IntPtr TxidCopy(IntPtr txid);

        /// <summary>
        /// Checks if two txids are equal.
        /// Returns 1 if equal, 0 otherwise.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_txid_equals")]
        public static extern int TxidEquals(IntPtr txid1, IntPtr txid2);

        /// <summary>
        /// Serializes a txid to bytes (32 bytes).
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_txid_to_bytes")]
        public static extern void TxidToBytes(IntPtr txid, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] byte[] output);

        /// <summary>
        /// Destroys a txid.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_txid_destroy")]
        public static extern void TxidDestroy(IntPtr txid);

        #endregion

        #region Coin Operations

        /// <summary>
        /// Copies a coin.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_coin_copy")]
        public static extern IntPtr CoinCopy(IntPtr coin);

        /// <summary>
        /// Returns the block height where the transaction that created this coin was included.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_coin_confirmation_height")]
        public static extern uint CoinConfirmationHeight(IntPtr coin);

        /// <summary>
        /// Returns whether the containing transaction was a coinbase.
        /// Returns 1 if coinbase, 0 otherwise.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_coin_is_coinbase")]
        public static extern int CoinIsCoinbase(IntPtr coin);

        /// <summary>
        /// Gets the transaction output of a coin.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_coin_get_output")]
        public static extern IntPtr CoinGetOutput(IntPtr coin);

        /// <summary>
        /// Destroys a coin.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_coin_destroy")]
        public static extern void CoinDestroy(IntPtr coin);

        #endregion

        #region Block Operations (Additional)

        /// <summary>
        /// Copies a block (reference counted).
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_copy")]
        public static extern IntPtr BlockCopy(IntPtr block);

        /// <summary>
        /// Gets a transaction at the specified index in a block.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_get_transaction_at")]
        public static extern IntPtr BlockGetTransactionAt(IntPtr block, nuint index);

        #endregion

        #region Chain Operations (Additional)

        /// <summary>
        /// Gets a block tree entry by height.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chain_get_by_height")]
        public static extern IntPtr ChainGetByHeight(IntPtr chain, int height);

        /// <summary>
        /// Checks if a block tree entry is in the chain.
        /// Returns 1 if in chain, 0 otherwise.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chain_contains")]
        public static extern int ChainContains(IntPtr chain, IntPtr block_tree_entry);

        #endregion

        #region BlockSpentOutputs Operations

        /// <summary>
        /// Reads block spent outputs (undo data) from disk.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_spent_outputs_read")]
        public static extern IntPtr BlockSpentOutputsRead(
            IntPtr chainstate_manager,
            IntPtr block_tree_entry);

        /// <summary>
        /// Copies block spent outputs.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_spent_outputs_copy")]
        public static extern IntPtr BlockSpentOutputsCopy(IntPtr block_spent_outputs);

        /// <summary>
        /// Gets the count of transaction spent outputs in block spent outputs.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_spent_outputs_count")]
        public static extern nuint BlockSpentOutputsCount(IntPtr block_spent_outputs);

        /// <summary>
        /// Gets transaction spent outputs at the specified index.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_spent_outputs_get_transaction_spent_outputs_at")]
        public static extern IntPtr BlockSpentOutputsGetTransactionSpentOutputsAt(
            IntPtr block_spent_outputs,
            nuint index);

        /// <summary>
        /// Destroys block spent outputs.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_spent_outputs_destroy")]
        public static extern void BlockSpentOutputsDestroy(IntPtr block_spent_outputs);

        #endregion

        #region TransactionSpentOutputs Operations

        /// <summary>
        /// Copies transaction spent outputs.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_spent_outputs_copy")]
        public static extern IntPtr TransactionSpentOutputsCopy(IntPtr transaction_spent_outputs);

        /// <summary>
        /// Gets the count of coins in transaction spent outputs.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_spent_outputs_count")]
        public static extern nuint TransactionSpentOutputsCount(IntPtr transaction_spent_outputs);

        /// <summary>
        /// Gets a coin at the specified index.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_spent_outputs_get_coin_at")]
        public static extern IntPtr TransactionSpentOutputsGetCoinAt(
            IntPtr transaction_spent_outputs,
            nuint index);

        /// <summary>
        /// Destroys transaction spent outputs.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_spent_outputs_destroy")]
        public static extern void TransactionSpentOutputsDestroy(IntPtr transaction_spent_outputs);

        #endregion

        #region Missing Functions

        /// <summary>
        /// Copy a context.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_context_copy")]
        public static extern IntPtr ContextCopy(IntPtr context);

        /// <summary>
        /// Copy chain parameters.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_chain_parameters_copy")]
        public static extern IntPtr ChainParametersCopy(IntPtr chain_params);

        /// <summary>
        /// Checks if two block hashes are equal.
        /// Returns 1 if equal, 0 otherwise.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_hash_equals")]
        public static extern int BlockHashEquals(IntPtr hash1, IntPtr hash2);

        /// <summary>
        /// Copy a block hash.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_block_hash_copy")]
        public static extern IntPtr BlockHashCopy(IntPtr block_hash);

        #endregion

        #region TransactionInput Operations

        /// <summary>
        /// Copies a transaction input.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_input_copy")]
        public static extern IntPtr TransactionInputCopy(IntPtr transaction_input);

        /// <summary>
        /// Gets the transaction out point from a transaction input.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_input_get_out_point")]
        public static extern IntPtr TransactionInputGetOutPoint(IntPtr transaction_input);

        /// <summary>
        /// Destroys a transaction input.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_input_destroy")]
        public static extern void TransactionInputDestroy(IntPtr transaction_input);

        #endregion

        #region TransactionOutPoint Operations

        /// <summary>
        /// Copies a transaction out point.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_out_point_copy")]
        public static extern IntPtr TransactionOutPointCopy(IntPtr transaction_out_point);

        /// <summary>
        /// Gets the output index from a transaction out point.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_out_point_get_index")]
        public static extern uint TransactionOutPointGetIndex(IntPtr transaction_out_point);

        /// <summary>
        /// Gets the txid from a transaction out point.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_out_point_get_txid")]
        public static extern IntPtr TransactionOutPointGetTxid(IntPtr transaction_out_point);

        /// <summary>
        /// Destroys a transaction out point.
        /// </summary>
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btck_transaction_out_point_destroy")]
        public static extern void TransactionOutPointDestroy(IntPtr transaction_out_point);

        #endregion

    }
}