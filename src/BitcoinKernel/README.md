# BitcoinKernel

A high-level C# library for Bitcoin Core functionality, providing a simple and fluent API for working with Bitcoin blockchain operations via libbitcoinkernel.
⚠️🚧 This library is still under contruction. ⚠️🚧

## Installation

```bash
dotnet add package BitcoinKernel
```

## Quick Start

```csharp
using BitcoinKernel;
using BitcoinKernel.Interop.Enums;

// Create a kernel instance for regtest
using var kernel = KernelLibrary.Create()
    .ForRegtest()
    .Build();

// Get chain information
int height = kernel.GetChainHeight();
byte[] tipHash = kernel.GetChainTipHash();

// Verify a script
bool isValid = kernel.VerifyScript(
    scriptPubKeyHex: "76a9144bfbaf6afb76cc5771bc6404810d1cc041a6933988ac",
    amount: 100000,
    transactionHex: "02000000...",
    inputIndex: 0,
    spentOutputsHex: new List<string>(),
    flags: ScriptVerificationFlags.All
);
```


## Documentation

For more information, visit the [GitHub repository](https://github.com/janB84/BitcoinKernel.NET).

## License

MIT
