# Integration Guide

This guide helps you choose and implement the right platform integration for your Magma-based application.

## Platform Integration Overview

Magma supports multiple platform-specific integrations for high-performance packet I/O:

| Platform | Integration | Status | Kernel Requirement | Performance |
|----------|------------|--------|-------------------|-------------|
| Linux | AF_XDP | **Recommended** | 4.18+ | Excellent |
| Linux | NetMap | Legacy | Any (module) | Excellent |
| Linux | DPDK | Reference | Any | Maximum |
| Windows | WinTun | **Recommended** | Win 7+ | Excellent |

## Decision Matrix

### Choose AF_XDP if:
- ✅ Running on modern Linux (kernel 4.18+)
- ✅ Want kernel-native integration
- ✅ Need security and stability of mainline kernel
- ✅ Prefer zero-copy packet processing
- ✅ Want to avoid out-of-tree kernel modules

### Choose NetMap if:
- ✅ Running on older Linux systems (< 4.18)
- ✅ Already have NetMap infrastructure
- ✅ Need specific NetMap features
- ✅ Comfortable managing kernel modules

### Choose WinTun if:
- ✅ Running on Windows (any version from Win 7)
- ✅ Building VPN or tunnel applications
- ✅ Need virtual network adapters
- ✅ Want modern, maintained driver

## References

- [Magma.AF_XDP README](../src/Magma.AF_XDP/README.md)
- [Magma.WinTun README](../src/Magma.WinTun/README.md)
- [AF_XDP Kernel Documentation](https://www.kernel.org/doc/html/latest/networking/af_xdp.html)
- [WinTun Official Site](https://www.wintun.net/)
