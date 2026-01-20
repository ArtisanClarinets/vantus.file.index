# AGENTS_WPF.md — Vantus File Indexer Development Guide (WPF stack)

This guide is for AI coding agents working on the Vantus File Indexer repo using **WPF (.NET 8+)** for the Settings UI.

## 1) Project overview

- **Vantus.Core** — settings models, schema/registry, persistence, presets, policy enforcement, import/export, IPC interfaces
- **Vantus.App** — WPF desktop application (navigation shell + metadata-driven settings pages)
- **Vantus.Tests** — xUnit unit tests

Core principles:
- Settings are metadata-driven from `settings_definitions.json`
- Pages do not hardcode control lists; they render from registry filtered by IA page
- Policy locks override user values (effective state)

## 2) Tech stack

- .NET 8+
- WPF
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Hosting / DI
- System.Text.Json
- xUnit

Styling (repo-pinned):
- Prefer **WPF UI** or **ModernWpf** for Fluent-like visuals.

Do NOT introduce Windows App SDK / WinUI 3 packages.

## 3) Build & test commands

```powershell
dotnet build Vantus.FileIndexer.sln
dotnet test Vantus.Tests/Vantus.Tests.csproj
dotnet run --project Vantus.App/Vantus.App.csproj
```

Clean rebuild:
```powershell
dotnet clean
Remove-Item -Recurse -Force obj, bin -ErrorAction SilentlyContinue
dotnet restore
dotnet build Vantus.FileIndexer.sln
```

## 4) Repo layout

| Path | Purpose |
|------|---------|
| `Vantus.Core/Models/` | Settings models, schema types |
| `Vantus.Core/Services/` | Store, registry, preset manager, policy engine, import/export |
| `Vantus.Core/Engine/` | `IEngineClient` + IPC abstractions |
| `Vantus.App/` | WPF app shell, pages, UI services |
| `Vantus.App/Views/` | Page views (thin), bind to registry-driven VMs |
| `Vantus.App/Controls/` | Reusable settings UI components + templates |
| `Vantus.Tests/` | Unit tests |
| `docs/` | Docs |
| `settings_definitions.json` | Settings registry metadata |
| `policies.json` | Example policy file |

## 5) UI thread safety (WPF)

- Do not do IO or heavy compute on the UI thread.
- Use async and marshal back via:
  - `Application.Current.Dispatcher.InvokeAsync(...)` or `Dispatcher.BeginInvoke(...)`

## 6) JSON & persistence conventions

- `System.Text.Json` only
- Shared serializer options
- Persist to `%LocalAppData%\Vantus\settings.json`
- Always include `schema_version`
- Preserve unknown keys

## 7) Effective value resolution

1) Policy lock
2) User value
3) Preset default
4) Schema fallback

Expose to UI:
- `Value`
- `IsLocked`
- `LockReason`
- `RequiresRestart`
- `IsDangerousAction`

## 8) Settings renderer pattern (WPF)

- Use ItemsControl/ListView to render settings definitions.
- Use DataTemplates keyed by `control_type`.
- Bind to a `SettingItemViewModel` that handles:
  - getting/setting values
  - validation
  - lock state
  - restart required badge

## 9) Testing guidelines

- Use temp directories for persistence tests
- Avoid timing-sensitive tests
- Cover:
  - preset apply sets all defaults
  - policy locks override and block changes
  - import/export roundtrip stable
  - migration example

## 10) Engine IPC stub conventions

- `IEngineClient` in `Vantus.Core`
- Provide `StubEngineClient` for local dev
- UI depends only on interface (DI)
