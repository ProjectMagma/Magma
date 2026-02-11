# Contributing to Magma

Thank you for your interest in contributing to Magma! We welcome contributions from the community and are excited to have you join us in building a high-performance, low-level network stack library for .NET.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Environment](#development-environment)
- [Building the Project](#building-the-project)
- [Running Tests](#running-tests)
- [Code Style and Conventions](#code-style-and-conventions)
- [Making Changes](#making-changes)
- [Submitting a Pull Request](#submitting-a-pull-request)
- [Reporting Issues](#reporting-issues)
- [Project Structure](#project-structure)
- [Additional Resources](#additional-resources)

## Code of Conduct

This project adheres to a Code of Conduct that we expect all contributors to follow. Please read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing.

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET SDK 10.0.102** (or a newer 10.0.1xx patch, as specified in `global.json`)
- **Git** for version control
- A code editor or IDE with C# support (Visual Studio 2022, VS Code with C# extension, or JetBrains Rider recommended)

### Platform-Specific Requirements

#### Linux (Optional - for platform-specific features)
- **AF_XDP** (recommended): Linux kernel 4.18+ with XDP support enabled
- **NetMap**: NetMap kernel module (legacy option)
- **DPDK**: DPDK libraries and drivers (for maximum performance scenarios)

#### Windows (Optional - for platform-specific features)
- **WinTun** driver from WireGuard project: https://www.wintun.net/

> **Note**: Core library development and most testing can be done without platform-specific requirements. These are only needed when working on transport-specific features.

## Development Environment

### 1. Fork and Clone the Repository

```bash
# Fork the repository on GitHub, then clone your fork
git clone https://github.com/YOUR-USERNAME/Magma.git
cd Magma
```

### 2. Set Up Upstream Remote

```bash
git remote add upstream https://github.com/ProjectMagma/Magma.git
git fetch upstream
```

### 3. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

## Building the Project

Build the entire solution:

```bash
dotnet restore
dotnet build
```

Build in Release mode:

```bash
dotnet build -c Release
```

Build a specific project:

```bash
dotnet build src/Magma.Transport.Tcp/Magma.Transport.Tcp.csproj
```

## Running Tests

Run all tests:

```bash
dotnet test
```

Run tests for a specific project:

```bash
dotnet test test/Magma.Common.Facts
dotnet test test/Magma.Internet.Ip.Facts
dotnet test test/Magma.Link.Facts
```

Run tests with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

Run tests with coverage (if configured):

```bash
dotnet test /p:CollectCoverage=true
```

### Test Naming Convention

Test projects use the `.Facts` suffix (e.g., `Magma.Common.Facts`), not `.Tests`. When adding new tests, follow this convention.

## Code Style and Conventions

Magma follows strict coding conventions to maintain consistency and quality. Please adhere to the following guidelines:

### EditorConfig

The project uses `.EditorConfig` to enforce code style rules. Key conventions include:

- **Indentation**: 4 spaces (2 spaces for JSON/YAML)
- **Charset**: UTF-8
- **Line endings**: LF enforced for shell scripts (`*.sh`) via `.EditorConfig`; other files use Git/editor defaults
- **Insert final newline**: Yes

### C# Coding Standards

#### General Style
- **Use `var`** for all local variable declarations (enforced at error level)
- **No `this.` prefix** for members (enforced at error level)
- **Use language keywords** over framework type names (e.g., `string` not `String`)
- **Prefer expression-bodied members** where applicable
- **Do NOT use throw expressions** (enforced at error level)
- **Use pattern matching** over traditional type checks (`is` with cast, `as` with null check)
- **Prefer `out var`** for inline variable declarations
- **Use `nameof`** instead of string literals when referring to member names

#### Null Handling
- **Nullable reference types are DISABLED** globally. Do not enable them in individual projects.
- Always use explicit null checks where needed
- **Prefer `is null` or `is not null`** over `== null` or `!= null` for consistency
- **Prefer `?.`** for null propagation (e.g., `scope?.Dispose()`)
- **Prefer `??`** coalesce expressions over ternary null checks
- **Use `ObjectDisposedException.ThrowIf`** where applicable

#### Unsafe Code & Performance Patterns

Magma makes extensive use of unsafe code for zero-copy packet processing. Follow these patterns:

- Use `[StructLayout(LayoutKind.Sequential, Pack = 1)]` for all packet header structs
- Use `Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span))` for type-punning packet headers
- Use `Unsafe.SizeOf<T>()` for header size calculations (not `sizeof` or `Marshal.SizeOf`)
- Use `Span<T>`, `Memory<T>`, and `IMemoryOwner<byte>` for buffer management
- Prefer `ref` parameters to avoid unnecessary copies of structs
- Use `System.Net.IPAddress.NetworkToHostOrder()` for endianness conversion

#### Packet Header Pattern

All protocol headers should follow this consistent pattern:

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MyProtocolHeader
{
    // Fields in wire order...

    public static bool TryConsume(ReadOnlySpan<byte> input, out MyProtocolHeader header, out ReadOnlySpan<byte> data)
    {
        if (input.Length >= Unsafe.SizeOf<MyProtocolHeader>())
        {
            header = Unsafe.As<byte, MyProtocolHeader>(ref MemoryMarshal.GetReference(input));
            data = input.Slice(Unsafe.SizeOf<MyProtocolHeader>());
            return true;
        }
        header = default;
        data = default;
        return false;
    }
}
```

#### Testing Conventions
- Test projects use the `.Facts` suffix, not `.Tests`
- Prefer adding tests to existing test files rather than creating new ones
- Do NOT emit "Act", "Arrange", or "Assert" comments in tests
- Avoid regression comments citing GitHub issue/PR numbers unless explicitly requested
- Ensure new code files are listed in the `.csproj` file if other files in that folder are explicitly listed
- Never finish work with tests commented out or disabled that weren't previously so

#### File Format
- For markdown (`.md`) files, ensure there is **no trailing whitespace** at the end of any line

### Namespace and File Organization
- Prefer file-scoped namespace declarations
- One primary type per file
- File names should match the primary type name

## Making Changes

### 1. Keep Changes Focused

- Make small, focused commits
- Each commit should represent a logical unit of change
- Write clear, descriptive commit messages

### 2. Write Tests

- Add tests for new functionality
- Ensure existing tests pass
- Follow existing test patterns and conventions
- Use xUnit framework (already configured in test projects)

### 3. Update Documentation

- Update XML documentation comments for public APIs
- Update README.md if adding major features
- Update relevant docs in the `docs/` directory
- Ensure markdown files have no trailing whitespace

### 4. Follow the Boy Scout Rule

Leave the code cleaner than you found it, but avoid unrelated changes in your PR.

## Submitting a Pull Request

### 1. Ensure Your Branch is Up to Date

```bash
git fetch upstream
git rebase upstream/main
```

### 2. Run Tests and Build

```bash
dotnet build
dotnet test
```

Ensure all tests pass and there are no build warnings or errors.

### 3. Push Your Changes

```bash
git push origin feature/your-feature-name
```

### 4. Create a Pull Request

1. Go to the [Magma repository](https://github.com/ProjectMagma/Magma)
2. Click "New Pull Request"
3. Select your fork and branch
4. Fill out the PR template with:
   - **Description**: Clear explanation of what the PR does
   - **Motivation**: Why this change is needed
   - **Testing**: How you tested the changes
   - **Related Issues**: Link to any related issues

### 5. Code Review Process

- Maintainers will review your PR
- Address any feedback or requested changes
- Once approved, a maintainer will merge your PR

### 6. Pull Request Guidelines

- Keep PRs focused on a single feature or bug fix
- Include tests for new functionality
- Update documentation as needed
- Ensure CI checks pass
- Be responsive to feedback

## Reporting Issues

### Before Opening an Issue

1. Check if the issue already exists in the [issue tracker](https://github.com/ProjectMagma/Magma/issues)
2. Ensure you're using the latest version of Magma
3. Try to isolate the problem and create a minimal reproducible example

### Opening an Issue

When reporting a bug, please include:

- **Description**: Clear description of the issue
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **Steps to Reproduce**: Detailed steps to reproduce the issue
- **Environment**: OS, .NET version, relevant hardware (network cards, etc.)
- **Code Sample**: Minimal code that reproduces the issue (if applicable)

For feature requests, please include:

- **Description**: Clear description of the proposed feature
- **Motivation**: Why this feature would be useful
- **Use Cases**: Real-world scenarios where this would be beneficial
- **Implementation Ideas**: Any thoughts on how it might be implemented (optional)

## Project Structure

Understanding the project structure will help you navigate the codebase:

```
Magma/
├── src/
│   ├── Magma.AF_XDP/         # Linux AF_XDP socket transport
│   ├── Magma.NetMap/          # NetMap kernel-bypass transport
│   ├── Magma.WinTun/          # Windows WinTun TUN interface
│   ├── Magma.PCap/            # Packet capture (PCAP)
│   ├── Magma.Link/            # Data link layer (Ethernet, ARP)
│   ├── Magma.Internet.Ip/     # IPv4/IPv6 implementation
│   ├── Magma.Internet.Icmp/   # ICMP protocol
│   ├── Magma.Transport.Tcp/   # TCP protocol
│   ├── Magma.Transport.Udp/   # UDP protocol
│   ├── Magma.Network/         # Core packet processing
│   ├── Magma.Network.Abstractions/  # Core interfaces
│   └── Magma.Common/          # Shared utilities
├── test/                      # xUnit test projects (*.Facts)
├── samples/                   # Primary sample applications
├── sample/                    # Legacy/experimental sample applications
├── benchmarks/                # Performance benchmarks
├── docs/                      # Documentation
└── .EditorConfig              # Code style rules
```

## Additional Resources

- **README.md**: Project overview and getting started guide
- **CODE_OF_CONDUCT.md**: Community guidelines and standards
- **LICENSE**: Project license information
- **docs/INTEGRATION_GUIDE.md**: Platform integration details
- **Issues**: https://github.com/ProjectMagma/Magma/issues
- **Pull Requests**: https://github.com/ProjectMagma/Magma/pulls

## Questions?

If you have questions about contributing, feel free to:

1. Open a discussion in the GitHub repository
2. Ask in an existing issue or PR
3. Contact the maintainers

Thank you for contributing to Magma! Your efforts help make this project better for everyone.
