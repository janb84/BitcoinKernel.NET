using System.Text.Json.Serialization;

namespace BitcoinKernel.TestHandler.Protocol;

/// <summary>
/// Represents a response to the test runner.
/// </summary>
public class Response
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorResponse? Error { get; set; }
}

/// <summary>
/// Represents an error response.
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("code")]
    public ErrorCode Code { get; set; } = new();
}

/// <summary>
/// Represents an error code with type and member information.
/// </summary>
public class ErrorCode
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("member")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Member { get; set; }
}
