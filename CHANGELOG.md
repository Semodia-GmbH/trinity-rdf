# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.2.0 - 2023-03-23

### Changed
- Nuget and Assembly names changed to Semodia, to resolve naming conflicts with the original package
- Updated Changelog

## 1.1.0 - 2023-03-22

Initial changelog entry after fork. 

### Added
- Support for Byte and SByte serialization
- Support for .NET 6.0
  - Support for linux
  - Support for macOS
- Preliminary support for multi-language strings
- Semodia copyright notice for derived work

### Removed
- .NET 3/4 target
  - Binaries are now using .NET 6.0
  - Libraries are compiled for .netstandard2.0

### Changed
- Switched project files to modern SDK style
- Refactoring
  - Use `var` instead of types
  - Use switch expressions for large if constructs
  - Updated used nuget packages to latest versions

### Fixed
- multiple typos