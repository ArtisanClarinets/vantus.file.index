# AGENTS.md — Vantus File Indexer Development Guide

This document provides guidelines, conventions, and commands for AI coding agents working on the **Vantus File Indexer** repo.

## 1) Project overview

Vantus File Indexer is a Windows desktop app with a WinUI 3 settings experience. The solution consists of:

- **Vantus.Core** — settings models, schema/registry, persistence, presets, policy enforcement, import/export, IPC interfaces
- **Vantus.App** — WinUI 3 desktop application (NavigationView shell, settings pages rendered from metadata)
- **Vantus.Tests** — xUnit unit tests

Core principles:
- Settings are **metadata-driven** from `settings_definitions.json`
- UI pages do not hardcode control lists; they render from registry filtered by IA page
- Policy locks override user values (effective state)

## 2) Tech stack

- .NET 8+
- WinUI 3 (Windows App SDK 1.8.x recommended)
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Hosting / DI
- System.Text.Json
- xUnit (project pinned to the chosen major version; do not upgrade without explicit instruction)

### Recommended package baseline (as of Jan 2026)

- `Microsoft.WindowsAppSDK`: **1.8.4 (1.8.260101001)** or the repo-pinned 1.8.x version
- `CommunityToolkit.Mvvm`: **8.4.0** (or repo-pinned)
- `CommunityToolkit.WinUI.Controls.SettingsControls`: latest stable compatible with WinUI 3
- Windows SDK: use a supported SDK installed locally; keep pinned versions consistent across projects

If the repo already pins versions, **do not upgrade** unless asked.

## 3) Build & test commands

### Full solution build
```powershell
dotnet build Vantus.FileIndexer.sln
```

### Build with warnings as errors
```powershell
dotnet build Vantus.FileIndexer.sln /p:TreatWarningsAsErrors=true
```

### Clean and rebuild
```powershell
dotnet clean
Remove-Item -Recurse -Force obj, bin -ErrorAction SilentlyContinue
dotnet restore
dotnet build Vantus.FileIndexer.sln
```

### Run the application
```powershell
dotnet run --project Vantus.App/Vantus.App.csproj
```

### Run all tests
```powershell
dotnet test Vantus.Tests/Vantus.Tests.csproj
```

### Run a single test class
```powershell
dotnet test Vantus.Tests/Vantus.Tests.csproj --filter "FullyQualifiedName~PresetManagerTests"
```

### Run a single test method
```powershell
dotnet test Vantus.Tests/Vantus.Tests.csproj --filter "FullyQualifiedName~PresetManagerTests.GetAvailablePresets_ReturnsAllFourPresets"
```

### Coverage (optional)
```powershell
dotnet test Vantus.Tests/Vantus.Tests.csproj --collect:"XPlat Code Coverage"
```

## 4) Repo layout

| Path | Purpose |
|------|---------|
| `Vantus.Core/Models/` | Settings models, schema types |
| `Vantus.Core/Services/` | Store, registry, preset manager, policy engine, import/export |
| `Vantus.Core/Engine/` | `IEngineClient` + IPC abstractions |
| `Vantus.App/` | WinUI app shell, pages, UI services |
| `Vantus.App/Pages/` | Page views (thin), bind to registry-driven view models |
| `Vantus.App/Controls/` | Reusable settings UI components |
| `Vantus.Tests/` | xUnit tests |
| `docs/` | `/docs/settings-ia.md`, `/docs/presets.md`, `/docs/policy-management.md` |
| `settings_definitions.json` | Settings registry metadata |
| `policies.json` | Example policy file |

## 5) Agent workflow rules

### Branching & PR hygiene
- Work on a feature branch: `feature/<topic>`
- Keep PRs focused; avoid drive-by refactors
- Update docs/tests for any schema or behavioral change

### “Source of truth” rule
- UI and defaults come from `settings_definitions.json`
- Do not duplicate labels/helper text in multiple places
- Any UI label shown to user must be traceable to registry metadata

### Do not block the UI thread
- Never do file IO, JSON parsing, or “diff” computation synchronously on UI thread
- Use async methods and marshal back to UI via `DispatcherQueue.TryEnqueue` when needed

## 6) Code style

### Imports
- Put `using` statements at top, alphabetized
- Prefer implicit usings (enabled) and nullable reference types

### Naming
- PascalCase for public types/members
- `_camelCase` for private fields
- `Async` suffix for async methods

### Nullability
- `<Nullable>enable</Nullable>`
- Use `?` and guard clauses; prefer `ArgumentNullException.ThrowIfNull`

### Error handling
- Catch specific exceptions where possible
- Surface recoverable issues to UI (InfoBar) and return failure results
- Never swallow exceptions silently

## 7) JSON & persistence conventions

### Serializer options
Use `System.Text.Json` with explicit options (shared singleton), e.g.:
- `WriteIndented = true`
- `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
- Consider enum string serialization if needed

### Storage location
Persist user settings under LocalAppData, e.g.:
- `%LocalAppData%\Vantus\settings.json`

### Migration
- Always include `schema_version`
- Keep a migration map for renamed keys
- Preserve unknown keys (forward-compat)

## 8) Settings model & registry patterns

### Registry loader
- Load `settings_definitions.json` at startup (async)
- Validate required fields (setting_id, label, helper_text, defaults, etc.)
- Build indexes:
  - by `page`
  - by `section`
  - by `setting_id`
  - by search tokens (label + helper text)

### Effective setting value
Effective value resolution order:
1) Policy lock (if present)
2) User value (persisted)
3) Preset default
4) Schema default (fallback)

Expose for UI:
- `Value`
- `IsLocked`
- `LockReason`
- `RequiresRestart`
- `IsDangerousAction`

## 9) UI conventions (WinUI)

### Navigation
- Use `NavigationView` left rail
- Route to pages by IA key
- Persist last visited page

### Settings layout
- Use SettingsCard / SettingsExpander controls for Windows 11-style settings where possible
- Consistent spacing, typography, and helper text
- “Danger Zone” section with warning InfoBar + confirm dialogs

### Search
- Global search box searches labels + helper text
- Clicking result navigates to the page and highlights the matching control

## 10) Testing guidelines

- One behavior per test
- Use temp directories for persistence tests
- Avoid timing-sensitive tests
- Cover at minimum:
  - preset application sets all defaults correctly
  - policy locks override and block changes
  - import/export roundtrip stable
  - migration runs and produces expected shape

## 11) Common operations

### Add a new setting
1) Add definition to `settings_definitions.json`
2) Add defaults for all presets
3) Ensure value type + allowed values are correct
4) Add/extend UI renderer mapping if new control type
5) Add unit test(s)
6) Update docs tables

### Add a new preset
1) Add preset key definition (if modeled)
2) Add defaults for all settings for that preset
3) Add tests for preset diff + apply
4) Update `/docs/presets.md`

### Add policy lock support
1) Ensure `policy_lockable: true`
2) Add sample lock entry to `policies.json`
3) Verify UI shows locked state + reason
4) Add unit tests for lock precedence

## 12) Engine IPC stub conventions

- `IEngineClient` lives in `Vantus.Core`
- Provide a `StubEngineClient` implementation used for local dev
- UI must only depend on the interface (DI-resolved)
