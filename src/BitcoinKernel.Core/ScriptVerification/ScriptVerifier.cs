using System;
using System.Runtime.InteropServices;

using BitcoinKernel.Core.Exceptions;
using BitcoinKernel.Interop;
using BitcoinKernel.Interop.Enums;

using BitcoinKernel.Core.Abstractions;

namespace BitcoinKernel.Core.ScriptVerification;

/// <summary>
/// Handles script verification operations.
/// </summary>
public static class ScriptVerifier
{

    /// <summary>
    /// Verifies a script pubkey against a transaction input, throwing an exception on error.
    /// </summary>
    /// <param name="scriptPubkey">The output script to verify against.</param>
    /// <param name="amount">The amount of the output being spent.</param>
    /// <param name="transaction">The transaction containing the input to verify.</param>
    /// <param name="inputIndex">The index of the transaction input to verify within the transaction.</param>
    /// <param name="spentOutputs">The outputs being spent by the transaction .</param>
    /// <param name="flags">Script verification flags to use.</param>
    /// <exception cref="ScriptVerificationException">Thrown when verification fails with an error status.</exception>
    public static bool VerifyScript(
        ScriptPubKey scriptPubkey,
        long amount,
        Transaction transaction,
        uint inputIndex,
        List<TxOut> spentOutputs,
        ScriptVerificationFlags flags = ScriptVerificationFlags.All)
    {
        var inputCount = transaction.InputCount;

        if (inputIndex >= inputCount)
        {
            throw new ArgumentOutOfRangeException(nameof(inputIndex),
                $"Input index {inputIndex} is out of bounds for transaction with {inputCount} inputs");
        }

        if (spentOutputs.Any() && spentOutputs.Count != inputCount)
        {
            throw new ScriptVerificationException(
                ScriptVerifyStatus.ERROR_SPENT_OUTPUTS_MISMATCH,
                $"Spent outputs count ({spentOutputs.Count}) does not match transaction input count ({inputCount})");
        }

        if (((uint)flags & ~(uint)ScriptVerificationFlags.All) != 0)
        {
            throw new ScriptVerificationException(
                ScriptVerifyStatus.ERROR_INVALID_FLAGS,
                $"Invalid script verification flags: 0x{flags:X}");
        }

        // Create spent outputs
        var kernelSpentOutputs = spentOutputs.Any()
            ? spentOutputs.Select(utxo => utxo.Handle).ToArray()
            : null;



        // Verify script
        // Allocate memory for status byte
        IntPtr statusPtr = Marshal.AllocHGlobal(1);
        try
        {
            int result = NativeMethods.ScriptPubkeyVerify(
                scriptPubkey.Handle,
                amount,
                transaction.Handle,
                kernelSpentOutputs,
                (nuint)spentOutputs.Count,
                inputIndex,
                (uint)flags,
                statusPtr);
            byte statusCode = Marshal.ReadByte(statusPtr);
            var status = (ScriptVerifyStatus)statusCode;

            // Check for errors
            if (status != ScriptVerifyStatus.OK)
            {
                throw new ScriptVerificationException(status, $"Script verification failed: {status}");
            }

            // Even if status is OK, result==0 means script verification failed
            if (result == 0)
            {
                throw new ScriptVerificationException(status, "Script verification failed");
            }

        }
        finally
        {
            Marshal.FreeHGlobal(statusPtr);
        }
        return true;
    }
}