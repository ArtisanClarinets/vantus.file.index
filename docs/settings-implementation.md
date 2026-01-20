# Settings Implementation Guide

## Architecture
- **Vantus.Core**: Models, Persistence (JSON), Policy Engine.
- **Vantus.App**: WPF (.NET 8) UI using `WPF-UI` library.
- **Vantus.Tests**: xUnit tests.

## Metadata Registry
Settings are defined in `settings_definitions.json`.
UI renders controls dynamically based on `control_type`.

## Policy
Policies are loaded from `policies.json`. Locked settings are disabled in UI (logic in ViewModel).
