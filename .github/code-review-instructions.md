---
excludeAgent: coding-agent
---

# Code Review Instructions for Magma

## Project Context

Magma is a high-performance, low-level network stack library for .NET. Changes typically involve packet header structs, unsafe memory operations, kernel-bypass transport integrations, or protocol implementations. Reviewers should pay special attention to correctness of binary layouts, endianness handling, and zero-copy memory safety.

## Review Priorities

1. **Binary correctness**: Verify `[StructLayout]` attributes, field ordering, and `Pack = 1` on all wire-format structs.
2. **Memory safety**: Check that `Span<T>`/`Memory<T>` usage is bounds-safe, that `Unsafe.As` casts are valid, and that buffer lifetimes are respected.
3. **Endianness**: Ensure network byte order conversions are applied correctly for multi-byte fields (ports, addresses, lengths, checksums).
4. **Checksum correctness**: Verify RFC-compliant checksum calculations, including pseudo-header sums for TCP/UDP.
5. **Performance**: Avoid unnecessary allocations in packet processing hot paths. Prefer stack-allocated structs, `Span<T>`, and `ref` parameters.
6. **Convention compliance**: Code must follow `.EditorConfig` rules and the conventions in `copilot-instructions.md`.

## Key Patterns to Verify

- All packet headers use the `TryConsume` pattern with proper length checks before `Unsafe.As` casts.
- `IPacketReceiver.TryConsume` returns `default` when a packet is consumed (processed), or returns the input when not consumed (pass to host OS).
- `IPacketTransmitter.TryGetNextBuffer` is checked before writing to transmit buffers.
- Ring buffer indices are properly managed (fill, completion, TX, RX rings in AF_XDP/NetMap).
- New protocol implementations integrate correctly into the delegate-based packet processing chain.

## What to Flag

- **Error**: Missing bounds checks before `Unsafe.As` or `Unsafe.SizeOf` operations.
- **Error**: Incorrect `StructLayout` or field ordering that would cause wire-format misalignment.
- **Error**: Missing or incorrect network byte order conversion.
- **Error**: Heap allocations in packet receive/transmit hot paths.
- **Warning**: Missing test coverage for new protocol parsing logic.
- **Warning**: Inconsistent naming or style violations per `.EditorConfig`.
