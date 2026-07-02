using System.Text.Json;
using BitcoinKernel.TestHandler.Handlers;
using BitcoinKernel.TestHandler.Protocol;

namespace BitcoinKernel.TestHandler;

/// <summary>
/// Test handler for Bitcoin Kernel conformance tests.
/// Implements the JSON-based protocol for testing bindings.
/// Reads JSON requests line-by-line from stdin, writes JSON responses to stdout.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        using var dispatcher = new MethodDispatcher();

        try
        {
            string? line;
            while ((line = await Console.In.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                Response response;
                try
                {
                    var request = JsonSerializer.Deserialize<Request>(line, jsonOptions);
                    if (request == null)
                    {
                        response = new Response { Id = "unknown", Result = null, Error = new ErrorResponse() };
                    }
                    else
                    {
                        response = Dispatch(request, dispatcher, jsonOptions);
                    }
                }
                catch (JsonException)
                {
                    response = new Response { Id = "unknown", Result = null, Error = new ErrorResponse() };
                }

                var responseJson = JsonSerializer.Serialize(response, jsonOptions);
                await Console.Out.WriteLineAsync(responseJson);
                await Console.Out.FlushAsync();
            }

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    private static Response Dispatch(Request request, MethodDispatcher dispatcher, JsonSerializerOptions opts)
    {
        var id = request.Id;

        try
        {
            return request.Method switch
            {
                // ── Context ──────────────────────────────────────────────────
                "btck_context_create" =>
                    dispatcher.ContextCreate(id, request.Ref,
                        Deserialize<BtckContextCreateParams>(request.Params, opts)),

                "btck_context_destroy" =>
                    dispatcher.ContextDestroy(id,
                        Deserialize<BtckContextDestroyParams>(request.Params, opts)),

                // ── Chainstate Manager ────────────────────────────────────────
                "btck_chainstate_manager_create" =>
                    dispatcher.ChainstateManagerCreate(id, request.Ref,
                        Deserialize<BtckChainstateManagerCreateParams>(request.Params, opts)),

                "btck_chainstate_manager_get_active_chain" =>
                    dispatcher.ChainstateManagerGetActiveChain(id, request.Ref,
                        Deserialize<BtckChainstateManagerGetActiveChainParams>(request.Params, opts)),

                "btck_chainstate_manager_process_block" =>
                    dispatcher.ChainstateManagerProcessBlock(id,
                        Deserialize<BtckChainstateManagerProcessBlockParams>(request.Params, opts)),

                "btck_chainstate_manager_destroy" =>
                    dispatcher.ChainstateManagerDestroy(id,
                        Deserialize<BtckChainstateManagerDestroyParams>(request.Params, opts)),

                // ── Chain ─────────────────────────────────────────────────────
                "btck_chain_get_height" =>
                    dispatcher.ChainGetHeight(id,
                        Deserialize<BtckChainGetHeightParams>(request.Params, opts)),

                "btck_chain_get_by_height" =>
                    dispatcher.ChainGetByHeight(id, request.Ref,
                        Deserialize<BtckChainGetByHeightParams>(request.Params, opts)),

                "btck_chain_contains" =>
                    dispatcher.ChainContains(id,
                        Deserialize<BtckChainContainsParams>(request.Params, opts)),

                // ── Block ─────────────────────────────────────────────────────
                "btck_block_create" =>
                    dispatcher.BlockCreate(id, request.Ref,
                        Deserialize<BtckBlockCreateParams>(request.Params, opts)),

                "btck_block_get_hash" =>
                    dispatcher.BlockGetHash(id, request.Ref,
                        Deserialize<BtckBlockRefParams>(request.Params, opts)),

                "btck_block_get_header" =>
                    dispatcher.BlockGetHeader(id, request.Ref,
                        Deserialize<BtckBlockRefParams>(request.Params, opts)),

                "btck_block_copy" =>
                    dispatcher.BlockCopy(id, request.Ref,
                        Deserialize<BtckBlockRefParams>(request.Params, opts)),

                "btck_block_count_transactions" =>
                    dispatcher.BlockCountTransactions(id,
                        Deserialize<BtckBlockRefParams>(request.Params, opts)),

                "btck_block_get_transaction_at" =>
                    dispatcher.BlockGetTransactionAt(id, request.Ref,
                        Deserialize<BtckBlockGetTransactionAtParams>(request.Params, opts)),

                "btck_block_to_bytes" =>
                    dispatcher.BlockToBytes(id,
                        Deserialize<BtckBlockRefParams>(request.Params, opts)),

                "btck_block_destroy" =>
                    dispatcher.BlockDestroy(id,
                        Deserialize<BtckBlockRefParams>(request.Params, opts)),

                "btck_block_tree_entry_get_block_hash" =>
                    dispatcher.BlockTreeEntryGetBlockHash(id, request.Ref,
                        Deserialize<BtckBlockTreeEntryGetBlockHashParams>(request.Params, opts)),

                // ── Block Hash ────────────────────────────────────────────────
                "btck_block_hash_create" =>
                    dispatcher.BlockHashCreate(id, request.Ref,
                        Deserialize<BtckBlockHashCreateParams>(request.Params, opts)),

                "btck_block_hash_to_bytes" =>
                    dispatcher.BlockHashToBytes(id,
                        Deserialize<BtckBlockHashRefParams>(request.Params, opts)),

                "btck_block_hash_equals" =>
                    dispatcher.BlockHashEquals(id,
                        Deserialize<BtckBlockHashEqualsParams>(request.Params, opts)),

                "btck_block_hash_copy" =>
                    dispatcher.BlockHashCopy(id, request.Ref,
                        Deserialize<BtckBlockHashRefParams>(request.Params, opts)),

                "btck_block_hash_destroy" =>
                    dispatcher.BlockHashDestroy(id,
                        Deserialize<BtckBlockHashRefParams>(request.Params, opts)),

                // ── Block Header ──────────────────────────────────────────────
                "btck_block_header_create" =>
                    dispatcher.BlockHeaderCreate(id, request.Ref,
                        Deserialize<BtckBlockHeaderCreateParams>(request.Params, opts)),

                "btck_block_header_to_bytes" =>
                    dispatcher.BlockHeaderToBytes(id,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                "btck_block_header_get_hash" =>
                    dispatcher.BlockHeaderGetHash(id, request.Ref,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                "btck_block_header_get_prev_hash" =>
                    dispatcher.BlockHeaderGetPrevHash(id, request.Ref,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                "btck_block_header_get_version" =>
                    dispatcher.BlockHeaderGetVersion(id,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                "btck_block_header_get_timestamp" =>
                    dispatcher.BlockHeaderGetTimestamp(id,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                "btck_block_header_get_bits" =>
                    dispatcher.BlockHeaderGetBits(id,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                "btck_block_header_get_nonce" =>
                    dispatcher.BlockHeaderGetNonce(id,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                "btck_block_header_copy" =>
                    dispatcher.BlockHeaderCopy(id, request.Ref,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                "btck_block_header_destroy" =>
                    dispatcher.BlockHeaderDestroy(id,
                        Deserialize<BtckBlockHeaderRefParams>(request.Params, opts)),

                // ── Script Pubkey ─────────────────────────────────────────────
                "btck_script_pubkey_create" =>
                    dispatcher.ScriptPubkeyCreate(id, request.Ref,
                        Deserialize<BtckScriptPubkeyCreateParams>(request.Params, opts)),

                "btck_script_pubkey_destroy" =>
                    dispatcher.ScriptPubkeyDestroy(id,
                        Deserialize<BtckScriptPubkeyDestroyParams>(request.Params, opts)),

                "btck_script_pubkey_copy" =>
                    dispatcher.ScriptPubkeyCopy(id, request.Ref,
                        Deserialize<BtckScriptPubkeyRefParams>(request.Params, opts)),

                "btck_script_pubkey_to_bytes" =>
                    dispatcher.ScriptPubkeyToBytes(id,
                        Deserialize<BtckScriptPubkeyRefParams>(request.Params, opts)),

                "btck_script_pubkey_verify" =>
                    dispatcher.ScriptPubkeyVerify(id,
                        Deserialize<BtckScriptPubkeyVerifyParams>(request.Params, opts)),

                // ── Transaction ───────────────────────────────────────────────
                "btck_transaction_create" =>
                    dispatcher.TransactionCreate(id, request.Ref,
                        Deserialize<BtckTransactionCreateParams>(request.Params, opts)),

                "btck_transaction_destroy" =>
                    dispatcher.TransactionDestroy(id,
                        Deserialize<BtckTransactionDestroyParams>(request.Params, opts)),

                "btck_transaction_copy" =>
                    dispatcher.TransactionCopy(id, request.Ref,
                        Deserialize<BtckTransactionRefParams>(request.Params, opts)),

                "btck_transaction_count_inputs" =>
                    dispatcher.TransactionCountInputs(id,
                        Deserialize<BtckTransactionRefParams>(request.Params, opts)),

                "btck_transaction_count_outputs" =>
                    dispatcher.TransactionCountOutputs(id,
                        Deserialize<BtckTransactionRefParams>(request.Params, opts)),

                "btck_transaction_get_txid" =>
                    dispatcher.TransactionGetTxid(id, request.Ref,
                        Deserialize<BtckTransactionRefParams>(request.Params, opts)),

                "btck_transaction_to_bytes" =>
                    dispatcher.TransactionToBytes(id,
                        Deserialize<BtckTransactionRefParams>(request.Params, opts)),

                "btck_transaction_get_input_at" =>
                    dispatcher.TransactionGetInputAt(id, request.Ref,
                        Deserialize<BtckTransactionGetInputAtParams>(request.Params, opts)),

                "btck_transaction_get_output_at" =>
                    dispatcher.TransactionGetOutputAt(id, request.Ref,
                        Deserialize<BtckTransactionGetOutputAtParams>(request.Params, opts)),

                // ── Transaction Input ─────────────────────────────────────────
                "btck_transaction_input_get_out_point" =>
                    dispatcher.TransactionInputGetOutPoint(id, request.Ref,
                        Deserialize<BtckTransactionInputRefParams>(request.Params, opts)),

                "btck_transaction_input_copy" =>
                    dispatcher.TransactionInputCopy(id, request.Ref,
                        Deserialize<BtckTransactionInputRefParams>(request.Params, opts)),

                "btck_transaction_input_destroy" =>
                    dispatcher.TransactionInputDestroy(id,
                        Deserialize<BtckTransactionInputRefParams>(request.Params, opts)),

                // ── Transaction Out Point ─────────────────────────────────────
                "btck_transaction_out_point_get_index" =>
                    dispatcher.TransactionOutPointGetIndex(id,
                        Deserialize<BtckTransactionOutPointRefParams>(request.Params, opts)),

                "btck_transaction_out_point_get_txid" =>
                    dispatcher.TransactionOutPointGetTxid(id, request.Ref,
                        Deserialize<BtckTransactionOutPointRefParams>(request.Params, opts)),

                "btck_transaction_out_point_copy" =>
                    dispatcher.TransactionOutPointCopy(id, request.Ref,
                        Deserialize<BtckTransactionOutPointRefParams>(request.Params, opts)),

                "btck_transaction_out_point_destroy" =>
                    dispatcher.TransactionOutPointDestroy(id,
                        Deserialize<BtckTransactionOutPointRefParams>(request.Params, opts)),

                // ── Txid ──────────────────────────────────────────────────────
                "btck_txid_to_bytes" =>
                    dispatcher.TxidToBytes(id,
                        Deserialize<BtckTxidRefParams>(request.Params, opts)),

                "btck_txid_equals" =>
                    dispatcher.TxidEquals(id,
                        Deserialize<BtckTxidEqualsParams>(request.Params, opts)),

                "btck_txid_copy" =>
                    dispatcher.TxidCopy(id, request.Ref,
                        Deserialize<BtckTxidRefParams>(request.Params, opts)),

                "btck_txid_destroy" =>
                    dispatcher.TxidDestroy(id,
                        Deserialize<BtckTxidRefParams>(request.Params, opts)),

                // ── Transaction Output ────────────────────────────────────────
                "btck_transaction_output_create" =>
                    dispatcher.TransactionOutputCreate(id, request.Ref,
                        Deserialize<BtckTransactionOutputCreateParams>(request.Params, opts)),

                "btck_transaction_output_destroy" =>
                    dispatcher.TransactionOutputDestroy(id,
                        Deserialize<BtckTransactionOutputDestroyParams>(request.Params, opts)),

                "btck_transaction_output_copy" =>
                    dispatcher.TransactionOutputCopy(id, request.Ref,
                        Deserialize<BtckTransactionOutputRefParams>(request.Params, opts)),

                "btck_transaction_output_get_amount" =>
                    dispatcher.TransactionOutputGetAmount(id,
                        Deserialize<BtckTransactionOutputRefParams>(request.Params, opts)),

                "btck_transaction_output_get_script_pubkey" =>
                    dispatcher.TransactionOutputGetScriptPubkey(id, request.Ref,
                        Deserialize<BtckTransactionOutputRefParams>(request.Params, opts)),

                // ── Precomputed Transaction Data ──────────────────────────────
                "btck_precomputed_transaction_data_create" =>
                    dispatcher.PrecomputedTransactionDataCreate(id, request.Ref,
                        Deserialize<BtckPrecomputedTransactionDataCreateParams>(request.Params, opts)),

                "btck_precomputed_transaction_data_destroy" =>
                    dispatcher.PrecomputedTransactionDataDestroy(id,
                        Deserialize<BtckPrecomputedTransactionDataDestroyParams>(request.Params, opts)),

                // ── Unknown ───────────────────────────────────────────────────
                _ => new Response { Id = id, Result = null, Error = new ErrorResponse() }
            };
        }
        catch (Exception)
        {
            return new Response { Id = id, Result = null, Error = new ErrorResponse() };
        }
    }

    private static T Deserialize<T>(JsonElement? element, JsonSerializerOptions opts) where T : new()
    {
        if (element == null) return new T();
        return JsonSerializer.Deserialize<T>(element.Value, opts) ?? new T();
    }
}
