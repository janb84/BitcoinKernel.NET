# BitcoinKernel.NET

.NET bindings and high-level library for [libbitcoinkernel](https://github.com/bitcoin/bitcoin/tree/master/src/kernel), providing access to Bitcoin Core's consensus and validation logic.


⚠️🚧 This library is still under contruction. ⚠️🚧

This library uses [libbitcoinkernel](https://github.com/bitcoin/bitcoin/tree/master/src/kernel) which is in an experimental state, do not use for production purposes.  

## Overview

BitcoinKernel.NET brings Bitcoin Core's robust consensus engine to .NET applications through a clean, idiomatic C# API. Built on top of libbitcoinkernel, it provides everything from low-level P/Invoke bindings to high-level abstractions for common Bitcoin operations.

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| **BitcoinKernel** | 0.1.0 | High-level API with fluent builder pattern |
| **BitcoinKernel.Core** | 0.1.0 | Managed wrappers and native bindings |


## Quick Start

### Installation

```bash
dotnet add package BitcoinKernel
```

or 

```bash
dotnet add package BitcoinKernel.Core
```

## Architecture

The library is organized in three layers:

1. **BitcoinKernel.Interop** - P/Invoke bindings to libbitcoinkernel (bundled, not published separately)
2. **BitcoinKernel.Core** - Managed C# wrappers with automatic memory management
3. **BitcoinKernel** - High-level facade with fluent API

```
┌─────────────────────────────┐
│      BitcoinKernel          │  ← Fluent API, simple usage
│      (Facade Layer)         │
└─────────────┬───────────────┘
              │
┌─────────────▼───────────────┐
│   BitcoinKernel.Core        │  ← Managed wrappers, IDisposable
│   (Wrapper Layer)           │
└─────────────┬───────────────┘
              │
┌─────────────▼───────────────┐
│  BitcoinKernel.Interop      │  ← P/Invoke bindings
│  (Binding Layer)            │
└─────────────┬───────────────┘
              │
┌─────────────▼───────────────┐
│    libbitcoinkernel         │  ← Native C library
│    (Bitcoin Core)           │
└─────────────────────────────┘
```

## Examples

Explore the [examples](examples/) directory for complete working samples:

- **[BasicUsage](examples/BasicUsage/)** - Getting started with the high-level API
- **[BlockProcessing](examples/BlockProcessing/)** - Block validation and chain management

## Tools

### Kernel Bindings Test Handler

A conformance test handler for Kernel bindings Test handler framework, see [tools/kernel-bindings-test-handler](tools/kernel-bindings-test-handler/) for details.

**Usage:**
```bash
dotnet run --project tools/kernel-bindings-test-handler
```

The handler communicates via stdin/stdout and is designed for automated conformance testing.

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

For other platforms, for now,  you'll need to build libbitcoinkernel from the [Bitcoin Core repository](https://github.com/bitcoin/bitcoin).

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

**Note**: This library provides access to Bitcoin Core's consensus engine. The libbitcoinkernel and this package is stil experimental and not ready for production use. 
