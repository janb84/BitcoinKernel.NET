# BitcoinKernel.NET

.NET bindings for [libbitcoinkernel](https://github.com/bitcoin/bitcoin/tree/master/src/kernel), providing access to Bitcoin Core's consensus and validation logic.

⚠️🚧 This library is still under construction. ⚠️🚧

This library uses [libbitcoinkernel](https://github.com/bitcoin/bitcoin/tree/master/src/kernel) which is in an experimental state, do not use for production purposes.

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| **BitcoinKernel** | 0.2.0 | Managed wrappers and native bindings |

```bash
dotnet add package BitcoinKernel
```

## Architecture

The library is organized in two layers:

1. **BitcoinKernel.Interop** - P/Invoke bindings to libbitcoinkernel (bundled, not published separately)
2. **BitcoinKernel** - Managed C# wrappers with automatic memory management

```
┌─────────────────────────────┐
│   BitcoinKernel             │  ← Managed wrappers, IDisposable
│   (Wrapper Layer)           │
└─────────────┬───────────────┘
              │
┌─────────────▼───────────────┐
│  BitcoinKernel.Interop      │  ← P/Invoke bindings (bundled)
│  (Binding Layer)            │
└─────────────┬───────────────┘
              │
┌─────────────▼───────────────┐
│    libbitcoinkernel         │  ← Native C library
│    (Bitcoin Core)           │
└─────────────────────────────┘
```

## Quick Start

```csharp
using BitcoinKernel;
using BitcoinKernel.Chain;
using BitcoinKernel.Interop.Enums;

using var logging = new LoggingConnection((category, message, level) =>
    Console.WriteLine($"[{category}] {message}"));

using var chainParams = new ChainParameters(ChainType.MAINNET);
using var contextOptions = new KernelContextOptions().SetChainParams(chainParams);
using var context = new KernelContext(contextOptions);
using var options = new ChainstateManagerOptions(context, dataDir, blocksDir);
using var chainstate = new ChainstateManager(context, chainParams, options);

var chain = chainstate.GetActiveChain();
Console.WriteLine($"Height: {chain.Height}");
Console.WriteLine($"Genesis: {Convert.ToHexString(chain.GetGenesis().GetBlockHash())}");
```

## Examples

Explore the [examples](examples/) directory for complete working samples:

- **[BasicUsage](examples/BasicUsage/)** - Getting started with chain queries
- **[BlockProcessing](examples/BlockProcessing/)** - Block validation and processing

## Tools

### Kernel Bindings Test Handler

A conformance test handler for the Kernel bindings test framework, see [tools/kernel-bindings-test-handler](tools/kernel-bindings-test-handler/) for details.

```bash
dotnet run --project tools/kernel-bindings-test-handler
```

## Building from Source

### Prerequisites

- .NET 9.0 SDK or later

### Build

```bash
git clone https://github.com/janb84/BitcoinKernel.NET.git
cd BitcoinKernel.NET
dotnet build
```

### Run Tests

```bash
dotnet test
```

## Native Library

This package includes pre-built `libbitcoinkernel` binaries for:
- macOS (x64, ARM64)
- others will follow

For other platforms, you'll need to build libbitcoinkernel from the [Bitcoin Core repository](https://github.com/bitcoin/bitcoin).

## Documentation

- [API Documentation](docs/) (coming soon)
- [libbitcoinkernel Documentation](https://thecharlatan.ch/kernel-docs/index.html)
- [Bitcoin Core Developer Documentation](https://github.com/bitcoin/bitcoin/tree/master/doc)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built on [libbitcoinkernel](https://github.com/bitcoin/bitcoin/tree/master/src/kernel) from Bitcoin Core

**Note**: This library provides access to Bitcoin Core's consensus engine. libbitcoinkernel and this package are still experimental and not ready for production use.
