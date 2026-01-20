# Vantus Settings Information Architecture

This document defines the complete settings sitemap and control specifications for the Vantus File Indexer.

## Navigation Structure

The settings are organized into the following categories and pages:

### General
- **Appearance & Language** - Theme, accent color, app language, reduce motion, increase contrast
- **Startup & Tray** - Launch at sign-in, minimize to tray, tray badge, tray quick actions
- **Modes & Presets** - Preset selection, performance profile, private mode, automation guardrails
- **Power & Performance** - Pause on battery, battery threshold, pause on low disk, disk threshold, idle-only heavy tasks
- **Data Handling** - On-device status, anonymous analytics, crash reports

### Workspaces
- **Workspace Switcher** - Active workspace selection, lock workspace switching
- **Workspace Defaults** - Default tag namespace, default organizing mode, default preview behavior
- **Import & Export** - Export/import workspace settings

### Locations
- **Included Locations** - Add folder/drive/share, index enabled, allow organizing, treat as sensitive
- **Exclusions** - Excluded folders, excluded patterns, excluded file types
- **Location Policy (Detail)** - Index depth, auto-tagging, partner matching, organizing mode, heavy tasks schedule, max file size

### Indexing
- **Status** - Enable indexing, reindex now, rebuild index
- **Change Detection** - Change detection mode, full sweep frequency, external drives behavior
- **Performance** - CPU usage limit, disk IO limit, parallel extractors, throttle while active
- **Content Limits** - Max text per file, max PDF pages to OCR, max media minutes to transcribe

### AI Models
- **Runtime & Hardware** - Compute device preference, fallback order, low power inference
- **Model Set** - Embedding model, classifier model, explanation model
- **Quality Controls** - Auto-tag threshold, auto-partner threshold, auto-organize threshold, require approval below thresholds, deterministic results

### Extraction
- **Documents (PDF/Office/Text)** - Extract text, store snippets, Office metadata, PDF processing mode, OCR scanned PDFs, OCR languages
- **Images** - Read EXIF, OCR images, visual tagging
- **Code** - Code-aware indexing, respect .gitignore, index dependencies, extract symbols
- **Archives** - Index manifest, unpack small archives, small archive size limit, password archives behavior
- **Media (Audio/Video)** - Transcription, store transcript text, speaker separation, max media minutes
- **Email & Exports** - Enable email parsing, index email bodies, index attachments

### Organize & Automations
- **Organizing Mode** - Organizing mode, show suggestions in Explorer, one-click apply
- **Rules** - Enable rules engine, suggest rules from approvals, require approval for new rules, rule priority mode
- **Safety & Undo** - Never move open files, copy-verify-delete, cloud sync behavior, collision handling, undo retention window
- **Review Queue** - Enable review queue, notify threshold, batch approvals

### Tags & Taxonomy
- **Tagging Behavior** - Auto-tag files, suggest tags while browsing, max tags per file
- **Vocabulary & Synonyms** - Tag vocabulary mode, synonyms editor
- **Windows Metadata Fields** - Expose AI Tags, expose AI Partner, expose AI Explanation, tag formatting

### Partners
- **Partner Directory** - Enable directory, partner types enabled, default destination per partner
- **Matching Logic** - Match by domain, match by filename, match by content, require approval for assignment, partner confidence threshold
- **Partner Policies** - Per-partner exclusions, pin partner to folder

### Search
- **Search Mode** - Default search mode, enable "Find similar"
- **Results & Previews** - Show content snippets, show thumbnails, require Hello for previews
- **Ranking & Facets** - Recency boost, filename exact-match boost, enabled facets

### Windows Integration
- **Explorer Surfaces** - Enable Explorer columns, enable Details pane, enable preview panel
- **Context Menu Actions** - Context menu actions, modern menu
- **Windows Search Integration** - Register AI properties, allow Windows Search to index extracted text, rebuild property cache

### Privacy & Security
- **Local Storage & Encryption** - Encrypt index DB, encrypt text cache, embeddings-only mode, store previews/thumbnails
- **Access Controls** - Require sign-in to open, require Hello to edit rules, require Hello to view previews
- **Sensitive Data Protections** - Redact sensitive in logs, disable indexing in sensitive locations
- **Keys & Rotation** - Key storage, rotate encryption keys

### Compliance & Audit
- **Audit Log** - Enable audit logging, audit detail level, export audit log
- **Retention & Legal Hold** - Legal hold mode, audit log retention
- **Policy Reports** - Generate index scope report, generate security posture report

### Notifications
- **Alerts & Banners** - Indexing completed, suggestions ready, errors requiring attention
- **Quiet Hours** - Enable quiet hours, quiet hours schedule

### Storage & Maintenance
- **Storage Locations** - Index storage location, move index to another drive
- **Cache Controls** - Text cache size limit, preview/thumbnail cache size limit
- **Maintenance & Repair** - Optimize DB weekly, integrity check monthly, clear caches
- **Backup & Restore** - Backup index, restore index

### Diagnostics
- **System Status** - Show device usage, show model versions, test extraction
- **Performance Trace** - Start trace, include paths in trace
- **Export Diagnostics Bundle** - Export bundle, redact content snippets
- **Advanced (Power User)** - Show raw AI JSON, experimental features

### Admin (Managed)
- **Policy Overview** - Managed status, view enforced policies
- **Enforced Restrictions** - Policy-defined list of locked settings
- **Deployment & Updates** - Update channel, update deferral window, export policy file

### About
- **Version & Licenses** - Version, build number, open-source licenses, privacy statement
- **Reset** - Reset to defaults

## Setting ID Conventions

Settings use dot-notation with scope suffixes:
- `general.*` - Global settings
- `workspace.*` - Workspace-scoped settings
- `location.*` - Per-location settings
- `action.*` - Action buttons (not persisted)

## Control Types

| Type | Description |
|------|-------------|
| `toggle` | Boolean on/off switch |
| `dropdown` | Single selection from list |
| `slider` | Numeric range with step |
| `multi_select` | Checkbox list for multiple selections |
| `segmented` | Radio button row |
| `button` | Action button |
| `list` | Editable list |
| `token_list` | Comma-separated values |
| `status` | Read-only status indicator |
| `read_only` | Display-only value |
| `link` | Clickable link |
| `time_range` | Time range picker |
| `editor` | Text editor |
| `reorder_list` | Draggable reorderable list |

## Scope Definitions

- **Global** - Applies across the entire application
- **Workspace** - Overrides global for specific workspaces
- **Location** - Overrides workspace for specific paths
