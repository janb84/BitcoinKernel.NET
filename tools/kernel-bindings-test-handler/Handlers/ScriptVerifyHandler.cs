using BitcoinKernel.Core;
using BitcoinKernel.Core.Abstractions;
using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Core.ScriptVerification;
using BitcoinKernel.Interop.Enums;
using BitcoinKernel.TestHandler.Protocol;

namespace BitcoinKernel.TestHandler.Handlers;

/// <summary>
/// Handles script_pubkey.verify method requests.
/// </summary>
public class ScriptVerifyHandler
{
    private readonly KernelContext _context;

    public ScriptVerifyHandler(KernelContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles a script verification request.
    /// </summary>
    public Response Handle(string requestId, BtckScriptPubkeyVerifyParams parameters)
    {
        try
        {
            // Parse input data
            var scriptPubKey = ScriptPubKey.FromHex(parameters.ScriptPubKeyHex);
            var transaction = Transaction.FromHex(parameters.TxHex);

            // Parse spent outputs if provided
            var spentOutputs = new List<TxOut>();
            if (parameters.SpentOutputs != null && parameters.SpentOutputs.Any())
            {
                foreach (var output in parameters.SpentOutputs)
                {
                    var outputScriptPubKey = ScriptPubKey.FromHex(output.ScriptPubKeyHex);
                    spentOutputs.Add(new TxOut(outputScriptPubKey, output.Value));
                }
            }

            // Parse flags
            var flags = ParseFlags(parameters.Flags);

            // Verify the script
            ScriptVerifier.VerifyScript(
                scriptPubKey,
                parameters.Amount,
                transaction,
                parameters.InputIndex,
                spentOutputs,
                flags
            );

            // Success
            return new Response
            {
                Id = requestId,
                Result = true
            };
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "inputIndex")
        {
            return new Response
            {
                Id = requestId,
                Result = null,
                Error = new ErrorResponse
                {
                    Code = new ErrorCode
                    {
                        Type = "btck_ScriptVerifyStatus",
                        Member = "TxInputIndex"
                    }
                }
            };
        }
        catch (ScriptVerificationException ex)
        {
            // If status is OK, the script just failed verification (result: false)
            // If status is not OK, it's an actual error condition
            if (ex.Status == ScriptVerifyStatus.OK)
            {
                return new Response
                {
                    Id = requestId,
                    Result = false
                };
            }

            return new Response
            {
                Id = requestId,
                Result = null,
                Error = new ErrorResponse
                {
                    Code = new ErrorCode
                    {
                        Type = "btck_ScriptVerifyStatus",
                        Member = MapScriptVerifyStatus(ex.Status)
                    }
                }
            };
        }
    }

    /// <summary>
    /// Parses flags from either uint or string format.
    /// </summary>
    private ScriptVerificationFlags ParseFlags(object? flags)
    {
        if (flags == null)
            return ScriptVerificationFlags.None;

        // Handle numeric flags
        if (flags is uint or int or long)
        {
            return (ScriptVerificationFlags)Convert.ToUInt32(flags);
        }

        // Handle System.Text.Json JsonElement
        if (flags.GetType().Name == "JsonElement")
        {
            var jsonElement = (System.Text.Json.JsonElement)flags;
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                return (ScriptVerificationFlags)jsonElement.GetUInt32();
            }
            else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                // Handle array of string flags - combine them with OR
                ScriptVerificationFlags combinedFlags = ScriptVerificationFlags.None;
                foreach (var element in jsonElement.EnumerateArray())
                {
                    if (element.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        combinedFlags |= ParseFlagString(element.GetString() ?? string.Empty);
                    }
                }
                return combinedFlags;
            }
        }

        // Handle string flags
        if (flags is string flagStr)
        {
            return ParseFlagString(flagStr);
        }

        return ScriptVerificationFlags.None;
    }

    /// <summary>
    /// Parses a string flag name to ScriptVerificationFlags.
    /// </summary>
    private ScriptVerificationFlags ParseFlagString(string flagStr)
    {
        // Handle btck_ prefixed format (e.g., "btck_ScriptVerificationFlags_WITNESS")
        if (flagStr.StartsWith("btck_ScriptVerificationFlags_", StringComparison.OrdinalIgnoreCase))
        {
            flagStr = flagStr.Substring("btck_ScriptVerificationFlags_".Length);
        }

        return flagStr.ToUpperInvariant() switch
        {
            "NONE" => ScriptVerificationFlags.None,
            "P2SH" => ScriptVerificationFlags.P2SH,
            "DERSIG" => ScriptVerificationFlags.DerSig,
            "NULLDUMMY" => ScriptVerificationFlags.NullDummy,
            "CHECKLOCKTIMEVERIFY" => ScriptVerificationFlags.CheckLockTimeVerify,
            "CHECKSEQUENCEVERIFY" => ScriptVerificationFlags.CheckSequenceVerify,
            "WITNESS" => ScriptVerificationFlags.Witness,
            "TAPROOT" => ScriptVerificationFlags.Taproot,
            "ALL" => ScriptVerificationFlags.All,
            "ALL_PRE_TAPROOT" => ScriptVerificationFlags.AllPreTaproot,
            _ => throw new ArgumentException($"Unknown flag: {flagStr}")
        };
    }

    /// <summary>
    /// Maps ScriptVerifyStatus to error variant strings.
    /// </summary>
    private string MapScriptVerifyStatus(ScriptVerifyStatus status)
    {
        return status switch
        {
            ScriptVerifyStatus.ERROR_INVALID_FLAGS_COMBINATION => "ERROR_INVALID_FLAGS_COMBINATION",
            ScriptVerifyStatus.ERROR_SPENT_OUTPUTS_REQUIRED => "ERROR_SPENT_OUTPUTS_REQUIRED",
            _ => "ERROR_INVALID"
        };
    }
}
