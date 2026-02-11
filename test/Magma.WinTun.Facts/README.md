# Magma.WinTun.Facts

Test suite for the WinTun module.

## Running Tests

### Unit Tests

Unit tests run on all platforms (Windows and Linux) and do not require the WinTun driver:

```bash
dotnet test test/Magma.WinTun.Facts
```

### Integration Tests

Integration tests require:
- Windows operating system
- WinTun driver installed
- A configured TAP adapter

Integration tests are **skipped by default**. To enable them:

1. Set the environment variable:
   ```powershell
   $env:WINTUN_INTEGRATION_TESTS="1"
   ```

2. (Optional) Specify a custom adapter name:
   ```powershell
   $env:WINTUN_ADAPTER_NAME="YourAdapterName"
   ```

3. Run the tests:
   ```bash
   dotnet test test/Magma.WinTun.Facts
   ```

Or run only integration tests:
```bash
dotnet test test/Magma.WinTun.Facts --filter Category=Integration
```

## Test Categories

- **Unit Tests**: Test public APIs without requiring the WinTun driver
  - `WinIOFacts`: Constants and P/Invoke declarations
  - `WinTunTransportReceiverFacts`: Transport receiver interface
  - `WinTunMemoryPoolFacts`: Memory pool lifecycle and management

- **Integration Tests** (Category="Integration"): Require WinTun driver
  - `WinTunPortIntegrationFacts`: Adapter create/destroy lifecycle

## Platform Support

- **Linux**: Unit tests run and pass. Integration tests are skipped.
- **Windows**: All tests can run if WinTun driver is installed and integration tests are enabled.
