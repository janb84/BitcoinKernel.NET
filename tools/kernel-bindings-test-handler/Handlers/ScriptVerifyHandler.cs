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
    public Response Handle(string requestId, ScriptVerifyParams parameters)
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
                    spentOutputs.Add(new TxOut(outputScriptPubKey, output.Amount));
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
                Success = new { }
            };
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "inputIndex")
        {
            return new Response
            {
                Id = requestId,
                Error = new ErrorResponse
                {
                    Type = "ScriptVerify",
                    Variant = "TxInputIndex"
                }
            };
        }
        catch (ScriptVerificationException ex)
        {
            return new Response
            {
                Id = requestId,
                Error = new ErrorResponse
                {
                    Type = "ScriptVerify",
                    Variant = MapScriptVerifyStatus(ex.Status)
                }
            };
        }
        catch (Exception
#if DEBUG
            ex
#endif
            )
        {
            // Log to stderr for debugging (can be disabled in production)
#if DEBUG
            Console.Error.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
#endif

            // Generic error for unexpected exceptions
            return new Response
            {
                Id = requestId,
                Error = new ErrorResponse
                {
                    Type = "ScriptVerify",
                    Variant = "Invalid"
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
            else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                return ParseFlagString(jsonElement.GetString() ?? string.Empty);
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
        return flagStr.ToUpperInvariant() switch
        {
            "VERIFY_NONE" or "NONE" => ScriptVerificationFlags.None,
            "VERIFY_P2SH" or "P2SH" => ScriptVerificationFlags.P2SH,
            "VERIFY_DERSIG" or "DERSIG" => ScriptVerificationFlags.DerSig,
            "VERIFY_NULLDUMMY" or "NULLDUMMY" => ScriptVerificationFlags.NullDummy,
            "VERIFY_CHECKLOCKTIMEVERIFY" or "CHECKLOCKTIMEVERIFY" => ScriptVerificationFlags.CheckLockTimeVerify,
            "VERIFY_CHECKSEQUENCEVERIFY" or "CHECKSEQUENCEVERIFY" => ScriptVerificationFlags.CheckSequenceVerify,
            "VERIFY_WITNESS" or "WITNESS" => ScriptVerificationFlags.Witness,
            "VERIFY_TAPROOT" or "TAPROOT" => ScriptVerificationFlags.Taproot,
            "VERIFY_ALL" or "ALL" => ScriptVerificationFlags.All,
            "VERIFY_ALL_PRE_TAPROOT" or "ALL_PRE_TAPROOT" => ScriptVerificationFlags.AllPreTaproot,
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
            ScriptVerifyStatus.ERROR_TX_INPUT_INDEX => "TxInputIndex",
            ScriptVerifyStatus.ERROR_INVALID_FLAGS => "InvalidFlags",
            ScriptVerifyStatus.ERROR_INVALID_FLAGS_COMBINATION => "InvalidFlagsCombination",
            ScriptVerifyStatus.ERROR_SPENT_OUTPUTS_MISMATCH => "SpentOutputsMismatch",
            ScriptVerifyStatus.ERROR_SPENT_OUTPUTS_REQUIRED => "SpentOutputsRequired",
            _ => "Invalid"
        };
    }
}
