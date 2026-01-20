# Vantus Presets

Vantus includes four preset configurations optimized for different use cases and environments.

## Preset Overview

| Preset | Target Audience | Key Characteristics |
|--------|-----------------|---------------------|
| **Personal** | Individual users | Best experience, helpful previews, suggestions over automation |
| **Pro** | Power users | Aggressive extraction, approval-based organizing |
| **Enterprise-Private** | Privacy-conscious organizations | Minimal retention, strict controls |
| **Enterprise-Automation** | Managed environments | Trusted rules can auto-run, heavy auditing |

---

## Personal Preset

**Intent:** Best experience for individual users on personal devices.

### Core Philosophy
- Maximizes user experience with rich previews and suggestions
- Minimizes friction with automatic tagging and organizing suggestions
- Privacy defaults balanced for personal use

### Key Default Settings

| Category | Setting | Default Value |
|----------|---------|---------------|
| Performance | Performance profile | Balanced |
| Privacy | Private mode | Off |
| Automation | Automation guardrails | Conservative |
| Indexing | CPU usage limit | 40% |
| Indexing | Max text per file | 50k chars |
| Extraction | Store text snippets | Yes |
| Search | Content snippets | Enabled |
| Tags | Auto-tag files | Enabled |

### What Changes When Applied
The Personal preset applies approximately 45 settings changes across all categories, focusing on:
- Enabling all preview and snippet features
- Setting moderate resource usage limits
- Enabling automatic tagging and suggestions
- Disabling strict enterprise restrictions

---

## Pro Preset

**Intent:** Power user configuration with aggressive indexing and explicit approval for organizing actions.

### Core Philosophy
- Extracts maximum information from files
- Requires approval for all file moves/renames
- Uses more system resources for faster indexing

### Key Default Settings

| Category | Setting | Default Value |
|----------|---------|---------------|
| Performance | Performance profile | Performance |
| Privacy | Private mode | Off |
| Automation | Automation guardrails | Standard |
| Indexing | CPU usage limit | 60% |
| Indexing | Max text per file | 100k chars |
| Extraction | Store text snippets | Yes |
| Search | Content snippets | Enabled |
| Tags | Auto-tag files | Enabled |
| Organizing | Default mode | Approval required |

### What Changes When Applied
The Pro preset applies approximately 50 settings changes:
- Increases resource limits (CPU, memory, extraction depth)
- Enables speaker separation for media
- Enables content-based partner matching
- Requires approval for all organizing actions
- Enables batch approvals

---

## Enterprise-Private Preset

**Intent:** Privacy-first configuration for shared or sensitive environments.

### Core Philosophy
- Minimizes stored content and metadata
- Enables all privacy protections
- Restricts automation to prevent accidental data exposure

### Key Default Settings

| Category | Setting | Default Value |
|----------|---------|---------------|
| Performance | Performance profile | Quiet |
| Privacy | Private mode | On |
| Automation | Automation guardrails | Strict |
| Indexing | CPU usage limit | 25% |
| Indexing | Max text per file | 15k chars |
| Extraction | Store text snippets | No |
| Search | Content snippets | Disabled |
| Tags | Auto-tag files | Disabled |
| Privacy | Encrypt index DB | Yes (Locked) |

### What Changes When Applied
The Enterprise-Private preset applies approximately 55 settings changes:
- Enables embeddings-only mode
- Disables all content previews and snippets
- Reduces all file processing limits
- Enables encryption (locked)
- Disables auto-tagging by default
- Requires Hello for previews

---

## Enterprise-Automation Preset

**Intent:** Managed environments with trusted automation and comprehensive auditing.

### Core Philosophy
- Allows trusted rules to run automatically
- Enables comprehensive audit logging
- Supports organization-wide policy enforcement

### Key Default Settings

| Category | Setting | Default Value |
|----------|---------|---------------|
| Performance | Performance profile | Balanced |
| Privacy | Private mode | On |
| Automation | Automation guardrails | Standard/Strict |
| Indexing | CPU usage limit | 50% |
| Indexing | Max text per file | 50k chars |
| Extraction | Store text snippets | No |
| Search | Content snippets | Disabled |
| Tags | Auto-tag files | Enabled (Policy) |
| Audit | Enable audit logging | Yes (Locked) |

### What Changes When Applied
The Enterprise-Automation preset applies approximately 52 settings changes:
- Enables audit logging (locked)
- Disables content previews (locked)
- Enables trusted rules mode
- Extends undo retention to 365 days
- Enables partner matching with policy controls

---

## Using Presets

### Applying a Preset

1. Navigate to **General > Modes & Presets**
2. Select a preset from the dropdown
3. Click "Preview Changes" to see what will change
4. Click "Apply Preset" to apply all settings

### Preset Preview

The preview shows:
- Total number of settings that will change
- Number of affected pages
- List of individual changes with current vs. new values

### Post-Preset Customization

Presets are starting points. After applying a preset:
- All settings remain editable (unless locked by policy)
- Customizations persist across preset switches
- Reverting to the same preset will not overwrite customizations

### Resetting to Preset

To reset all settings to the current preset defaults:
1. Go to **General > Modes & Presets**
2. Click "Revert to Preset Defaults"
3. Confirm the action

## Preset Technical Details

### File Location
Presets are defined in `settings_definitions.json` under the `presets` section.

### Schema Structure
```json
{
  "presets": {
    "personal": "Personal",
    "pro": "Pro", 
    "enterprise_private": "Enterprise-Private",
    "enterprise_automation": "Enterprise-Automation"
  }
}
```

### Default Values Format
Each setting definition includes a `defaults` object:
```json
"defaults": {
  "personal": true,
  "pro": true,
  "enterprise_private": false,
  "enterprise_automation": true
}
```

### Preset Manager Service

The `PresetManager` class handles:
- Loading available presets from schema
- Calculating diff between current settings and preset
- Applying preset values
- Generating preview changes
