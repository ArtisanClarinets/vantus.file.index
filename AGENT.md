# AGENT.md — Vantus File Indexer Development Guide (WPF)

This document provides guidelines, conventions, and commands for AI coding agents working on the **Vantus File Indexer** repo.

> **Important:** The Settings UI must be built with **WPF (.NET 8+)**. Do **not** use **Windows App SDK / WinUI 3** for the Settings application.

---

## 1) Project overview

Vantus File Indexer is a Windows desktop app with a dedicated Settings experience and a separate background engine/service. The solution consists of:

- **Vantus.Core** — settings models, schema/registry, persistence, presets, policy enforcement, import/export, IPC interfaces
- **Vantus.App** — **WPF desktop application** (left-nav shell, settings pages rendered from metadata)
- **Vantus.Tests** — xUnit unit tests

Core principles:
- Settings are **metadata-driven** from `settings_definitions.json`
- UI pages do **not** hardcode control lists; they render from registry filtered by IA page
- Policy locks override user values (effective state)
- No heavy logic on the UI thread (especially IO and diff computation)

Explorer integration note (high-level):
- Explorer in-proc extensions should **not** be written in managed code. Keep any Explorer in-proc work native/C++ and keep it “thin”. Settings UI controls these features via IPC to the engine.

---

## 2) Tech stack

- .NET 8+
- **WPF**
- `CommunityToolkit.Mvvm` (MVVM Toolkit)
- `Microsoft.Extensions.Hosting` + `Microsoft.Extensions.DependencyInjection` (Generic Host / DI)
- `System.Text.Json`
- xUnit (repo-pinned major version; do not upgrade without explicit instruction)

### UI styling (choose one; pin in repo)
Use **one** of these for a modern Fluent-like look:
- **WPF UI (lepoco/wpfui)** — fluent navigation + modern controls
- **ModernWpf (Kinnara/ModernWpf)** — WinUI-like styles/controls for WPF

If the repo already pins versions, **do not upgrade** unless asked.

---

## 3) Build, run, and test commands

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

### Run the application (WPF)
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

---

## 4) Repo layout

| Path | Purpose |
|------|---------|
| `Vantus.Core/Models/` | Settings models, schema types |
| `Vantus.Core/Services/` | Store, registry, preset manager, policy engine, import/export |
| `Vantus.Core/Engine/` | `IEngineClient` + IPC abstractions |
| `Vantus.App/` | **WPF** app shell, views, UI services |
| `Vantus.App/Views/` | Page views (thin), bind to registry-driven view models |
| `Vantus.App/Controls/` | Reusable settings UI components + templates |
| `Vantus.Tests/` | xUnit tests |
| `docs/` | `/docs/settings-ia.md`, `/docs/presets.md`, `/docs/policy-management.md` |
| `settings_definitions.json` | Settings schema metadata |
| `policies.json` | Example policy file |

---

## 5) Agent workflow rules

### Branching & PR hygiene
- Work on a feature branch: `feature/<topic>`
- Keep PRs focused; avoid drive-by refactors
- Update docs/tests for any schema or behavioral change

### “Source of truth” rule
- UI and defaults come from `settings_definitions.json`
- Do not duplicate labels/helper text in multiple places
- Any UI label shown to user must be traceable to registry metadata

### Do not block the UI thread (WPF)
- Never do file IO, JSON parsing, or “diff” computation synchronously on the UI thread
- Use async methods and marshal updates back to UI via:
  - `Application.Current.Dispatcher.InvokeAsync(...)` / `Dispatcher.BeginInvoke(...)`
- Prefer background services for long-running work; keep UI responsive.

---

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
- Surface recoverable issues to UI (banner/infobar/snackbar depending on UI library) and return failure results
- Never swallow exceptions silently

---

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

---

## 8) Settings model & registry patterns

### Registry loader
- Load `settings_definitions.json` at startup (async)
- Validate required fields (`setting_id`, `label`, `helper_text`, `defaults`, etc.)
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

---

## 9) UI conventions (WPF)

### Navigation
- Implement left-rail navigation matching the IA 1:1
  - If using **WPF UI**, use its Navigation framework
  - Otherwise: left ListBox/TreeView + Frame navigation
- Route to pages by IA key and persist last visited page

### Settings layout
- Use a consistent “Settings page template”:
  - Title
  - one-line intro
  - sections with headings
  - controls with label + helper text
- Render all controls from `settings_definitions.json` via templates:
  - Use DataTemplates keyed by `control_type`
- “Danger Zone” section:
  - warning panel + confirmation dialogs for destructive actions
- Display “Restart required” tags/badges for `requires_restart = true`

### Search
- Global search box searches labels + helper text
- Clicking a result navigates to the page and highlights the matching control (brief visual emphasis + scroll into view)

---

## 10) Testing guidelines

- One behavior per test
- Use temp directories for persistence tests
- Avoid timing-sensitive tests
- Cover at minimum:
  - preset application sets all defaults correctly
  - policy locks override and block changes
  - import/export roundtrip stable
  - migration runs and produces expected shape

---

## 11) Common operations

### Add a new setting
1) Add definition to `settings_definitions.json`
2) Add defaults for all presets
3) Ensure value type + allowed values are correct
4) Add/extend UI template mapping if new control type
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

---

## 12) Engine IPC stub conventions

- `IEngineClient` lives in `Vantus.Core`
- Provide a `StubEngineClient` implementation used for local dev
- UI must only depend on the interface (DI-resolved)

---

## Appendix: Reference materials (for agents)

- WPF on modern .NET (Microsoft Learn): WPF runs on .NET and is Windows-only.
- MVVM Toolkit (CommunityToolkit.Mvvm) docs (Microsoft Learn): UI-framework-agnostic MVVM helpers.
- .NET Generic Host docs (Microsoft Learn): patterns for HostApplicationBuilder and DI.
- WPF Dispatcher + threading model docs (Microsoft Learn): `Dispatcher.InvokeAsync/BeginInvoke` for UI marshaling.
- WPF UI and ModernWpf repositories for Fluent-style WPF UI building.
- Windows Shell guidance: avoid managed in-proc shell extensions.
