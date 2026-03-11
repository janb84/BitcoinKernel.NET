# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-03-03

### Breaking Changes
- Removed `BitcoinKernel` facade package and `KernelLibrary` fluent builder; consumers now use the managed API directly
- Renamed package/namespaces from `BitcoinKernel.Core` to `BitcoinKernel`
- `Chain` class moved from `BitcoinKernel.Abstractions` to `BitcoinKernel.Chain`; import `using BitcoinKernel.Chain;` to access it
 - `BitcoinKernel.Abstractions` namespace renamed to `BitcoinKernel.Primitives`; update any `using BitcoinKernel.Abstractions;` imports accordingly
- `LoggingConnection` moved from the global namespace into `BitcoinKernel`; add `using BitcoinKernel;` if not already present

### Changed
- Examples rewritten to use the managed API directly without the fluent builder

## [0.1.2] - 2026-01-26

### Added
- Kernel bindings test handler for conformance testing
- Support for block validation and chain management in examples
- Improved project documentation and examples

### Changed
- N/A

### Fixed
- Minor bug fixes and stability improvements

### Security
- N/A

## [0.1.1] - Previous Release

Initial pre-release version with basic functionality.

---

**Note**: This library is still in early development. Version 0.1.x releases are pre-release and not recommended for production use.
