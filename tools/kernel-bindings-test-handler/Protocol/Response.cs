using System.Text.Json.Serialization;

namespace BitcoinKernel.TestHandler.Protocol;

/// <summary>
/// Represents a response to the test runner.
/// </summary>
public class Response
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Success { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorResponse? Error { get; set; }
}

/// <summary>
/// Represents an error response.
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("variant")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Variant { get; set; }
}
