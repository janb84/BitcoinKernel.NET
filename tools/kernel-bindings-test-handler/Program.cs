using System.Text.Json;
using BitcoinKernel.Core;
using BitcoinKernel.TestHandler.Handlers;
using BitcoinKernel.TestHandler.Protocol;

namespace BitcoinKernel.TestHandler;

/// <summary>
/// Test handler for Bitcoin Kernel conformance tests.
/// Implements the JSON-RPC-like protocol for testing bindings.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        // Initialize kernel context
        using var context = new KernelContext();
        var scriptVerifyHandler = new ScriptVerifyHandler(context);

        // Configure JSON serialization options
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        try
        {
            // Read requests line-by-line from stdin
            string? line;
            while ((line = await Console.In.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                Response response;
                try
                {
                    // Parse the request
                    var request = JsonSerializer.Deserialize<Request>(line, jsonOptions);

                    if (request == null)
                    {
                        response = new Response
                        {
                            Id = "unknown",
                            Error = new ErrorResponse
                            {
                                Type = "InvalidRequest"
                            }
                        };
                    }
                    else
                    {
                        response = HandleRequest(request, scriptVerifyHandler, jsonOptions);
                    }
                }
                catch (JsonException)
                {
                    response = new Response
                    {
                        Id = "unknown",
                        Error = new ErrorResponse
                        {
                            Type = "InvalidRequest"
                        }
                    };
                }

                // Write response to stdout
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

    /// <summary>
    /// Routes the request to the appropriate handler.
    /// </summary>
    private static Response HandleRequest(Request request, ScriptVerifyHandler scriptVerifyHandler, JsonSerializerOptions jsonOptions)
    {
        try
        {
            switch (request.Method)
            {
                case "script_pubkey.verify":
                    if (request.Params == null)
                    {
                        return new Response
                        {
                            Id = request.Id,
                            Error = new ErrorResponse
                            {
                                Type = "InvalidParams"
                            }
                        };
                    }

                    var scriptVerifyParams = JsonSerializer.Deserialize<ScriptVerifyParams>(request.Params.Value, jsonOptions);

                    if (scriptVerifyParams == null)
                    {
                        return new Response
                        {
                            Id = request.Id,
                            Error = new ErrorResponse
                            {
                                Type = "InvalidParams"
                            }
                        };
                    }

                    return scriptVerifyHandler.Handle(request.Id, scriptVerifyParams);

                default:
                    return new Response
                    {
                        Id = request.Id,
                        Error = new ErrorResponse
                        {
                            Type = "MethodNotFound"
                        }
                    };
            }
        }
        catch (Exception)
        {
            return new Response
            {
                Id = request.Id,
                Error = new ErrorResponse
                {
                    Type = "InternalError"
                }
            };
        }
    }
}
