# Kernel Bindings Test Handler

This is a conformance test handler for the BitcoinKernel.NET library. 
It implements the protocol specification [kernel-bindings-test handler-spec](https://github.com/stringintech/kernel-bindings-tests/blob/main/docs/handler-spec.md) for testing Bitcoin Kernel bindings via stdin/stdout JSON-RPC-like communication.

## Overview

The handler is a console application that:
- Reads JSON requests line-by-line from stdin
- Processes each request using the BitcoinKernel.Core library
- Writes JSON responses to stdout
- Exits cleanly when stdin closes

## Protocol

### Communication
- **Input**: JSON requests on stdin (one per line)
- **Output**: JSON responses on stdout (one per line)

### Request Format
```json
{
  "id": "unique-request-id",
  "method": "method_name",
  "params": { /* method-specific parameters */ }
}
```

### Response Format
**Success:**
```json
{
  "id": "unique-request-id",
  "success": { /* result */ }
}
```

**Error:**
```json
{
  "id": "unique-request-id",
  "error": {
    "type": "error_category",
    "variant": "specific_error"
  }
}
```

## Supported Methods

### `script_pubkey.verify`

Verifies a Bitcoin script pubkey against a transaction input.

**Parameters:**
- `script_pubkey` (string): Hex-encoded script pubkey
- `amount` (number): Amount of the output being spent
- `transaction` (string): Hex-encoded transaction
- `input_index` (number): Index of the transaction input to verify
- `spent_outputs` (array, optional): Array of spent outputs
  - Each output contains: `script_pubkey` (string), `amount` (number)
- `script_verify_flags` (number): Script verification flags

**Success Response:**
```json
{
  "id": "test-id",
  "success": {}
}
```

**Error Variants:**
- `TxInputIndex`: Input index is out of bounds
- `InvalidFlags`: Invalid verification flags
- `InvalidFlagsCombination`: Invalid flag combination
- `SpentOutputsMismatch`: Spent outputs count doesn't match input count
- `SpentOutputsRequired`: Spent outputs required but not provided
- `Invalid`: Script verification failed

## Building

### Build for Development
```bash
dotnet build
```

### Build Release Binary
```bash
./build.sh
```

This creates a compiled binary at `bin/kernel-bindings-test-handler` that can be invoked directly without `dotnet run`.

## Running

### Option 1: Run with dotnet (development)
```bash
dotnet run --project tools/kernel-bindings-test-handler
```

### Option 2: Run compiled binary (production)
```bash
./tools/kernel-bindings-test-handler/bin/kernel-bindings-test-handler
```

### invoke via Bitcoin Kernel Binding Conformance Tests framework.
Clone the repo and build.

Execute the test: 
```bash
 ./build/runner --handler /Users/arjan/Projects/BitcoinKernel.NET/tools/kernel-bindings-test-handler/bin/kernel-bindings-test-handler
```

## Testing

The handler is designed to be used with conformance test suite. Example:

```bash
echo '{"id":"1","method":"script_pubkey.verify","params":{...}}' | \
  ./bin/kernel-bindings-test-handler
```

## Project Structure

```
tools/kernel-bindings-test-handler/
|-- kernel-bindings-test-handler.csproj
|-- Program.cs                 # Main entry point and request router
|-- Protocol/
|   |-- Request.cs            # Request message definitions
|   |-- Response.cs           # Response message definitions
|-- Handlers/
|   |-- ScriptVerifyHandler.cs # Script verification handler
|-- Bin/
|   |-- Compiled binaries useable for the Bitcoin Kernel Binding Conformance Tests framework
```

## Dependencies

- BitcoinKernel.Core: The core library being tested
- System.Text.Json: JSON serialization

## Error Handling

The handler maps BitcoinKernel.Core exceptions to protocol error responses:

| Exception | Error Type | Error Variant |
|-----------|-----------|---------------|
| ArgumentOutOfRangeException (inputIndex) | ScriptVerify | TxInputIndex |
| ScriptVerificationException (ERROR_INVALID_FLAGS) | ScriptVerify | InvalidFlags |
| ScriptVerificationException (ERROR_INVALID_FLAGS_COMBINATION) | ScriptVerify | InvalidFlagsCombination |
| ScriptVerificationException (ERROR_SPENT_OUTPUTS_MISMATCH) | ScriptVerify | SpentOutputsMismatch |
| ScriptVerificationException (ERROR_SPENT_OUTPUTS_REQUIRED) | ScriptVerify | SpentOutputsRequired |
| Any other exception | ScriptVerify | Invalid |
