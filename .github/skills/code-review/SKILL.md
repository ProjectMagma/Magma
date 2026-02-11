---
name: code-review
description: Review code changes in Magma for correctness, performance, and consistency with project conventions. Use when reviewing PRs or code changes.
---

# Magma Code Review

Review code changes against conventions and patterns established in the Magma codebase. Magma is a high-performance, low-level network stack library for .NET that provides zero-copy packet processing through kernel-bypass transports (AF_XDP, NetMap) and TUN/TAP interfaces (WinTun).

**Reviewer mindset:** Be polite but very skeptical. Your job is to find problems the author may have missed and to question the approach. Treat the PR description and linked issues as claims to verify, not facts to accept.

## When to Use This Skill

Use this skill when:
- Reviewing a PR or code change in Magma
- Checking code for correctness, performance, style, or consistency issues before submitting a PR
- Asked to review, critique, or provide feedback on code changes
- Validating that a change follows Magma conventions

## Review Process

### Step 0: Gather Code Context (No PR Narrative Yet)

Before analyzing anything, collect as much relevant **code** context as you can. **Do NOT read the PR description, linked issues, or existing review comments yet.** You must form your own independent assessment before being exposed to the author's framing.

1. **Diff and file list**: Fetch the full diff and the list of changed files.
2. **Full source files**: For every changed file, read the **entire source file** (not just the diff hunks). You need surrounding code to understand invariants, packet processing chains, and memory lifetimes.
3. **Consumers and callers**: If the change modifies a public/internal API, an interface method, or a packet processing delegate, search for how consumers use the functionality. Understanding how the code is consumed reveals whether the change could break existing behavior.
4. **Sibling types and related code**: If the change fixes a bug or adds a pattern in one transport or protocol, check whether other transports or protocols have the same issue or need the same fix.
5. **Key utility/helper files**: If the diff calls into shared utilities (e.g. `Checksum`, `IPAddress` types), read those to understand the contracts.
6. **Git history**: Check recent commits to the changed files (`git log --oneline -20 -- <file>`). Look for related recent changes or prior attempts to fix the same problem.

### Step 1: Form an Independent Assessment

Based **only** on the code context gathered above (without the PR description or issue), answer these questions:

1. **What does this change actually do?** Describe the behavioral change in your own words.
2. **Why might this change be needed?** Infer the motivation from the code itself.
3. **Is this the right approach?** Would a simpler alternative be more consistent with the codebase?
4. **What problems do you see?** Identify bugs, edge cases, memory safety issues, performance regressions, and anything else that concerns you.

Write down your independent assessment before proceeding. You must produce a holistic assessment (see [Holistic PR Assessment](#holistic-pr-assessment)) at this stage.

### Step 2: Incorporate PR Narrative and Reconcile

Now read the PR description, labels, linked issues, author information, and existing review comments. Treat all of this as **claims to verify**, not facts to accept.

1. **PR metadata**: Fetch the PR description, labels, linked issues, and author.
2. **Existing review comments**: Check if there are already review comments on the PR to avoid duplicating feedback.
3. **Reconcile your assessment with the author's claims.** Where your independent reading of the code disagrees with the PR description, investigate further — but do not simply defer to the author's framing.
4. **Update your holistic assessment** if the additional context reveals information that genuinely changes your evaluation.

### Step 3: Detailed Analysis

1. **Focus on what matters.** Prioritize binary layout correctness, memory safety, endianness bugs, checksum errors, performance regressions in hot paths, and protocol correctness. Do not comment on trivial style issues unless they violate an explicit rule below.
2. **Consider collateral damage.** For every changed code path, actively brainstorm: what other scenarios, callers, or inputs flow through this code? Could any of them break or behave differently after this change?
3. **Be specific and actionable.** Every comment should tell the author exactly what to change and why. Reference the relevant convention. Include evidence of how you verified the issue is real.
4. **Flag severity clearly:**
   - ❌ **error** — Must fix before merge. Bugs, safety issues, protocol violations, test gaps for behavior changes.
   - ⚠️ **warning** — Should fix. Performance issues, missing validation, inconsistency with established patterns.
   - 💡 **suggestion** — Consider changing. Style improvements, minor readability wins, optional optimizations.
5. **Don't pile on.** If the same issue appears many times, flag it once with a note listing all affected locations.
6. **Respect existing style.** When modifying existing files, the file's current style takes precedence over general guidelines.
7. **Don't flag what CI catches.** Do not flag issues that a compiler or CI build step would catch.
8. **Avoid false positives.** Before flagging any issue, verify the concern actually applies given the full context.

## Multi-Model Review

When the environment supports launching sub-agents with different models (e.g., the `task` tool with a `model` parameter), run the review in parallel across multiple model families to get diverse perspectives. If the environment does not support this, proceed with a single-model review.

**How to execute (when supported):**
1. Inspect the available model list and select one model from each distinct model family. Use at least 2 and at most 4 models. Pick the highest capability tier from each family. Do not select the same model that is already running the primary review.
2. Launch a sub-agent for each selected model in parallel, giving each the same review prompt.
3. Wait for all agents to complete, then synthesize: deduplicate findings, elevate issues flagged by multiple models, and include unique findings that meet the confidence bar. **Timeout:** If a sub-agent has not completed after 10 minutes, proceed with the results you have.
4. Present a single unified review, noting when an issue was flagged by multiple models.

---

## Review Output Format

### Structure

```
## Code Review — PR #<number>

### Holistic Assessment

**Motivation**: <1-2 sentences on whether the PR is justified and the problem is real>

**Approach**: <1-2 sentences on whether the fix/change takes the right approach>

**Summary**: <✅ LGTM / ⚠️ Needs Human Review / ⚠️ Needs Changes / ❌ Reject>. <2-3 sentence summary of the overall verdict and key points.>

---

### Detailed Findings

#### ✅/⚠️/❌ <Category Name> — <Brief description>

<Explanation with specifics. Reference code, line numbers, etc.>

(Repeat for each finding category.)
```

### Guidelines

- **Holistic Assessment** comes first and covers Motivation, Approach, and Summary.
- **Detailed Findings** uses emoji-prefixed category headers:
  - ✅ for things that are correct / look good
  - ⚠️ for warnings or impactful suggestions
  - ❌ for errors (must fix before merge)
  - 💡 for minor suggestions or observations
- **Summary** gives a clear verdict: LGTM, Needs Human Review, Needs Changes, or Reject.
- **Never give a blanket LGTM when you are unsure.** Use "Needs Human Review" instead.

### Verdict Consistency Rules

1. **The verdict must reflect your most severe finding.** If you have any ⚠️ findings, the verdict cannot be "LGTM."
2. **When uncertain, always escalate to human review.**
3. **Separate code correctness from approach completeness.** A change can be correct code that is an incomplete approach.
4. **Classify each ⚠️ and ❌ finding as merge-blocking or advisory.**
5. **Devil's advocate check before finalizing.** Re-read all your ⚠️ findings and ask: does this represent an unresolved concern?

---

## Holistic PR Assessment

Before reviewing individual lines of code, evaluate the PR as a whole.

### Motivation & Justification

- **Every PR must articulate what problem it solves and why.** Don't accept vague or absent motivation.
- **Challenge every addition with "Do we need this?"** New abstractions, APIs, and flags must justify their existence.
- **Require performance data for optimization PRs.** Never accept performance claims at face value.

### Approach & Alternatives

- **Check whether the PR solves the right problem at the right layer.** Prefer fixing the actual source of an issue over adding workarounds.
- **Ask "Why not just X?" — always prefer the simplest solution.**
- **Explicitly weigh whether the change is a net positive.** Complexity is a first-class cost.

### Scope & Focus

- **Require large or mixed PRs to be split into focused changes.** Each PR should address one concern.
- **Defer tangential improvements to follow-up PRs.**

---

## Correctness & Safety

### Binary Layout & Wire Format

- **All packet header structs must use `[StructLayout(LayoutKind.Sequential, Pack = 1)]`.** Missing or incorrect attributes will cause wire-format misalignment. Verify field ordering matches the protocol specification.
- **Verify `TryConsume` bounds checks.** Every `TryConsume` method must check `input.Length >= Unsafe.SizeOf<T>()` before performing `Unsafe.As` casts. Missing bounds checks are buffer overread vulnerabilities.
- **Verify `Unsafe.As<byte, T>()` is only used on pinned or stack-local spans.** The reference must not move during the cast.
- **Header fields must be in wire order.** The struct field order must exactly match the protocol's on-wire byte layout. Bit fields must use the correct shifting and masking.

### Endianness

- **All multi-byte wire fields must handle network byte order.** Ports, lengths, addresses, protocol numbers, and checksums are big-endian on the wire. Use `IPAddress.NetworkToHostOrder()` or `BinaryPrimitives` for conversion.
- **Verify endianness in both read and write paths.** A common bug is converting correctly on read but forgetting to convert back on write (or vice versa).
- **EtherType values are already in network byte order.** The `EtherType` enum values in Magma are defined in network byte order — do not double-convert.

### Checksum

- **Verify RFC-compliant Internet checksum calculations.** Checksums must follow RFC 791/793/1071/1141/1624. Check for correct one's complement addition with carry.
- **TCP/UDP checksums require pseudo-header sums.** Verify that the pseudo-header (source IP, destination IP, protocol, length) is included.
- **Incremental checksum updates must preserve correctness.** When modifying a field in-place, the checksum must be updated incrementally or recomputed from scratch.

### Memory Safety

- **Buffer lifetimes must be respected.** `Span<T>` and `ref` references must not outlive the buffer they point into. Watch for captures of `Span` in closures or async methods (which is a compiler error, but verify related patterns).
- **`IMemoryOwner<byte>` must be disposed or passed along.** The `TryConsume` pattern on `IPacketReceiver` returns `default` to indicate the packet was consumed (and the owner is now managed by the receiver). Returning the input means the caller still owns it.
- **Ring buffer index management must be correct.** In AF_XDP and NetMap transports, verify that fill/completion/TX/RX ring indices are properly advanced and wrapped. Double-reservation or missed replenishment causes packet loss or hangs.

### Transport-Specific Correctness

- **AF_XDP**: Verify UMEM frame management (allocation, deallocation, recycling), XDP socket bind options, and that the fill ring is replenished after consuming from the RX ring.
- **NetMap**: Verify ring slot management, `NETMAP_BUF` offset calculations, and that `nm_ring_next` is used correctly.
- **WinTun**: Verify WinTun session packet allocation and release patterns, and correct use of async event handles.

### Error Handling

- **Use `Debug.Assert` for internal invariants, not exceptions.** For internal-only callers, assert assumptions rather than throwing.
- **Include actionable details in exception messages.** Use `nameof` for parameter names.
- **Use `ThrowIf` helpers over manual checks.** Use `ObjectDisposedException.ThrowIf`, `ArgumentOutOfRangeException.ThrowIfNegative`, etc.

### Thread Safety

- **Packet receive loops run on dedicated threads.** Verify that shared state accessed from receive threads is properly synchronized.
- **Use `Volatile` or `Interlocked` for cross-thread field access.** Fields written on one thread and read on another must use appropriate synchronization.

---

## Performance & Allocations

### Hot Path Rules

Packet receive and transmit paths are the hottest code in Magma. These rules apply strictly to code in those paths:

- **Zero heap allocations.** No `new` objects, no closures, no boxing, no LINQ, no string operations in packet processing loops.
- **Use stack-allocated structs.** All packet headers are value types for a reason — do not box them or store them on the heap.
- **Use `Span<T>` and `ref` parameters.** Avoid copying packet data. The zero-copy architecture is fundamental to Magma's performance.
- **Minimize branching.** Prefer branchless patterns where possible in inner loops.

### General Performance

- **Pre-allocate collections when size is known.** Pass capacity to `Dictionary`, `HashSet`, `List` constructors.
- **Avoid closures and delegate allocations.** When a lambda captures locals, it allocates a closure object on every invocation.
- **Cache repeated accessor calls in locals.** Store results of repeated property/getter calls in local variables.
- **Place cheap checks before expensive operations.** Order conditionals so cheapest/most-common checks come first.

---

## Code Style & Formatting

All rules from [`.EditorConfig`](/.EditorConfig) are enforced. Key rules:

- **Use `var` in all cases** (enforced at error level).
- **No `this.` prefix** (enforced at error level).
- **Prefer language keywords** (`string` not `String`, `int` not `Int32`) (enforced at error level).
- **No throw expressions** (enforced at error level).
- **Prefer null propagation** (`?.`) and coalesce expressions (`??`) (enforced at error level).
- **Prefer `out var`** for inline declarations (enforced at error level).
- **Prefer expression-bodied members** where applicable (suggestion level).
- **Prefer pattern matching** over `is` with cast check or `as` with null check (warning level).
- **Allman-style braces** — newline before all opening braces.
- **Sort `using` directives** with `System` first.
- **Use explicit `using` directives** — implicit usings are disabled.
- **Nullable reference types are disabled globally.** Do not enable per-project.

### Additional Conventions

- **Use `Unsafe.SizeOf<T>()` for header size calculations** (not `sizeof` or `Marshal.SizeOf`).
- **Use PascalCase for constants.**
- **Prefer early return to reduce nesting.**
- **Delete dead code and unnecessary wrappers.** Remove obsolete fields and unused variables when encountered.
- **Do not initialize fields to default values.** The CLR zero-initializes fields; explicit `= false`, `= 0`, `= null` is redundant.

---

## Testing

- **Test projects use the `.Facts` suffix** (e.g. `Magma.Common.Facts`), not `.Tests`.
- **Always add regression tests for bug fixes and behavior changes.** Prefer adding test cases to existing test files rather than creating new ones.
- **Do not emit "Act", "Arrange" or "Assert" comments** in test methods.
- **Do not add regression comments citing issue numbers** unless explicitly asked.
- **Test assertions must be specific.** Assert exact expected values, not broad conditions.
- **Test edge cases.** Include empty spans, minimum-size packets, maximum-size packets, and malformed headers.
- **Packet parsing tests should use hex byte arrays.** Follow the existing pattern of `"00-15-5D-..."` style hex string conversion.
- **Do not finish work with tests commented out or disabled** that were not previously so.
- **When running tests, verify they actually ran** by checking test run counts in the output.

---

## Protocol Implementation

- **Follow the relevant RFC specification.** When implementing or modifying protocol headers, cite the RFC section number in comments for non-obvious field layouts.
- **Bit field manipulation must be correct.** Verify shift amounts and masks for packed fields (e.g. IPv4 version + IHL in one byte, TCP data offset + flags).
- **New protocol implementations must integrate into the packet processing chain.** Verify that `ConsumeInternetLayer`, `IPv4ConsumeTransportLayer`, or equivalent delegates are updated to route to the new protocol.
- **Checksum-validated protocols must verify checksums on receive and compute them on transmit.**

---

## Kestrel Integration

Magma integrates with ASP.NET Core Kestrel as a custom transport:

- **`ITransport` implementations** (AF_XDP, NetMap) must correctly implement `BindAsync`/`UnbindAsync` lifecycle.
- **`TcpConnection<T>` extends Kestrel's `TransportConnection`.** Changes to connection state management must respect Kestrel's threading model and `PipeScheduler`.
- **`IPacketReceiver` implementations** must correctly return `default` (consumed) or input (not consumed) to control whether packets are passed to the host OS.
- **Memory pool integration** must use Kestrel's shared `MemoryPool<byte>` for pipeline buffers.

---

## Documentation & Comments

- **Comments should explain why, not restate code.** Delete comments that just duplicate the code in English.
- **Delete or update obsolete comments when code changes.**
- **For markdown (`.md`) files, ensure no trailing whitespace** at the end of any line.
- **Do not duplicate comments on interface implementations.** Documentation belongs on the interface definition.
