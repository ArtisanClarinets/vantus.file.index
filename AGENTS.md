# AGENTS_WPF.md — Vantus File Indexer Agent Technical Specification

**Target Audience:** AI Coding Agents & Developers
**Source of Truth:** Aligns with `PROMPT.MD`
**Related Guide:** [AGENT.md](./AGENT.md) (General Developer Guide)

This document provides the **comprehensive technical specifications** and **implementation rules** for the Vantus File Indexer Settings UI.

---

## 1. High-Level Success Criteria

The Settings UI implementation is considered complete only when:
1.  **App Compiles & Launches:** No build errors on .NET 8+.
2.  **Navigation:** Left navigation contains **every** IA page and routes correctly.
3.  **Metadata-Driven:** Every page renders settings using the `settings_definitions.json` registry (no hardcoded pages).
4.  **Presets:** Presets apply correctly with a **diff preview** (grouped by page) before applying.
5.  **Policy Locks:** Policy locks work and are visible in UI (lock icon, disabled control, reason).
6.  **Import/Export:** Works with preview diff; reset restores defaults.
7.  **Tests:** Unit tests pass and cover core behaviors (presets, policy, persistence).
8.  **Docs:** Documentation exists and matches the implementation.

---

## 2. Tech Stack & Constraints

### Required Stack
-   **Framework:** **WPF** on .NET 8+ (Windows-only).
-   **MVVM:** `CommunityToolkit.Mvvm`.
-   **DI:** `Microsoft.Extensions.Hosting` + `Microsoft.Extensions.DependencyInjection`.
-   **Serialization:** `System.Text.Json` (Strict usage).
-   **Testing:** xUnit.

### Styling (Repo-Pinned)
Use **one** of the following (do not mix):
-   **WPF UI** (Recommended): Fluent-like navigation + controls.
-   **ModernWpf**: WinUI-like styles for WPF controls.

### strict Constraints
-   **NO WinUI 3 / Windows App SDK:** Do not introduce `Microsoft.WindowsAppSDK` for the Settings UI.
-   **UI Thread Safety:** Never perform IO, JSON parsing, or "diff" computation synchronously on the UI thread. Use `Dispatcher.InvokeAsync`.
-   **Explorer Integration:** Internal Explorer logic stays native/C++; Settings UI only configures it via IPC.

---

## 3. Repo Layout

| Path | Purpose |
|------|---------|
| `Vantus.Core/Models/` | Settings models, schema types |
| `Vantus.Core/Services/` | Store, registry, preset manager, policy engine, import/export |
| `Vantus.Core/Engine/` | `IEngineClient` + IPC abstractions |
| `Vantus.App/` | **WPF** app shell, pages, UI services |
| `Vantus.App/Views/` | Page views (thin), bind to registry-driven VMs |
| `Vantus.App/Controls/` | Reusable settings UI components + templates |
| `Vantus.Tests/` | Unit tests |
| `docs/` | Documentation |
| `settings_definitions.json` | Settings registry metadata (Source of Truth) |
| `policies.json` | Example policy file |

---

## 4. Information Architecture (IA)

The application must implement the following structure exactly:

### General
-   Appearance & Language
-   Startup & Tray
-   Modes & Presets
-   Power & Performance
-   Data Handling

### Workspaces
-   Workspace Switcher
-   Workspace Defaults
-   Import & Export

### Locations
-   Included Locations
-   Exclusions
-   Location Policy (Detail)

### Indexing
-   Status
-   Change Detection
-   Performance
-   Content Limits

### AI Models
-   Runtime & Hardware
-   Model Set
-   Quality Controls

### Extraction
-   Documents (PDF/Office/Text)
-   Images
-   Code
-   Archives
-   Media (Audio/Video)
-   Email & Exports

### Organize & Automations
-   Organizing Mode
-   Rules
-   Safety & Undo
-   Review Queue

### Tags & Taxonomy
-   Tagging Behavior
-   Vocabulary & Synonyms
-   Windows Metadata Fields

### Partners
-   Partner Directory
-   Matching Logic
-   Partner Policies

### Search
-   Search Mode
-   Results & Previews
-   Ranking & Facets

### Windows Integration
-   Explorer Surfaces
-   Context Menu Actions
-   Windows Search Integration

### Privacy & Security
-   Local Storage & Encryption
-   Access Controls
-   Sensitive Data Protections
-   Keys & Rotation

### Compliance & Audit
-   Audit Log
-   Retention & Legal Hold
-   Policy Reports

### Notifications
-   Alerts & Banners
-   Quiet Hours

### Storage & Maintenance
-   Storage Locations
-   Cache Controls
-   Maintenance & Repair
-   Backup & Restore

### Diagnostics
-   System Status
-   Performance Trace
-   Export Diagnostics Bundle
-   Advanced (Power User)

### Admin (Managed)
-   Policy Overview
-   Enforced Restrictions
-   Deployment & Updates

### About
-   Version & Licenses
-   Reset

---

## 5. Implementation Specifications

### 5.1 Settings Registry (`settings_definitions.json`)
The registry is the authoritative definition for UI, policy, and defaulting.
-   **Fields:** `setting_id`, `page`, `section`, `label`, `helper_text`, `control_type`, `value_type`, `allowed_values`, `defaults` (per preset), `scope`, `requires_restart`, `policy_lockable`, `visibility`, `dangerous_action`.
-   **Control Types:** `toggle`, `slider`, `dropdown`, `multi_select`, `button`, `status`.

### 5.2 Metadata-Driven Renderer
-   **Do not hardcode controls.**
-   Load `settings_definitions.json` into a `SettingsRegistry`.
-   Use `ItemsControl` / `ListView` with `DataTemplate` keyed by `control_type`.
-   Bind to a `SettingItemViewModel` handling validation, lock state, and restart badges.

### 5.3 Presets System
-   **Apply:** Sets every setting to the preset's defaults.
-   **Preview:** Shows a diff grouped by page before applying.
-   **Tracking:** Track `ActivePreset` and `IsCustomized`.

### 5.4 Policy Enforcement (Managed Mode)
-   **Resolution:** 1) Policy Lock -> 2) User Value -> 3) Preset Default -> 4) Schema Fallback.
-   **UI:** Locked controls must be disabled, show a lock icon, and display the `LockReason`.

### 5.5 Persistence
-   File: `%LocalAppData%\Vantus\settings.json`
-   Must include `schema_version`.
-   **Migrations:** Carry forward unknown keys; implement rename maps.

### 5.6 Import / Export
-   Export global/workspace settings.
-   Import with validation and **preview diff**.

### 5.7 Engine IPC
-   Define `IEngineClient` in `Vantus.Core`.
-   Implement `StubEngineClient` for local development.
-   UI must only depend on the interface (DI).

---

## 6. Build & Test Commands

```powershell
# Build
dotnet build Vantus.FileIndexer.sln

# Test
dotnet test Vantus.Tests/Vantus.Tests.csproj

# Run App
dotnet run --project Vantus.App/Vantus.App.csproj

# Clean Rebuild
dotnet clean
Remove-Item -Recurse -Force obj, bin -ErrorAction SilentlyContinue
dotnet restore
dotnet build Vantus.FileIndexer.sln
```

## 7. Quality Standards

-   **Code Style:** PascalCase public, _camelCase private, Async suffix.
-   **Nullability:** Enabled (`<Nullable>enable</Nullable>`).
-   **Error Handling:** Surface recoverable issues to UI; never swallow exceptions silently.
-   **Documentation:** Maintain docs in `docs/` matching the code.
