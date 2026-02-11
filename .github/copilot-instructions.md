---
excludeAgent: code-review-agent
---

**Any code you commit MUST compile, and new and existing tests related to the change MUST pass.**

You MUST make your best effort to ensure any code changes satisfy those criteria before committing. If for any reason you were unable to build or test code changes, you MUST report that. You MUST NOT claim success unless all builds and tests pass as described above.

If you make code changes, do not complete without checking the relevant code builds and relevant tests still pass after the last edits you make. Do not simply assume that your changes fix test failures you see, actually build and run those tests again to confirm.

Before completing, use the `code-review` skill to review your code changes. Any issues flagged as errors or warnings should be addressed before completing.

If the changes are intended to improve performance, or if they could negatively impact performance, use the `performance-benchmark` skill to validate the impact before completing.

You MUST follow all code-formatting and naming conventions defined in [`.editorconfig`](/.editorconfig).

In addition to the rules enforced by `.editorconfig`, you SHOULD:

- Prefer file-scoped namespace declarations and single-line using directives.
- Ensure that the final return statement of a method is on its own line.
- Use `nameof` instead of string literals when referring to member names.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
- Prefer `?.` if applicable (e.g. `scope?.Dispose()`).
- Use `ObjectDisposedException.ThrowIf` where applicable.
- When adding new unit tests, strongly prefer to add them to existing test code files rather than creating new code files.
- When adding new unit tests, avoid adding a regression comment citing a GitHub issue or PR number unless explicitly asked to include such information.
- If you add new code files, ensure they are listed in the csproj file (if other files in that folder are listed there) so they build.
- When running tests, if possible use filters and check test run counts, or look at test logs, to ensure they actually ran.
- Do not finish work with any tests commented out or disabled that were not previously commented out or disabled.
- When writing tests, do not emit "Act", "Arrange" or "Assert" comments.
- For markdown (`.md`) files, ensure there is no trailing whitespace at the end of any line.
You MUST follow all code-formatting and naming conventions defined in [`.EditorConfig`](/.EditorConfig).

---

# About Magma

Magma is a high-performance, low-level network stack library for .NET. It provides direct packet-level access at the Link, Internet, and Transport layers, enabling zero-copy network I/O through kernel-bypass transports (AF_XDP, NetMap) and TUN/TAP interfaces (WinTun).

## Project Structure

| Directory | Purpose |
|-----------|---------|
| `src/Magma.AF_XDP/` | Linux AF_XDP socket transport (kernel 4.18+) |
| `src/Magma.NetMap/` | NetMap kernel-bypass transport (Linux) |
| `src/Magma.WinTun/` | WinTun TUN interface (Windows) |
| `src/Magma.PCap/` | Packet capture (PCAP file writer) |
| `src/Magma.Link/` | Data link layer (Ethernet, ARP, MAC addresses) |
| `src/Magma.Internet.Ip/` | IPv4/IPv6 packet parsing and construction |
| `src/Magma.Internet.Icmp/` | ICMP protocol support |
| `src/Magma.Transport.Tcp/` | TCP protocol implementation |
| `src/Magma.Transport.Udp/` | UDP protocol implementation |
| `src/Magma.Network/` | Core packet processing with delegate-based chaining |
| `src/Magma.Network.Abstractions/` | `IPacketTransmitter` and `IPacketReceiver` interfaces |
| `src/Magma.Common/` | Shared utilities (checksum, IP address structs) |
| `test/` | xUnit test projects (use `.Facts` suffix) |
| `samples/` and `sample/` | Sample applications |
| `benchmarks/` | Performance benchmarks |

---

# Building & Testing

## Prerequisites

- .NET SDK 10.0.102+ (see `global.json`)

## Build

```bash
dotnet build
```

## Test

```bash
dotnet test
```

To run a specific test project:
```bash
dotnet test test/Magma.Common.Facts
dotnet test test/Magma.Internet.Ip.Facts
dotnet test test/Magma.Link.Facts
dotnet test test/Magma.NetMap.Facts
```

When running tests, check test run counts or look at test logs to ensure they actually ran.

---

# Code Conventions

In addition to the rules enforced by `.EditorConfig`, you SHOULD:

## General Style

- Use `var` in all cases (enforced at error level).
- Prefer file-scoped namespace declarations.
- Use explicit `using` directives (implicit usings are disabled).
- Do not use `this.` prefix (enforced at error level).
- Prefer language keywords over framework type names (e.g. `string` not `String`).
- Prefer expression-bodied members where applicable.
- Do not use throw expressions.
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Prefer `?.` for null propagation (e.g. `scope?.Dispose()`).
- Prefer `??` coalesce expressions over ternary null checks.
- Prefer `out var` for inline variable declarations.
- Use `ObjectDisposedException.ThrowIf` where applicable.

## Nullable Reference Types

- Nullable is **disabled** globally (`Directory.Build.props`). Do not enable it in individual projects without discussion.
- Use explicit null checks where needed.

## Unsafe Code & Performance Patterns

This codebase makes extensive use of unsafe code for zero-copy packet processing. Follow these patterns:

- Use `[StructLayout(LayoutKind.Sequential, Pack = 1)]` for all packet header structs to ensure binary compatibility.
- Use `Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span))` for type-punning packet headers from byte spans.
- Use `Unsafe.SizeOf<T>()` for header size calculations (not `sizeof` or `Marshal.SizeOf`).
- Use `Span<T>`, `Memory<T>`, and `IMemoryOwner<byte>` for buffer management.
- Prefer `ref` parameters to avoid unnecessary copies of structs.
- Use `System.Net.IPAddress.NetworkToHostOrder()` for endianness conversion.

## Packet Header Pattern

All protocol headers follow a consistent `TryConsume` pattern:

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

## Testing Conventions

- Test projects use the `.Facts` suffix (e.g. `Magma.Common.Facts`), not `.Tests`.
- When adding new unit tests, strongly prefer adding them to existing test files rather than creating new ones.
- Do not emit "Act", "Arrange" or "Assert" comments in tests.
- Do not add regression comments citing GitHub issue or PR numbers unless explicitly asked.
- Do not finish work with any tests commented out or disabled that were not previously so.

## Other

- For markdown (`.md`) files, ensure there is no trailing whitespace at the end of any line.
- If you add new code files, ensure they are included in the build (check `.csproj` if other files in that folder are explicitly listed).
