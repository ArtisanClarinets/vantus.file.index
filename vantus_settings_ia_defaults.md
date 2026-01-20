# Vantus File Indexer — Complete Settings IA & Defaults (Design + Engineering Source of Truth)
_Last updated: 2026-01-19 (America/Chicago)_

This document is a **single source of truth** for:
- The **left-nav settings sitemap** (Figma-ready IA)
- **Exact control labels**, **helper text**, and **default values per preset**
- Notes for **scope** (Global / Workspace / Location), **managed-mode locking**, and **danger-zone actions**

> Presets covered: **Personal**, **Pro**, **Enterprise-Private**, **Enterprise-Automation**

---

## Presets (intent)
- **Personal** — best experience, helpful previews, suggestions over automation  
- **Pro** — power user, more aggressive extraction, approval-based organizing  
- **Enterprise-Private** — privacy-first, minimized content retention, strong controls  
- **Enterprise-Automation** — managed environment, trusted rules can auto-run, heavy auditing  

### Default conventions used throughout
- ✅ = On
- ⛔ = Off
- **Locked** = enforced by policy (control disabled + lock UI + reason)
- **Policy** = value supplied by policy; user cannot change
- **Available** = action exists, not a saved setting
- **Admin only** = action exists only for admin / managed environments

---

## Setting ID conventions
Use `category.subcategory.setting_name` with these scope suffixes when needed:
- Global: `general.*`, `indexing.*`, `privacy.*` …
- Workspace: `workspace.*`
- Location: `location.*` (applies per included root)
- Actions (buttons): `action.*` (not persisted as a setting)

**Example IDs**
- `indexing.cpu_usage_limit`
- `ai.compute_device_preference`
- `privacy.encrypt_index_db`
- `windows.explorer.enable_preview_panel`
- `action.rebuild_index`

---

# 1) Left-nav sitemap (Figma-ready)
Copy/paste into Figma as a page tree:

- **Settings**
  - **General**
    - Appearance & Language
    - Startup & Tray
    - Modes & Presets
    - Power & Performance
    - Data Handling
  - **Workspaces**
    - Workspace Switcher
    - Workspace Defaults
    - Import & Export
  - **Locations**
    - Included Locations
    - Exclusions
    - Location Policy (Detail)
  - **Indexing**
    - Status
    - Change Detection
    - Performance
    - Content Limits
  - **AI Models**
    - Runtime & Hardware
    - Model Set
    - Quality Controls
  - **Extraction**
    - Documents (PDF/Office/Text)
    - Images
    - Code
    - Archives
    - Media (Audio/Video)
    - Email & Exports
  - **Organize & Automations**
    - Organizing Mode
    - Rules
    - Safety & Undo
    - Review Queue
  - **Tags & Taxonomy**
    - Tagging Behavior
    - Vocabulary & Synonyms
    - Windows Metadata Fields
  - **Partners**
    - Partner Directory
    - Matching Logic
    - Partner Policies
  - **Search**
    - Search Mode
    - Results & Previews
    - Ranking & Facets
  - **Windows Integration**
    - Explorer Surfaces
    - Context Menu Actions
    - Windows Search Integration
  - **Privacy & Security**
    - Local Storage & Encryption
    - Access Controls
    - Sensitive Data Protections
    - Keys & Rotation
  - **Compliance & Audit**
    - Audit Log
    - Retention & Legal Hold
    - Policy Reports
  - **Notifications**
    - Alerts & Banners
    - Quiet Hours
  - **Storage & Maintenance**
    - Storage Locations
    - Cache Controls
    - Maintenance & Repair
    - Backup & Restore
  - **Diagnostics**
    - System Status
    - Performance Trace
    - Export Diagnostics Bundle
    - Advanced (Power User)
  - **Admin (Managed)**
    - Policy Overview
    - Enforced Restrictions
    - Deployment & Updates
  - **About**
    - Version & Licenses
    - Reset

---

# 2) Page-by-page controls, helper text, and preset defaults

## General → Appearance & Language
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `general.theme` | Segmented | Theme | Match Windows or choose a theme for Vantus. | System | System | System | System |
| `general.accent_color` | Dropdown | Accent color | Used for highlights and status indicators. | Windows | Windows | Windows | Windows |
| `general.app_language` | Dropdown | App language | Language for menus and settings. | System | System | System | System |
| `general.reduce_motion` | Toggle | Reduce motion | Minimize animations for comfort and accessibility. | ⛔ | ⛔ | ✅ | ✅ |
| `general.increase_contrast` | Toggle | Increase contrast | Improve readability in low-contrast environments. | Inherit OS | Inherit OS | Inherit OS | Inherit OS |

---

## General → Startup & Tray
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `general.launch_on_signin` | Toggle | Launch Vantus at sign-in | Start in the background so indexing stays current. | ✅ | ✅ | ✅ | ✅ (Locked if managed) |
| `general.minimize_to_tray_on_close` | Toggle | Minimize to system tray when closed | Closing the window keeps Vantus running. | ✅ | ✅ | ✅ | ✅ |
| `general.tray_status_badge` | Toggle | Show tray status badge | Shows indexing status and alerts on the tray icon. | ✅ | ✅ | ✅ | ✅ |
| `general.tray_quick_actions` | Multi-select | Tray quick actions | Choose actions available from the tray menu. | Search, Pause | Search, Pause, Review Queue | Search, Pause | Pause, Review Queue |

---

## General → Modes & Presets
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `general.preset` | Dropdown | Preset | Applies recommended defaults. You can still customize settings. | Personal | Pro | Enterprise-Private | Enterprise-Automation (Locked if managed) |
| `general.performance_profile` | Segmented | Performance profile | Controls CPU/disk usage and how aggressively Vantus runs. | Balanced | Performance | Quiet | Balanced/Performance (Policy) |
| `general.private_mode` | Toggle | Private mode | Hides content snippets and reduces stored text. Best for shared machines. | ⛔ | ⛔ | ✅ | ✅ (Locked) |
| `general.automation_guardrails` | Dropdown | Automation guardrails | How cautious Vantus is about moving/renaming files. | Conservative | Standard | Strict | Standard/Strict (Policy) |

---

## General → Power & Performance
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `general.pause_on_battery` | Toggle | Pause indexing on battery | Reduce background activity when unplugged. | ✅ | ⛔ | ✅ | ✅ |
| `general.battery_threshold` | Slider | Battery threshold | Pause below this battery level. | 35% | 25% | 40% | 35% |
| `general.pause_on_low_disk` | Toggle | Pause on low disk space | Prevents caches from filling your drive. | ✅ | ✅ | ✅ | ✅ |
| `general.low_disk_threshold` | Dropdown | Low disk threshold | Amount of free space to keep available. | 10 GB | 10 GB | 20 GB | 10 GB |
| `general.heavy_tasks_idle_only` | Toggle | Only run heavy tasks when idle | OCR/transcription and deep extraction run when you’re away. | ✅ | ✅ | ✅ | ✅ |

---

## General → Data Handling
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `general.on_device_only_status` | Status | Keep everything on this device | Vantus processes files locally. No cloud upload is required. | On-device | On-device | On-device | On-device |
| `general.analytics_anonymous` | Toggle | Share anonymous usage analytics | Helps improve performance and stability. Never includes file contents. | ⛔ | ⛔ | ⛔ (Locked) | ⛔ (Locked) |
| `general.crash_reports` | Toggle | Allow crash reports | Sends crash details to improve reliability. | ✅ | ✅ | ✅ (content redacted) | ✅ (content redacted) |

---

## Workspaces → Workspace Switcher
Scope: Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `workspace.active_workspace` | Dropdown | Active workspace | Switch configurations for different contexts (Personal/Work/Client). | Personal | Pro | Work (default) | Work (Locked) |
| `workspace.lock_switching` | Toggle | Lock workspace switching | Require Windows Hello to switch workspaces. | ⛔ | ⛔ | ✅ | ✅ (Locked) |

---

## Workspaces → Workspace Defaults
Scope: Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `workspace.default_tag_namespace` | Dropdown | Default tag namespace | Applied when new tags are created. | Personal | Work | Work | Work |
| `workspace.default_organizing_mode` | Dropdown | Default organizing mode | Used for new locations unless overridden. | Suggestions only | Approval required | Index only | Trusted rules only (Locked) |
| `workspace.default_preview_behavior` | Dropdown | Default preview behavior | Controls snippets/previews across the workspace. | Full previews | Full previews | Safe previews | Safe previews (Locked) |

---

## Workspaces → Import & Export
Scope: Workspace / Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `action.export_workspace_settings` | Button | Export workspace settings | Save a portable configuration file. | Available | Available | Available | Admin only |
| `action.import_workspace_settings` | Button | Import workspace settings | Replace settings from a configuration file. | Available | Available | Available | Admin only |

---

## Locations → Included Locations
Scope: Location (per root)

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `action.locations_add_folder` | Button | Add folder… | Add a location to index. | Available | Available | Available | Available (policy may restrict) |
| `action.locations_add_drive` | Button | Add drive… | Index an entire drive. Recommended only for secondary drives. | Available | Available | Available | Available (policy may restrict) |
| `action.locations_add_share` | Button | Add network share… | Index SMB shares (may be slower). | ⛔ | Optional | Optional (often off) | Optional (policy) |
| `location.index_enabled` | Toggle | Index this location | Include files from this location in the index. | ✅ | ✅ | ✅ | ✅ |
| `location.allow_organizing` | Toggle | Allow organizing actions | Allow moves/renames/taxonomy actions in this location. | ⛔ (default) | ✅ (with approval) | ⛔ (Locked) | ✅ (Locked for trusted rules) |
| `location.treat_as_sensitive` | Toggle | Treat as sensitive | Stricter retention and preview rules for this location. | ⛔ | ⛔ | ✅ | ✅ |

---

## Locations → Exclusions
Scope: Global + Location

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `locations.excluded_folders` | List | Excluded folders | Folders that will never be indexed. | System defaults | System defaults | Expanded defaults | Expanded defaults |
| `locations.excluded_patterns` | Token list | Excluded patterns | Glob patterns like `node_modules` or `*.tmp`. | `node_modules`, `.git`, `bin`, `obj` | Same + `dist`, `build` | Same + `AppData*` | Same + managed list |
| `locations.excluded_file_types` | List | Excluded file types | File extensions that won’t be processed. | `.exe`, `.dll`, `.sys` | Same | Same + `.pst` optional | Policy-defined |

---

## Locations → Location Policy (Detail)
Scope: Location (per root)

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `location.index_depth` | Dropdown | Index depth | How much content is analyzed. | Full content | Full content | Metadata only (or Full without retention) | Full content (Policy) |
| `location.auto_tagging` | Toggle | Auto-tagging | Assign tags automatically as files are indexed. | ✅ | ✅ | ⛔ (or limited) | ✅ (Policy) |
| `location.partner_matching` | Toggle | Partner matching | Assign a client/vendor/project based on content and context. | ✅ | ✅ | ✅ (approval) | ✅ (Policy) |
| `location.organizing_mode` | Dropdown | Organizing mode | Controls moves/renames. | Suggestions only | Approval required | Index only | Trusted rules only |
| `location.heavy_tasks_schedule` | Dropdown | Heavy tasks schedule | OCR/transcription timing. | Idle only | Idle only | Idle only | Idle only / Policy |
| `location.max_file_size_to_process` | Dropdown | Max file size to process | Skip huge files to keep indexing fast. | 250 MB | 500 MB | 100 MB | 250–500 MB (Policy) |

---

## Indexing → Status
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `indexing.enabled` | Toggle | Indexing is enabled | Turn off to pause all background indexing. | ✅ | ✅ | ✅ | ✅ (Locked) |
| `action.reindex_now` | Button | Reindex now | Re-run extraction and AI metadata for current scope. | Available | Available | Available | Available |
| `action.rebuild_index` | Button | Rebuild index (advanced) | Deletes and rebuilds the index. Use only if support requests it. | Available | Available | Available | Admin only |

---

## Indexing → Change Detection
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `indexing.change_detection_mode` | Dropdown | Change detection mode | Auto chooses the most reliable method for your drives. | Auto | Auto | Auto | Auto |
| `indexing.full_sweep_frequency` | Dropdown | Full sweep frequency | Fallback scan to catch missed events. | Weekly | Weekly | Monthly | Weekly |
| `indexing.external_drives_behavior` | Dropdown | Index external drives when connected | How removable drives are handled. | Prompt | Prompt | Ignore | Policy |

---

## Indexing → Performance
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `indexing.cpu_usage_limit` | Slider | CPU usage limit | Caps CPU used by indexing and AI. | 40% | 60% | 25% | 50% |
| `indexing.disk_io_limit` | Slider | Disk IO limit | Caps disk reads/writes used by indexing. | Medium | High | Low | Medium |
| `indexing.parallel_extractors` | Dropdown | Parallel extractors | More parallelism is faster but uses more resources. | 2 | 4 | 1 | 3 |
| `indexing.throttle_while_active` | Toggle | Throttle while actively using the PC | Reduces background activity while you work. | ✅ | ⛔ | ✅ | ✅ |

---

## Indexing → Content Limits
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `indexing.max_text_per_file` | Dropdown | Max text per file to analyze | Limits content processed per file. | 50k chars | 100k chars | 15k chars | 50–100k chars |
| `indexing.max_pdf_pages_ocr` | Dropdown | Max PDF pages to OCR | Prevents long OCR runs. | 25 | 50 | 10 | 25 |
| `indexing.max_media_minutes_transcribe` | Dropdown | Max media minutes to transcribe | Prevents long transcriptions. | 10 | 30 | 0 (off) | 10–30 (Policy) |

---

## AI Models → Runtime & Hardware
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `ai.compute_device_preference` | Dropdown | Compute device preference | Auto chooses best device. Choose NPU/iGPU/CPU for troubleshooting. | Auto | Auto | Auto | Auto (Locked) |
| `ai.fallback_order` | Reorder list | Fallback order | If preferred device is unavailable, Vantus falls back in this order. | NPU → iGPU → CPU | NPU → iGPU → CPU | CPU → NPU → iGPU | Policy |
| `ai.low_power_inference` | Toggle | Low power inference | Uses smaller/faster models to reduce power usage. | ⛔ | ⛔ | ✅ | Policy |

---

## AI Models → Model Set
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `ai.embedding_model` | Dropdown | Embedding model | Used for semantic search and similarity. | Balanced (default) | High quality | Balanced (approved) | Locked (approved) |
| `ai.classifier_model` | Dropdown | Classifier model | Used for tags/category/partner assignment. | Balanced (default) | High quality | Conservative (approved) | Locked (approved) |
| `ai.explanation_model_enabled` | Dropdown | Explanation model | Generates “Why?” explanations. Optional. | On | On | Off | Off or Policy |

---

## AI Models → Quality Controls
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `ai.auto_tag_threshold` | Slider | Auto-tag confidence threshold | Minimum confidence required to apply tags automatically. | 0.70 | 0.65 | 0.85 | 0.75 |
| `ai.auto_partner_threshold` | Slider | Auto-partner confidence threshold | Minimum confidence required to assign a partner automatically. | 0.75 | 0.70 | 0.90 | 0.80 |
| `ai.auto_organize_threshold` | Slider | Auto-organize confidence threshold | Minimum confidence required to move/rename automatically. | 0.90 | 0.85 | 0.95 | 0.90 (Policy) |
| `ai.require_approval_below_threshold` | Toggle | Require approval below thresholds | Low-confidence actions go to Review Queue. | ✅ | ✅ | ✅ | ✅ (Locked) |
| `ai.deterministic_results` | Toggle | Deterministic results | More consistent outputs (recommended for enterprise). | ⛔ | ⛔ | ✅ | ✅ |

---

## Extraction → Documents (PDF/Office/Text)
Scope: Global + Location overrides

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `extract.docs.extract_text` | Toggle | Extract text from documents | Enables content indexing for docs and PDFs. | ✅ | ✅ | ✅ | ✅ |
| `extract.docs.store_snippets` | Toggle | Store text snippets for search previews | Shows short excerpts in search results and Explorer panel. | ✅ | ✅ | ⛔ (Locked) | ⛔ (Locked) |
| `extract.docs.office_metadata` | Toggle | Office metadata (author/title/comments) | Adds document metadata to the index. | ✅ | ✅ | ✅ | ✅ |
| `extract.docs.pdf_processing_mode` | Dropdown | PDF processing mode | Choose how PDFs are read. | Prefer text layer | Hybrid | Prefer text layer | Hybrid (Policy) |
| `extract.docs.ocr_scanned_pdfs` | Dropdown | OCR scanned PDFs | Runs OCR when text is missing. | Auto | Auto | Off / On-demand | Auto (idle) |
| `extract.docs.ocr_languages` | Multi-select | OCR languages | Languages used during OCR. | English | English + user adds | English (approved) | Policy |

---

## Extraction → Images
Scope: Global + Location overrides

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `extract.images.read_exif` | Toggle | Read EXIF metadata | Adds camera/date info for photos. | ✅ | ✅ | ✅ | ✅ |
| `extract.images.ocr_mode` | Dropdown | OCR images (screenshots/scans) | Extract text from images. | On-demand | Auto | Off / On-demand | On-demand |
| `extract.images.visual_tagging` | Dropdown | Visual tagging (objects/scenes) | Generates visual tags. Can be resource-intensive. | Off | On-demand | Off (Locked) | Off (Policy unless approved) |

---

## Extraction → Code
Scope: Global + Location overrides

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `extract.code.code_aware_indexing` | Toggle | Enable code-aware indexing | Improves results for source code and repos. | ✅ | ✅ | ✅ | ✅ |
| `extract.code.respect_gitignore` | Toggle | Respect `.gitignore` | Skips files marked ignored by the repo. | ✅ | ✅ | ✅ | ✅ |
| `extract.code.index_dependencies` | Toggle | Index dependency folders (node_modules/vendor) | Not recommended; increases index size. | ⛔ | ⛔ | ⛔ | ⛔ |
| `extract.code.extract_symbols` | Toggle | Extract symbols (classes/functions) | Adds structure for better code search. | ✅ | ✅ | ✅ | ✅ |

---

## Extraction → Archives
Scope: Global + Location overrides

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `extract.archives.index_manifest` | Toggle | Index archive contents list (manifest) | Indexes file names inside ZIP/RAR/7z. | ✅ | ✅ | ✅ | ✅ |
| `extract.archives.unpack_small_archives` | Toggle | Unpack small archives for full indexing | Extracts small archives to index contents. | ⛔ | ✅ | ⛔ (Locked) | Policy |
| `extract.archives.small_archive_size_limit` | Dropdown | Small archive size limit | Maximum archive size to unpack. | — | 250 MB | — | Policy |
| `extract.archives.password_archives_behavior` | Dropdown | Password-protected archives | How encrypted archives are handled. | Skip | Prompt | Skip (Locked) | Skip/Policy |

---

## Extraction → Media (Audio/Video)
Scope: Global + Location overrides

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `extract.media.transcription_mode` | Dropdown | Transcription | Generates searchable transcripts. | On-demand | Idle-only | Off (Locked) | Policy |
| `extract.media.store_transcript_text` | Toggle | Store transcript text | Allows snippet previews from transcripts. | ✅ | ✅ | ⛔ (Locked) | ⛔ (Locked) |
| `extract.media.speaker_separation` | Toggle | Speaker separation | Attempts to identify different speakers. | ⛔ | ✅ | ⛔ | Policy |
| `extract.media.max_minutes_transcribe` | Dropdown | Max media minutes to transcribe | Prevents long jobs. | 10 | 30 | — | Policy |

---

## Extraction → Email & Exports
Scope: Global + Location overrides

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `extract.email.enable_parsing` | Toggle | Enable email export parsing (PST/mbox) | Indexes supported email exports and attachments. | ⛔ | ✅ | ⛔ (Policy) | Policy |
| `extract.email.index_bodies` | Dropdown | Index email bodies | Choose how much email content is stored. | — | Full | Headers only | Policy |
| `extract.email.index_attachments` | Toggle | Index attachments | Attachments are indexed like normal files. | — | ✅ | ✅ (approval) | ✅ (Policy) |

---

## Organize & Automations → Organizing Mode
Scope: Global + Workspace + Location

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `organize.mode` | Dropdown | Organizing mode | Controls whether Vantus moves/renames files. | Suggestions only | Approval required | Index only | Trusted rules only (Locked) |
| `organize.show_suggestions_in_explorer` | Toggle | Show organizing suggestions in Explorer panel | Suggestions appear when you select a file. | ✅ | ✅ | ✅ | ✅ |
| `organize.allow_one_click_apply` | Toggle | Allow one-click apply | Apply suggestion without opening the app. | ✅ | ✅ | ⛔ | ✅ (Policy) |

---

## Organize & Automations → Rules
Scope: Global + Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `organize.rules_engine_enabled` | Toggle | Enable rules engine | Rules drive consistent tagging and organizing. | ✅ | ✅ | ✅ | ✅ (Locked) |
| `organize.suggest_rules_from_approvals` | Toggle | Suggest new rules from your approvals | Learns patterns and proposes new rules. | ✅ | ✅ | ⛔ | ✅ (Policy) |
| `organize.require_approval_for_new_rules` | Toggle | Require approval for new rules | Prevents silent automation changes. | ✅ | ✅ | ✅ | ✅ (Locked) |
| `organize.rule_priority_mode` | Dropdown | Rule priority mode | How conflicts between rules are resolved. | Highest priority wins | Highest priority wins | Strict + review | Strict + audit |

---

## Organize & Automations → Safety & Undo
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `organize.never_move_open_files` | Toggle | Never move open files | Avoids file lock issues and data loss. | ✅ | ✅ | ✅ | ✅ |
| `organize.copy_verify_delete` | Toggle | Copy → verify → delete | Safer but slower moves. | ✅ | ✅ | ✅ | ✅ |
| `organize.cloud_sync_behavior` | Dropdown | Avoid cloud-sync conflicts | OneDrive/Dropbox safety behavior. | Prompt | Prompt | Avoid | Policy |
| `organize.collision_handling` | Dropdown | Collision handling | How renames are handled when a name exists. | Add suffix | Add timestamp | Add suffix | Policy |
| `organize.undo_retention_window` | Dropdown | Undo retention window | How long changes can be undone. | 30 days | 90 days | 90 days | 365 days |

---

## Organize & Automations → Review Queue
Scope: Global + Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `organize.review_queue_enabled` | Toggle | Enable Review Queue | Collects low-confidence or approval-required actions. | ✅ | ✅ | ✅ | ✅ |
| `organize.review_notify_threshold` | Dropdown | Notify when queue exceeds | Helps you keep up with pending items. | 25 | 50 | 20 | 100 |
| `organize.batch_approvals_enabled` | Toggle | Batch approvals | Approve multiple actions at once. | ✅ | ✅ | ✅ | ✅ |

---

## Tags & Taxonomy → Tagging Behavior
Scope: Global + Workspace + Location

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `tags.auto_tag_files` | Toggle | Auto-tag files | Adds tags during indexing. | ✅ | ✅ | ⛔ (or limited) | ✅ (Policy) |
| `tags.suggest_tags_while_browsing` | Toggle | Suggest tags while you browse | Shows recommended tags in the Explorer panel. | ✅ | ✅ | ✅ | ✅ |
| `tags.max_tags_per_file` | Dropdown | Maximum tags per file | Limits tag spam. | 10 | 20 | 5 | 10 |

---

## Tags & Taxonomy → Vocabulary & Synonyms
Scope: Workspace (primarily)

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `tags.vocabulary_mode` | Dropdown | Tag vocabulary mode | Choose freeform or a controlled list. | Freeform | Freeform | Controlled | Controlled (Locked) |
| `tags.synonyms_editor_enabled` | Editor | Synonyms | Map tag synonyms (e.g., “invoice” ↔ “bill”). | Enabled | Enabled | Enabled | Enabled (Admin) |

---

## Tags & Taxonomy → Windows Metadata Fields
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `windows.props.expose_ai_tags` | Toggle | Expose “AI Tags” to Windows Explorer | Shows tags in Details pane and as a column. | ✅ | ✅ | ✅ | ✅ |
| `windows.props.expose_ai_partner` | Toggle | Expose “AI Partner” to Windows Explorer | Shows partner in Details pane and as a column. | ✅ | ✅ | ✅ | ✅ |
| `windows.props.expose_ai_explanation` | Toggle | Expose “AI Explanation” to Windows Explorer | Shows a short “Why?” summary. | ✅ | ✅ | ⛔ | ⛔ (Policy) |
| `windows.props.tag_formatting` | Dropdown | Tag formatting | How tags are shown in Explorer columns. | Semicolon-separated | Semicolon-separated | Semicolon-separated | Policy |

---

## Partners → Partner Directory
Scope: Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `partners.enable_directory` | Toggle | Enable partner directory | Track clients/vendors/projects for better organization. | ✅ | ✅ | ✅ | ✅ |
| `partners.types_enabled` | Multi-select | Partner types enabled | Choose which partner categories you use. | Client, Vendor | Client, Vendor, Project | Client, Vendor, Project | Policy |
| `partners.default_destination_per_partner` | Toggle | Default destination per partner | Partners can map to default folders. | ⛔ | ✅ | ⛔ | ✅ (Policy) |

---

## Partners → Matching Logic
Scope: Workspace + Location overrides

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `partners.match_by_domain` | Toggle | Match by email domain | High-confidence matching when domains are present. | ✅ | ✅ | ✅ | ✅ |
| `partners.match_by_filename` | Toggle | Match by filename patterns | Uses file naming conventions. | ✅ | ✅ | ✅ | ✅ |
| `partners.match_by_content` | Toggle | Match by document content | Uses extracted text to infer partner. | ✅ | ✅ | ✅ (approval) | ✅ (Policy) |
| `partners.require_approval_for_assignment` | Toggle | Require approval for partner assignment | Sends ambiguous matches to Review Queue. | ⛔ | ⛔ | ✅ | Policy |
| `partners.partner_confidence_threshold` | Slider | Partner confidence threshold | Minimum confidence for auto-assignment. | 0.75 | 0.70 | 0.90 | 0.80 |

---

## Partners → Partner Policies
Scope: Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `partners.per_partner_exclusions` | Editor | Per-partner exclusions | Exclude certain folders or file types for a partner. | Available | Available | Available | Available |
| `action.partners.pin_to_folder` | Button | Pin partner to folder | Treat everything in this folder as this partner. | Available | Available | Available | Available (Policy) |

---

## Search → Search Mode
Scope: Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `search.default_mode` | Dropdown | Default search mode | Choose keyword, semantic, or hybrid search. | Hybrid | Hybrid | Keyword/Hybrid | Hybrid |
| `search.enable_find_similar` | Toggle | Enable “Find similar” | Uses embeddings to find related files. | ✅ | ✅ | ✅ | ✅ |

---

## Search → Results & Previews
Scope: Workspace + Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `search.show_content_snippets` | Toggle | Show content snippets in search results | Displays excerpts from documents and transcripts. | ✅ | ✅ | ⛔ (Locked) | ⛔ (Locked) |
| `search.show_thumbnails` | Toggle | Show thumbnails/previews | Shows file previews in results. | ✅ | ✅ | ✅ (safe) | ✅ (safe/Policy) |
| `search.require_hello_for_previews` | Toggle | Require Windows Hello for previews | Adds privacy on shared machines. | ⛔ | ⛔ | ✅ | ✅ (Policy) |

---

## Search → Ranking & Facets
Scope: Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `search.recency_boost` | Slider | Recency boost | Prefer newer files in results. | Medium | High | Low | Medium |
| `search.filename_exact_match_boost` | Slider | Filename exact-match boost | Boost exact filename hits. | High | High | High | High |
| `search.enabled_facets` | Multi-select | Enabled facets | Filters available in search. | Type, Date, Tags, Partner | +Location, Size | Type, Date, Partner | Policy |

---

## Windows Integration → Explorer Surfaces
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `windows.explorer.enable_columns` | Toggle | Enable Explorer columns | Adds columns like AI Tags and AI Partner. | ✅ | ✅ | ✅ | ✅ |
| `windows.explorer.enable_details_pane` | Toggle | Enable Details pane properties | Shows AI metadata when a file is selected. | ✅ | ✅ | ✅ | ✅ |
| `windows.explorer.enable_preview_panel` | Toggle | Enable Vantus AI Panel (Preview pane) | Shows a rich panel in the Explorer Preview pane. | Optional (off) | ✅ | ✅ (read-only) | ✅ (Policy) |

---

## Windows Integration → Context Menu Actions
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `windows.context_menu.actions` | Multi-select | Context menu actions | Choose actions shown in Explorer. | Tag, Explain, Reindex | +Assign Partner, Propose Move | Tag, Explain (limited) | Policy |
| `windows.context_menu.modern_menu` | Toggle | Show actions in modern menu | Attempt to show in the Windows 11 primary menu. | ✅ | ✅ | ✅ | ✅ |

---

## Windows Integration → Windows Search Integration
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `windows.search.register_ai_properties` | Toggle | Register AI properties with Windows Search | Lets Windows Search filter on AI Tags/Partner. | ✅ | ✅ | ✅ | ✅ |
| `windows.search.allow_index_extracted_text` | Toggle | Allow Windows Search to index extracted text | Not recommended for strict privacy environments. | ✅ | ✅ | ⛔ (Locked) | ⛔ (Locked) |
| `action.windows.search.rebuild_property_cache` | Button | Rebuild Windows property cache | Use if Explorer columns look out of date. | Available | Available | Available | Available |

---

## Privacy & Security → Local Storage & Encryption
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `privacy.encrypt_index_db` | Toggle | Encrypt index database | Protects metadata at rest. | ✅ | ✅ | ✅ (Locked) | ✅ (Locked) |
| `privacy.encrypt_text_cache` | Toggle | Encrypt extracted text cache | Protects stored text and previews at rest. | Optional | ✅ | ✅ (Locked) | ✅ (Locked) |
| `privacy.embeddings_only_mode` | Toggle | Embeddings-only mode | Stores embeddings without raw extracted text. | ⛔ | ⛔ | ✅ | Policy |
| `privacy.store_previews_thumbnails` | Toggle | Store previews/thumbnails | Improves UX but may store visual content locally. | ✅ | ✅ | ⛔ | ⛔ (Policy) |

---

## Privacy & Security → Access Controls
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `privacy.require_signin_to_open` | Toggle | Require sign-in to open Vantus | Prevents casual access on shared machines. | ⛔ | ⛔ | ✅ | ✅ (Policy) |
| `privacy.require_hello_to_edit_rules` | Toggle | Require Windows Hello to edit rules | Adds protection against accidental automation changes. | ⛔ | ✅ | ✅ | ✅ (Policy) |
| `privacy.require_hello_to_view_previews` | Toggle | Require Windows Hello to view previews | Prevents shoulder-surfing. | ⛔ | ⛔ | ✅ | ✅ (Policy) |

---

## Privacy & Security → Sensitive Data Protections
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `privacy.redact_sensitive_in_logs` | Toggle | Redact sensitive strings in logs | Redacts patterns like SSNs and credit cards in diagnostics. | ✅ | ✅ | ✅ (Locked) | ✅ (Locked) |
| `privacy.disable_indexing_in_sensitive_locations` | Toggle | Disable indexing in sensitive locations | Blocks indexing where policies prohibit content processing. | ⛔ | ⛔ | ✅ | Policy |

---

## Privacy & Security → Keys & Rotation
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `privacy.key_storage` | Dropdown | Key storage | How encryption keys are protected. | Windows (DPAPI) | Windows (DPAPI) | Windows (DPAPI) / Enterprise key | Policy |
| `action.privacy.rotate_keys` | Button | Rotate encryption keys | Rotates keys and re-encrypts stored data. | Available | Available | Available (admin) | Admin only |

---

## Compliance & Audit → Audit Log
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `audit.enabled` | Toggle | Enable audit logging | Records actions like moves, renames, partner edits. | ✅ | ✅ | ✅ (Locked) | ✅ (Locked) |
| `audit.detail_level` | Dropdown | Audit detail level | Higher detail increases log size. | Standard | Detailed | Standard | Detailed (Locked) |
| `action.audit.export_log` | Button | Export audit log | Exports to CSV/JSON. | Available | Available | Available | Available |

---

## Compliance & Audit → Retention & Legal Hold
Scope: Global + Managed

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `compliance.legal_hold_mode` | Toggle | Legal hold mode | Prevents automated moves/renames for held content. | ⛔ | ⛔ | ✅ | ✅ (Policy) |
| `audit.retention` | Dropdown | Audit log retention | How long audit logs are kept. | 90 days | 180 days | 365 days | 365+ (Policy) |

---

## Compliance & Audit → Policy Reports
Scope: Global + Managed

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `action.reports.index_scope` | Button | Generate index scope report | Shows which locations and file types are indexed. | Available | Available | Available | Available |
| `action.reports.security_posture` | Button | Generate security posture report | Summarizes encryption, previews, and retention settings. | Available | Available | Available | Available |

---

## Notifications → Alerts & Banners
Scope: Workspace (mostly)

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `notify.indexing_completed` | Toggle | Indexing completed | Notify when background indexing finishes. | ⛔ | ⛔ | ⛔ | ⛔ |
| `notify.suggestions_ready` | Toggle | Suggestions ready | Notify when suggestions are ready to review. | ✅ | ✅ | ✅ | ✅ |
| `notify.errors_attention` | Toggle | Errors requiring attention | Notify when a fix is needed. | ✅ | ✅ | ✅ | ✅ |

---

## Notifications → Quiet Hours
Scope: Workspace

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `notify.quiet_hours_enabled` | Toggle | Enable quiet hours | Suppress notifications during set hours. | ✅ | ✅ | ✅ | ✅ |
| `notify.quiet_hours_schedule` | Time range | Quiet hours schedule | Notifications will be delivered after quiet hours. | 10 PM–8 AM | 10 PM–8 AM | 9 PM–9 AM | Policy |

---

## Storage & Maintenance → Storage Locations
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `storage.index_location` | Dropdown | Index storage location | Choose where Vantus stores its database. | System drive | System drive | System drive / approved location | Policy |
| `action.storage.move_index` | Button | Move index to another drive | Moves index and caches safely. | Available | Available | Available (admin) | Admin only |

---

## Storage & Maintenance → Cache Controls
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `storage.text_cache_size_limit` | Dropdown | Text cache size limit | Caps storage for extracted text. | 5 GB | 10 GB | 1 GB | Policy |
| `storage.preview_cache_size_limit` | Dropdown | Preview/thumbnail cache size limit | Caps storage for thumbnails. | 2 GB | 5 GB | 0 GB | Policy |

---

## Storage & Maintenance → Maintenance & Repair
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `storage.optimize_db_weekly` | Toggle | Optimize database weekly | Improves performance over time. | ✅ | ✅ | ✅ | ✅ |
| `storage.integrity_check_monthly` | Toggle | Integrity check monthly | Detects corruption early. | ✅ | ✅ | ✅ | ✅ |
| `action.storage.clear_caches` | Button | Clear caches | Clears previews and temporary extraction data. | Available | Available | Available | Available |

---

## Storage & Maintenance → Backup & Restore
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `action.storage.backup_index` | Button | Backup index | Saves index state for recovery. | Optional | Available | Available | Admin only |
| `action.storage.restore_index` | Button | Restore index | Restores from a backup file. | Available | Available | Available (admin) | Admin only |

---

## Diagnostics → System Status
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `diag.show_device_usage` | Read-only | Show runtime device usage | Shows whether NPU/iGPU/CPU is being used. | ✅ | ✅ | ✅ | ✅ |
| `diag.show_model_versions` | Read-only | Show current model versions | Model IDs and update channel. | ✅ | ✅ | ✅ | ✅ |
| `action.diag.test_extraction` | Button | Test extraction on a file… | Runs the pipeline and shows each stage output. | Available | Available | Available | Available (Admin may restrict) |

---

## Diagnostics → Performance Trace
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `action.diag.start_trace` | Button | Start performance trace | Captures performance metrics for support. | Available | Available | Available | Available |
| `diag.include_paths_in_trace` | Toggle | Include file paths in trace | May reveal sensitive paths. Use only if approved. | ✅ | ✅ | ⛔ (Locked) | ⛔ (Locked) |

---

## Diagnostics → Export Diagnostics Bundle
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `action.diag.export_bundle` | Button | Export diagnostics bundle | Exports logs and settings (redacted) for support. | Available | Available | Available | Admin only |
| `diag.redact_content_snippets` | Toggle | Redact content snippets | Removes any content previews from logs. | ✅ | ✅ | ✅ (Locked) | ✅ (Locked) |

---

## Diagnostics → Advanced (Power User)
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `diag.show_raw_ai_json` | Toggle | Show raw AI metadata (JSON) | For debugging and power users. | ⛔ | ✅ | ⛔ | ⛔ |
| `diag.experimental_features` | Toggle | Enable experimental features | May be unstable. | ⛔ | Optional | ⛔ (Locked) | ⛔ (Locked) |

---

## Admin (Managed) → Policy Overview
Visibility: Managed-only

| Setting ID | Control | Label | Helper text | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|
| `admin.managed_status` | Status | This device is managed | Settings may be enforced by your organization. | ✅ | ✅ |
| `action.admin.view_enforced_policies` | Button | View enforced policies | Shows which settings are locked and why. | Available | Available |

---

## Admin (Managed) → Enforced Restrictions
Visibility: Managed-only (policy-defined list)

This page is a policy viewer/editor depending on SKU. Typical lock targets:
- Restrict indexed locations
- Disable organizing
- Disable OCR/transcription
- Force encryption
- Disable snippets
- Lock models/update channel
- Lock telemetry

---

## Admin (Managed) → Deployment & Updates
Visibility: Managed-only

| Setting ID | Control | Label | Helper text | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|
| `admin.update_channel` | Dropdown | Update channel | Stable or enterprise-approved preview. | Stable (Locked) | Stable (Locked) |
| `admin.update_deferral_window` | Dropdown | Update deferral window | Delay updates for validation. | 30 days | 30–90 days |
| `action.admin.export_policy_file` | Button | Export policy file | Generate a policy file for deployment tools. | Available | Available |

---

## About → Version & Licenses
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `about.version` | Read-only | Version | Current installed version. | — | — | — | — |
| `about.build_number` | Read-only | Build number | Internal build identifier. | — | — | — | — |
| `about.oss_licenses` | Link | Open-source licenses | Third-party licenses used by Vantus. | Available | Available | Available | Available |
| `about.privacy_statement` | Link | Privacy statement | How Vantus handles your data. | Available | Available | Available | Available |

---

## About → Reset (Danger Zone)
Scope: Global

| Setting ID | Control | Label | Helper text | Personal | Pro | Enterprise-Private | Enterprise-Automation |
|---|---|---|---|---:|---:|---:|---:|
| `action.about.reset_to_defaults` | Button | Reset Vantus to defaults | Clears settings and rebuilds index (requires confirmation). | Available | Available | Admin only | Admin only |

---

# Notes for Engineering
## Managed/locked UI requirements
- Locked controls must show:
  - a lock icon
  - disabled interaction
  - helper/tooltip: “Managed by your organization” + policy reason
- Admin pages appear only when `policy.managed == true`.

## Scope rules
- Global settings apply across app.
- Workspace settings override global where applicable.
- Location settings override workspace where applicable.

## Danger zone actions
- `Rebuild index`, `Reset to defaults`, `Clear caches`, key rotation, etc. must require confirm dialogs.

---

_End of document._
