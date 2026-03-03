using System.Text.Json;
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
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    public object? Error { get; set; }
}

/// <summary>
/// An error response with a structured error code.
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorCode? Code { get; set; }
}

/// <summary>
/// Represents an error code with type and member information.
/// </summary>
public class ErrorCode
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("member")]
    public string Member { get; set; } = string.Empty;
}

/// <summary>
/// Helper to build Response objects.
/// </summary>
public static class Responses
{
    /// <summary>
    /// Creates a success response with null result (void operations).
    /// </summary>
    public static Response Null(string id) =>
        new() { Id = id, Result = null, Error = null };

    /// <summary>
    /// Creates a success response with a scalar result value.
    /// </summary>
    public static Response Ok(string id, object result) =>
        new() { Id = id, Result = result, Error = null };

    /// <summary>
    /// Creates a success response with a reference result.
    /// </summary>
    public static Response Ref(string id, string refName) =>
        new() { Id = id, Result = new RefType { Ref = refName }, Error = null };

    /// <summary>
    /// Creates an error response with an empty error object (operation failed, no code details).
    /// </summary>
    public static Response EmptyError(string id) =>
        new() { Id = id, Result = null, Error = new ErrorResponse() };

    /// <summary>
    /// Creates an error response with a typed error code.
    /// </summary>
    public static Response CodedError(string id, string errorType, string errorMember) =>
        new()
        {
            Id = id,
            Result = null,
            Error = new ErrorResponse
            {
                Code = new ErrorCode { Type = errorType, Member = errorMember }
            }
        };
}
