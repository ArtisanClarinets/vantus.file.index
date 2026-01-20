# PROMPT_WPF.md — Coding Agent Master Prompt

You are building the **Vantus File Indexer** desktop app’s **Settings experience** end-to-end based on the provided IA + defaults matrix. Deliver a production-grade **WPF (.NET 8+)** settings app that a Fortune-500 team could ship.

> Goal: Avoid Windows App SDK / WinUI 3 entirely for the **Settings UI**. Explorer integration internals remain Windows-native and can be stubbed behind interfaces.

## 0) High-level success criteria

The PR is “done” only when:

- App compiles and launches.
- Left navigation contains **every** IA page and routes correctly.
- Every page renders its settings using the **metadata registry** (not page-specific hardcoding).
- Presets apply correctly with a **diff preview** (grouped by page) before applying.
- Policy locks work and are visible in UI (lock icon, disabled control, reason).
- Import/export works with preview diff; reset restores defaults.
- Unit tests pass and cover core behaviors.
- Docs exist and match the app.

## 1) Context & constraints

Vantus File Indexer is a Windows app with:

- A background engine/service (separate process; settings UI is a client)
- Explorer integration toggles (columns/details/preview panel/context menu)
  - Explorer integration itself can be stubbed behind interfaces
  - Settings UX must be real and ready

Hard constraints:

- Do NOT run heavy logic inside Explorer. UI configures engine via local IPC (stub ok).
- “Enterprise” presets must support Managed Mode where settings can be policy-enforced (locked with reason).
- Keep pages fast: no blocking calls on UI thread.
- Do not ship demo-grade UI. Use modern Windows styling (Fluent-like) with consistent spacing, helper text, and section grouping.

## 2) Tech stack (required)

- **WPF on .NET 8+** (Windows-only)
- MVVM using `CommunityToolkit.Mvvm`
- DI using `Microsoft.Extensions.Hosting` / `Microsoft.Extensions.DependencyInjection`
- JSON persistence with `System.Text.Json`
- xUnit for tests

UI styling (choose one, repo-pinned):

- Option A: **WPF UI** (recommended: Fluent-like navigation + controls)
- Option B: **ModernWpf** (WinUI-like styles for WPF controls)

Do NOT introduce Windows App SDK packages (`Microsoft.WindowsAppSDK`) or WinUI 3 XAML.

## 3) Inputs / Source of truth

Treat these as canonical inputs (do not invent labels unless instructed):

- `/docs/settings-ia.md` — sitemap and page structure (provided)
- `/vantus_settings_ia_defaults.md` — exact control labels, helper text, types, allowed values, defaults per preset (provided)

If these are missing:
- Create placeholders with stable IDs and `TODO:` markers.
- Do not silently fabricate large control sets.

## 4) Information Architecture (must match 1:1)

Create these nav sections/pages exactly:

### General
- Appearance & Language
- Startup & Tray
- Modes & Presets
- Power & Performance
- Data Handling

### Workspaces
- Workspace Switcher
- Workspace Defaults
- Import & Export

### Locations
- Included Locations
- Exclusions
- Location Policy (Detail)

### Indexing
- Status
- Change Detection
- Performance
- Content Limits

### AI Models
- Runtime & Hardware
- Model Set
- Quality Controls

### Extraction
- Documents (PDF/Office/Text)
- Images
- Code
- Archives
- Media (Audio/Video)
- Email & Exports

### Organize & Automations
- Organizing Mode
- Rules
- Safety & Undo
- Review Queue

### Tags & Taxonomy
- Tagging Behavior
- Vocabulary & Synonyms
- Windows Metadata Fields

### Partners
- Partner Directory
- Matching Logic
- Partner Policies

### Search
- Search Mode
- Results & Previews
- Ranking & Facets

### Windows Integration
- Explorer Surfaces
- Context Menu Actions
- Windows Search Integration

### Privacy & Security
- Local Storage & Encryption
- Access Controls
- Sensitive Data Protections
- Keys & Rotation

### Compliance & Audit
- Audit Log
- Retention & Legal Hold
- Policy Reports

### Notifications
- Alerts & Banners
- Quiet Hours

### Storage & Maintenance
- Storage Locations
- Cache Controls
- Maintenance & Repair
- Backup & Restore

### Diagnostics
- System Status
- Performance Trace
- Export Diagnostics Bundle
- Advanced (Power User)

### Admin (Managed)
- Policy Overview
- Enforced Restrictions
- Deployment & Updates

### About
- Version & Licenses
- Reset

## 5) Required deliverables (single PR)

### 5.1 WPF Settings app shell
- Left navigation with all pages.
  - If using WPF UI: use its navigation control (recommended).
  - Otherwise: use a left-rail (ListBox/TreeView) + `Frame` navigation.
- Global “Search settings” box:
  - search labels + helper text
  - show results list
  - click result navigates to the page + highlights the control
- Consistent page template:
  - Title
  - 1-line intro sentence
  - Section headings
  - Controls with label + helper text
- “Modes & Presets” page:
  - Preset dropdown (Personal, Pro, Enterprise-Private, Enterprise-Automation)
  - “Preview changes” with diff grouped by page
  - “Apply preset”
  - “Revert to preset defaults”

### 5.2 Settings schema + typed model
Implement BOTH:
1) Strongly typed C# model(s) used by the app, and
2) A metadata registry file: `settings_definitions.json`

The metadata registry is the authoritative “UI + policy + defaulting” definition.

Each setting definition MUST include:
- `setting_id` (stable key, e.g. `privacy.encrypt_index_db`)
- `page` (matches IA page)
- `section` (string grouping inside page)
- `label`
- `helper_text`
- `control_type` one of:
  - `toggle | slider | dropdown | multi_select | button | status`
- `value_type` one of:
  - `bool | int | double | string | string_list | json`
- `allowed_values` (ranges/enums/lists as appropriate)
- `defaults` keyed by preset:
  - `personal | pro | enterprise_private | enterprise_automation`
- `scope`: `global | workspace | location`
- `requires_restart` (bool)
- `policy_lockable` (bool)
- `visibility`: `all | managed_only | power_user | hidden`
- `dangerous_action` (bool)

### 5.3 Metadata-driven UI rendering layer (must-have)
Do NOT hardcode controls per page in XAML only. Instead:
- Load `settings_definitions.json` into a `SettingsRegistry`.
- Build an ItemsControl-based renderer:
  - Use DataTemplates keyed by `control_type` to render the right control.
  - Bind value updates through a view-model layer that supports:
    - validation
    - policy lock state
    - restart-required badge
    - danger zone grouping

Mapping guidance:
- toggle → CheckBox/ToggleButton
- dropdown → ComboBox
- slider → Slider
- multi_select → ListBox with checkboxes
- button → Button
- status/info → TextBlock or InfoBar-style component (library-provided or custom)

### 5.4 Presets system
- Preset apply sets every setting to that preset’s defaults.
- “Preview changes” shows a diff before applying.
- Preset is not a straitjacket: users can override any unlocked setting afterward.
- Track `ActivePreset` and `IsCustomized`.

### 5.5 Managed Mode / policy enforcement
Implement policy locks:
- `locked_value`, `reason`, `source` (MDM/Intune/local policy file)

UI requirements:
- Locked controls disabled
- Lock icon + tooltip “Managed by your organization”
- Reason visible
- Link to Admin (Managed) → Enforced Restrictions

Provide `policies.json` example.

### 5.6 Persistence + migrations
- Save: `%LocalAppData%\Vantus\settings.json`
- Include `schema_version`
- Migration support:
  - carry-forward unknown keys
  - rename map for renamed keys
  - defaulting for new settings

### 5.7 Import / Export
- Export global/workspace settings to a single file.
- Import with validation + preview diff + Apply/Cancel.
- Never apply invalid settings silently.

### 5.8 Documentation
- `/docs/settings-ia.md`
- `/docs/presets.md`
- `/docs/policy-management.md`

### 5.9 Testing
Unit tests cover:
- preset application correctness
- policy lock enforcement
- import/export round-trip
- schema migration basics

### 5.10 IPC stub to engine
Define `IEngineClient` with stub calls:
- `GetIndexStatusAsync()`
- `PauseIndexingAsync()`
- `ResumeIndexingAsync()`
- `SetComputePreferenceAsync(...)`
- `RequestRebuildIndexAsync()`

No real service implementation required. UI depends only on interface (DI).

## 6) Repo structure (scaffold if missing)

- `Vantus.App` — WPF app
- `Vantus.Core` — schema/registry/store/presets/policy/import-export/IPC interfaces
- `Vantus.Tests` — xUnit tests
- `docs/`
- root: `settings_definitions.json`, `policies.json`

## 7) UX requirements (polish)

- Standard page template
- “Danger Zone” with warning + confirm dialogs
- Restart-required badge
- Global search navigates + highlights control

## 8) Execution plan (do not skip)

1) Scaffold solution if missing.
2) Implement registry loader + validation.
3) Implement store (load/save + schema_version + migrations).
4) Implement preset manager + diff preview model.
5) Implement policy engine + effective values + locked state.
6) Implement WPF renderer + page template.
7) Implement global search + highlight.
8) Implement import/export + preview.
9) Add tests + docs; run full build/test.

## 9) Execution Strategy

You are an autonomous agent. Use the `TodoWrite` tool to track your progress. Execute the following phases sequentially:

1) **Schema & Models**
   - Create `settings_definitions.json` schema, IDs, defaults per preset (from `vantus_settings_ia_defaults.md`).
   - Create typed C# models in `Vantus.Core`.

2) **WPF Shell**
   - Implement WPF settings shell with left nav, routing, page templates.
   - Implement settings search + highlight.

3) **Presets & Policy**
   - Implement `PresetManager` and diff preview.
   - Implement `PolicyEngine` with locked effective values and UI binding model.

4) **Persistence & I/O**
   - Implement persistence (load/save).
   - Implement Import/Export with diff preview.
   - Add core unit tests.

5) **Documentation**
   - Update `docs/settings-ia.md`, `docs/presets.md`, `docs/policy-management.md` to match implementation.

## 10) Final instruction

Ship it cleanly. Explorer integration internals or engine IPC may be stubbed behind interfaces, but the entire settings UX, registry, persistence, presets, policy locks, import/export, docs, and tests must be implemented.
