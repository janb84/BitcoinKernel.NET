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
}

/// <summary>
/// Parameters for btck_script_pubkey_verify method.
/// </summary>
public class BtckScriptPubkeyVerifyParams
{
    [JsonPropertyName("script_pubkey_hex")]
    public string ScriptPubKeyHex { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("tx_hex")]
    public string TxHex { get; set; } = string.Empty;

    [JsonPropertyName("input_index")]
    public uint InputIndex { get; set; }

    [JsonPropertyName("spent_outputs")]
    public List<SpentOutput>? SpentOutputs { get; set; }

    [JsonPropertyName("flags")]
    public JsonElement? Flags { get; set; }  // Can be uint, or array
}

/// <summary>
/// Represents a spent output.
/// </summary>
public class SpentOutput
{
    [JsonPropertyName("script_pubkey_hex")]
    public string ScriptPubKeyHex { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public long Value { get; set; }
}
