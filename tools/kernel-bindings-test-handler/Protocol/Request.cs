using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitcoinKernel.TestHandler.Protocol;

/// <summary>
/// Represents a request from the test runner.
/// </summary>
public class Request
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }

    /// <summary>
    /// Reference name for storing the returned object in the registry.
    /// </summary>
    [JsonPropertyName("ref")]
    public string? Ref { get; set; }
}

/// <summary>
/// Represents a reference to an object stored in the registry. Used both as a
/// parameter value and as a result value.
/// </summary>
public class RefType
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; } = string.Empty;
}

// ── Context ──────────────────────────────────────────────────────────────────

public class BtckContextCreateParams
{
    [JsonPropertyName("chain_parameters")]
    public ChainParametersParam? ChainParameters { get; set; }
}

public class ChainParametersParam
{
    [JsonPropertyName("chain_type")]
    public string ChainType { get; set; } = string.Empty;
}

public class BtckContextDestroyParams
{
    [JsonPropertyName("context")]
    public RefType? Context { get; set; }
}

// ── Chainstate Manager ────────────────────────────────────────────────────────

public class BtckChainstateManagerCreateParams
{
    [JsonPropertyName("context")]
    public RefType? Context { get; set; }
}

public class BtckChainstateManagerGetActiveChainParams
{
    [JsonPropertyName("chainstate_manager")]
    public RefType? ChainstateManager { get; set; }
}

public class BtckChainstateManagerProcessBlockParams
{
    [JsonPropertyName("chainstate_manager")]
    public RefType? ChainstateManager { get; set; }

    [JsonPropertyName("block")]
    public RefType? Block { get; set; }
}

public class BtckChainstateManagerDestroyParams
{
    [JsonPropertyName("chainstate_manager")]
    public RefType? ChainstateManager { get; set; }
}

// ── Chain ─────────────────────────────────────────────────────────────────────

public class BtckChainGetHeightParams
{
    [JsonPropertyName("chain")]
    public RefType? Chain { get; set; }
}

public class BtckChainGetByHeightParams
{
    [JsonPropertyName("chain")]
    public RefType? Chain { get; set; }

    [JsonPropertyName("block_height")]
    public int BlockHeight { get; set; }
}

public class BtckChainContainsParams
{
    [JsonPropertyName("chain")]
    public RefType? Chain { get; set; }

    [JsonPropertyName("block_tree_entry")]
    public RefType? BlockTreeEntry { get; set; }
}

// ── Block ─────────────────────────────────────────────────────────────────────

public class BtckBlockCreateParams
{
    [JsonPropertyName("raw_block")]
    public string RawBlock { get; set; } = string.Empty;
}

/// <summary>A single <c>block</c> reference parameter.</summary>
public class BtckBlockRefParams
{
    [JsonPropertyName("block")]
    public RefType? Block { get; set; }
}

public class BtckBlockGetTransactionAtParams
{
    [JsonPropertyName("block")]
    public RefType? Block { get; set; }

    [JsonPropertyName("transaction_index")]
    public int TransactionIndex { get; set; }
}

// ── Block Hash ────────────────────────────────────────────────────────────────

public class BtckBlockHashCreateParams
{
    [JsonPropertyName("block_hash")]
    public string BlockHashHex { get; set; } = string.Empty;
}

/// <summary>A single <c>block_hash</c> reference parameter.</summary>
public class BtckBlockHashRefParams
{
    [JsonPropertyName("block_hash")]
    public RefType? BlockHash { get; set; }
}

public class BtckBlockHashEqualsParams
{
    [JsonPropertyName("hash1")]
    public RefType? Hash1 { get; set; }

    [JsonPropertyName("hash2")]
    public RefType? Hash2 { get; set; }
}

// ── Block Header ──────────────────────────────────────────────────────────────

public class BtckBlockHeaderCreateParams
{
    [JsonPropertyName("raw_block_header")]
    public string RawBlockHeader { get; set; } = string.Empty;
}

/// <summary>A single <c>header</c> reference parameter.</summary>
public class BtckBlockHeaderRefParams
{
    [JsonPropertyName("header")]
    public RefType? Header { get; set; }
}

// ── Block Tree Entry ──────────────────────────────────────────────────────────

public class BtckBlockTreeEntryGetBlockHashParams
{
    [JsonPropertyName("block_tree_entry")]
    public RefType? BlockTreeEntry { get; set; }
}

// ── Script Pubkey ─────────────────────────────────────────────────────────────

public class BtckScriptPubkeyCreateParams
{
    [JsonPropertyName("script_pubkey")]
    public string ScriptPubKeyHex { get; set; } = string.Empty;
}

/// <summary>A single <c>script_pubkey</c> reference parameter.</summary>
public class BtckScriptPubkeyRefParams
{
    [JsonPropertyName("script_pubkey")]
    public RefType? ScriptPubKey { get; set; }
}

public class BtckScriptPubkeyVerifyParams
{
    [JsonPropertyName("script_pubkey")]
    public RefType? ScriptPubKey { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("tx_to")]
    public RefType? TxTo { get; set; }

    [JsonPropertyName("precomputed_txdata")]
    public RefType? PrecomputedTxData { get; set; }

    [JsonPropertyName("input_index")]
    public uint InputIndex { get; set; }

    [JsonPropertyName("flags")]
    public JsonElement? Flags { get; set; }
}

public class BtckScriptPubkeyDestroyParams
{
    [JsonPropertyName("script_pubkey")]
    public RefType? ScriptPubKey { get; set; }
}

// ── Transaction ───────────────────────────────────────────────────────────────

public class BtckTransactionCreateParams
{
    [JsonPropertyName("raw_transaction")]
    public string RawTransaction { get; set; } = string.Empty;
}

/// <summary>A single <c>transaction</c> reference parameter.</summary>
public class BtckTransactionRefParams
{
    [JsonPropertyName("transaction")]
    public RefType? Transaction { get; set; }
}

public class BtckTransactionGetInputAtParams
{
    [JsonPropertyName("transaction")]
    public RefType? Transaction { get; set; }

    [JsonPropertyName("input_index")]
    public int InputIndex { get; set; }
}

public class BtckTransactionGetOutputAtParams
{
    [JsonPropertyName("transaction")]
    public RefType? Transaction { get; set; }

    [JsonPropertyName("output_index")]
    public int OutputIndex { get; set; }
}

public class BtckTransactionDestroyParams
{
    [JsonPropertyName("transaction")]
    public RefType? Transaction { get; set; }
}

// ── Transaction Input ─────────────────────────────────────────────────────────

/// <summary>A single <c>transaction_input</c> reference parameter.</summary>
public class BtckTransactionInputRefParams
{
    [JsonPropertyName("transaction_input")]
    public RefType? TransactionInput { get; set; }
}

// ── Transaction Out Point ─────────────────────────────────────────────────────

/// <summary>A single <c>transaction_out_point</c> reference parameter.</summary>
public class BtckTransactionOutPointRefParams
{
    [JsonPropertyName("transaction_out_point")]
    public RefType? TransactionOutPoint { get; set; }
}

// ── Txid ──────────────────────────────────────────────────────────────────────

/// <summary>A single <c>txid</c> reference parameter.</summary>
public class BtckTxidRefParams
{
    [JsonPropertyName("txid")]
    public RefType? Txid { get; set; }
}

public class BtckTxidEqualsParams
{
    [JsonPropertyName("txid1")]
    public RefType? Txid1 { get; set; }

    [JsonPropertyName("txid2")]
    public RefType? Txid2 { get; set; }
}

// ── Transaction Output ────────────────────────────────────────────────────────

public class BtckTransactionOutputCreateParams
{
    [JsonPropertyName("script_pubkey")]
    public RefType? ScriptPubKey { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }
}

/// <summary>A single <c>transaction_output</c> reference parameter.</summary>
public class BtckTransactionOutputRefParams
{
    [JsonPropertyName("transaction_output")]
    public RefType? TransactionOutput { get; set; }
}

public class BtckTransactionOutputDestroyParams
{
    [JsonPropertyName("transaction_output")]
    public RefType? TransactionOutput { get; set; }
}

// ── Precomputed Transaction Data ──────────────────────────────────────────────

public class BtckPrecomputedTransactionDataCreateParams
{
    [JsonPropertyName("tx_to")]
    public RefType? TxTo { get; set; }

    [JsonPropertyName("spent_outputs")]
    public List<RefType>? SpentOutputs { get; set; }
}

public class BtckPrecomputedTransactionDataDestroyParams
{
    [JsonPropertyName("precomputed_txdata")]
    public RefType? PrecomputedTxData { get; set; }
}
