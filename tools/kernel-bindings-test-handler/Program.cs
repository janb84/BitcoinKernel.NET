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

                "btck_block_tree_entry_get_block_hash" =>
                    dispatcher.BlockTreeEntryGetBlockHash(id,
                        Deserialize<BtckBlockTreeEntryGetBlockHashParams>(request.Params, opts)),

                // ── Script Pubkey ─────────────────────────────────────────────
                "btck_script_pubkey_create" =>
                    dispatcher.ScriptPubkeyCreate(id, request.Ref,
                        Deserialize<BtckScriptPubkeyCreateParams>(request.Params, opts)),

                "btck_script_pubkey_destroy" =>
                    dispatcher.ScriptPubkeyDestroy(id,
                        Deserialize<BtckScriptPubkeyDestroyParams>(request.Params, opts)),

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

                // ── Transaction Output ────────────────────────────────────────
                "btck_transaction_output_create" =>
                    dispatcher.TransactionOutputCreate(id, request.Ref,
                        Deserialize<BtckTransactionOutputCreateParams>(request.Params, opts)),

                "btck_transaction_output_destroy" =>
                    dispatcher.TransactionOutputDestroy(id,
                        Deserialize<BtckTransactionOutputDestroyParams>(request.Params, opts)),

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
