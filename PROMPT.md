# PROMPT.md — Coding Agent Master Prompt (paste into your agent)

You are building the **Vantus File Indexer** desktop app’s **Settings experience** end-to-end based on the provided IA + defaults matrix. Deliver a production-grade **WinUI 3** settings shell that a Fortune-500 team could ship.

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
  - The **Explorer integration itself can be stubbed** behind interfaces
  - But the **settings UX must be real** and ready

Hard constraints:

- **Do NOT** run heavy logic inside Explorer. UI configures engine via local IPC (stub ok).
- “Enterprise” presets must support **Managed Mode** where settings can be **policy-enforced** (locked with reason).
- Keep pages fast: **no blocking calls on UI thread**.
- Do not ship “demo-grade” UI. Match Fluent/WinUI style conventions for spacing, typography, helper text, and sectioning.

## 2) Tech stack (required)

- **WinUI 3 (Windows App SDK)** in C#
- MVVM using `CommunityToolkit.Mvvm`
- DI using `Microsoft.Extensions.Hosting` / `Microsoft.Extensions.DependencyInjection`
- JSON persistence with `System.Text.Json`
- xUnit for tests

UI settings layout guidance:

- Use Windows 11-style “settings cards/expanders” patterns via `CommunityToolkit.WinUI.Controls.SettingsControls` (SettingsCard + SettingsExpander) where appropriate.
- Use `NavigationView` left-rail for the sitemap and routing.

## 3) Inputs / Source of truth

You must treat these as canonical inputs (do not invent labels unless instructed):

- `/docs/settings-ia.md` — sitemap and page structure (provided)
- `/docs/settings-defaults-matrix.md` (or equivalent) — exact control labels, helper text, types, allowed values, defaults per preset (provided)

If the repo does not contain these files yet:
- Create them as placeholders **with stable IDs**, and mark missing fields with `TODO:` so the project can be completed without losing structure.
- Do **not** silently fabricate large control sets; keep placeholders explicit.

## 4) Information Architecture (must match 1:1)

Create these NavigationView sections/pages exactly:

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

### 5.1 WinUI Settings app shell
- NavigationView left-rail with all pages.
- Global “Search settings” box (search labels + helper text; show results list; clicking result navigates + highlights control).
- Consistent page template:
  - Title
  - 1-line intro sentence
  - Section headings
  - Settings controls with label + helper text
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

#### `settings_definitions.json` (minimum shape)
Each setting definition MUST include:

- `setting_id` (stable string key, e.g. `privacy.encrypt_index_db`)
- `page` (matches IA page)
- `section` (string grouping inside page)
- `label`
- `helper_text`
- `control_type` one of:
  - `toggle | slider | dropdown | multi_select | button | status`
- `value_type` one of:
  - `bool | int | double | string | string_list | json`
- `allowed_values`:
  - ranges for slider (min/max/step)
  - enum list for dropdown
  - selectable list for multi_select
- `defaults` object keyed by preset:
  - `personal | pro | enterprise_private | enterprise_automation`
- `scope`:
  - `global | workspace | location`
- `requires_restart` (bool)
- `policy_lockable` (bool)
- `visibility`:
  - e.g. `all`, `managed_only`, `power_user`, `hidden`
- `dangerous_action` (bool) — for “Danger Zone” rendering

### 5.3 Metadata-driven rendering layer (must-have)
Do NOT hardcode controls only via XAML per page. Instead:

- Build a `SettingsRegistry` loader that reads `settings_definitions.json`
- Build a small rendering layer that maps `control_type` to WinUI controls:
  - toggle → `ToggleSwitch` inside SettingsCard
  - dropdown → `ComboBox`
  - slider → `Slider`
  - multi_select → `ListView` + checkboxes
  - button → `Button`
  - status/info → `InfoBar` or read-only `TextBlock`
- The page views should be “thin”:
  - filter registry by `page`
  - group by `section`
  - render via the renderer

### 5.4 Presets system
- Preset apply = set every setting to that preset’s default.
- “Preview changes” shows a diff before applying.
- Preset is not a straitjacket: users can override any unlocked setting afterward.
- Track effective state:
  - e.g. `ActivePreset` + `IsCustomized` flag if user has deviated from preset defaults.

### 5.5 Managed Mode / policy enforcement
Implement a policy layer that can lock any `policy_lockable` setting.
- Locks must include: `locked_value`, `reason`, `source` (MDM/Intune/local policy file).

UI requirements:
- Locked controls are disabled
- Show lock icon + tooltip: “Managed by your organization”
- Show reason text and a “View enforced policies” link to Admin (Managed) → Enforced Restrictions

Provide a repo example: `policies.json`.

Example format:
```json
{
  "managed": true,
  "locks": [
    {
      "setting_id": "privacy.encrypt_index_db",
      "locked_value": true,
      "reason": "Encryption is required by organization policy.",
      "source": "MDM"
    }
  ]
}
```

Rules:
- Policy overrides local value at runtime (effective value).
- Users cannot change locked settings.
- Admin (Managed) section is visible only when `managed: true`.

### 5.6 Persistence + migrations
- Save settings locally: `%LocalAppData%\Vantus\settings.json`
- Include `schema_version` in file.
- Provide migration support for version bumps:
  - at minimum: carry-forward unknown keys, migrate renamed keys (map table), and set defaults for newly introduced settings.

### 5.7 Import / Export
- Export global/workspace settings to a single file.
- Import with:
  - validation
  - preview diff
  - Apply/Cancel
- Never apply invalid settings silently.

### 5.8 Documentation
Add these docs:
- `/docs/settings-ia.md` — sitemap + complete controls list (table form recommended)
- `/docs/presets.md` — describe each preset and tradeoffs
- `/docs/policy-management.md` — policy locks + JSON format + examples

### 5.9 Testing
Unit tests must cover:
- Preset application correctness
- Policy lock enforcement (effective values + inability to set locked values)
- Import/export round-trip
- Schema migration basics (at least one version bump scenario)

### 5.10 IPC stub to engine
Define `IEngineClient` (in Core) with stub calls, e.g.:
- `GetIndexStatusAsync()`
- `PauseIndexingAsync()`
- `ResumeIndexingAsync()`
- `SetComputePreferenceAsync(...)`
- `RequestRebuildIndexAsync()`

Do not implement real background service unless trivial. Architect the UI to call the interface (DI-resolved).

## 6) Repo structure (scaffold if missing)

Create:
- `Vantus.App` — WinUI 3 app (NavigationView + pages)
- `Vantus.Core` — settings model, registry, store, presets, policy, import/export, IPC interfaces
- `Vantus.Tests` — xUnit tests
- `docs/` — markdown docs
- root files:
  - `settings_definitions.json`
  - `policies.json` (example)

## 7) UX requirements (polish)

- Each page uses the standard template (Title + intro + sections).
- Dangerous actions appear in a `Danger Zone` with a red warning InfoBar + confirm dialogs.
- Show “Restart required” tag for settings with `requires_restart = true`.
- Global search:
  - result click navigates to page + scrolls to control + highlights it briefly.

## 8) Execution plan (do not skip)

1) Inspect repo; scaffold solution if missing.
2) Implement `settings_definitions.json` + registry loader.
3) Implement `SettingsStore` (load/save + schema_version + migration).
4) Implement preset manager + diff preview model.
5) Implement policy engine + effective values + locked state.
6) Implement metadata renderer + page template.
7) Implement global search + highlight navigation.
8) Implement import/export + preview.
9) Add tests + docs; run full build/test.

## 9) Multi-agent coordination (opencode)

You may spawn sub-agents via CLI command `opencode`.

Spawn these immediately:

1) Schema Agent
```bash
opencode agent create --name settings-schema --task "Create settings_definitions.json schema, IDs, defaults per preset, and docs tables placeholders (no invented labels if missing source)"
```

2) WinUI Shell Agent
```bash
opencode agent create --name winui-shell --task "Implement WinUI 3 NavigationView shell, routing, page templates, settings search + highlight"
```

3) Presets & Policy Agent
```bash
opencode agent create --name presets-policy --task "Implement PresetManager, diff preview, PolicyEngine with locked effective values and UI binding model"
```

4) Import/Export + Tests Agent
```bash
opencode agent create --name io-tests --task "Implement persistence, import/export with diff preview, migration scaffolding, and core unit tests"
```

5) Docs Agent
```bash
opencode agent create --name docs --task "Write docs/settings-ia.md, docs/presets.md, docs/policy-management.md from registry + IA"
```

Main agent responsibilities:
- Integrate all sub-agent work
- Enforce naming consistency (IDs, page names, preset keys)
- Resolve conflicts, run build/test
- Ensure app runs and renders all pages

## 10) Final instruction

Ship it cleanly. If Explorer integration internals or engine IPC are stubbed, they must be stubbed behind interfaces with clear TODOs, but **the entire settings UX, metadata registry, persistence, presets, policy locks, import/export, docs, and tests must be implemented**.
