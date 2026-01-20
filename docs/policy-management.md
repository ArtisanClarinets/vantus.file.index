# Vantus Policy Management

This document describes how organizations can manage Vantus settings through policies.

## Overview

Vantus supports managed mode through policy files that can:
- Lock specific settings to predefined values
- Restrict which locations can be indexed
- Block certain file types from processing
- Control update channels and deferral periods
- Provide auditing and compliance features

## Policy File Format

Policies are stored in JSON format (e.g., `policies.json`):

```json
{
  "managed": true,
  "schema_version": "1.0",
  "locks": [
    {
      "setting_id": "privacy.encrypt_index_db",
      "locked_value": true,
      "reason": "Encryption is required by organization policy.",
      "source": "MDM"
    }
  ],
  "allowed_locations": [
    "C:\\Users\\*\\Documents",
    "C:\\Users\\*\\Desktop"
  ],
  "blocked_extensions": [".pst", ".ost"],
  "update_channel": "Stable",
  "update_deferral_days": 30
}
```

## Policy Components

### Managed Mode Flag

```json
"managed": true
```

When `managed` is `true`, the device is in managed mode:
- Admin pages become visible in settings
- Policy-locked settings are enforced
- Managed preset is indicated in UI

### Setting Locks

Each lock entry specifies:

| Field | Description |
|-------|-------------|
| `setting_id` | The setting to lock (e.g., `privacy.encrypt_index_db`) |
| `locked_value` | The enforced value |
| `reason` | Human-readable explanation for the lock |
| `source` | Policy source (MDM, Intune, Local Policy, etc.) |

### Location Restrictions

```json
"allowed_locations": [
  "C:\\Users\\*\\Documents",
  "D:\\Work"
]
```

Wildcards (`*`) allow pattern matching for user directories.

### Extension Blocking

```json
"blocked_extensions": [".pst", ".ost"]
```

Blocked extensions are excluded from indexing regardless of other settings.

### Update Control

```json
"update_channel": "Stable",
"update_deferral_days": 30
```

Controls how and when updates are applied.

## Policy Sources

| Source | Description |
|--------|-------------|
| `MDM` | Mobile Device Management (Intune, etc.) |
| `GPO` | Group Policy Object |
| `Local Policy` | Local computer policy |
| `Registry` | Registry-based policy |

## Locked UI Behavior

When a setting is locked by policy:

1. **Control is disabled** - User cannot change the value
2. **Lock icon appears** - Visual indicator next to the control
3. **Tooltip shows reason** - Hovering reveals the policy source and reason
4. **InfoBar may appear** - For significant locks, an InfoBar explains the restriction

Example:
```
┌─────────────────────────────────────────────────────────────┐
│ [Toggle]  Private mode                         🔒           │
│ Hides content snippets and reduces stored text.            │
│ Managed by your organization - Required for compliance.     │
└─────────────────────────────────────────────────────────────┘
```

## Admin (Managed) Pages

When `managed: true`, additional pages appear in settings:

### Policy Overview
- Shows managed status
- Link to view all enforced policies

### Enforced Restrictions
- Lists all locked settings
- Shows lock reasons and sources

### Deployment & Updates
- Update channel selection (may be locked)
- Update deferral configuration
- Export policy file for deployment

## Policy Loading

Policies are loaded from:

1. `%LocalAppData%\Vantus\policies.json` (user policy)
2. `%ProgramData%\Vantus\policies.json` (machine policy)
3. Registry-based policies
4. MDM-provisioned policies

Policies are merged with user settings at startup. Locked settings take precedence.

## PowerShell Commands for Policy Management

### Export Current Policy
```powershell
Get-VantusPolicy -ExportTo policy.json
```

### Apply Policy File
```powershell
Set-VantusPolicy -Path policy.json
```

### View Active Locks
```powershell
Get-VantusPolicyLocks
```

### Remove All Policies
```powershell
Remove-VantusPolicy
```

## Common Policy Scenarios

### Scenario 1: Require Encryption
```json
{
  "managed": true,
  "locks": [
    {
      "setting_id": "privacy.encrypt_index_db",
      "locked_value": true,
      "reason": "Encryption required by corporate policy.",
      "source": "GPO"
    }
  ]
}
```

### Scenario 2: Disable Cloud Features
```json
{
  "managed": true,
  "locks": [
    {
      "setting_id": "general.analytics_anonymous",
      "locked_value": false,
      "reason": "Telemetry collection disabled.",
      "source": "MDM"
    },
    {
      "setting_id": "search.show_content_snippets",
      "locked_value": false,
      "reason": "Content previews disabled for data protection.",
      "source": "MDM"
    }
  ]
}
```

### Scenario 3: Restrict Locations
```json
{
  "managed": true,
  "allowed_locations": [
    "C:\\Users\\*\\Documents",
    "C:\\Users\\*\\Desktop",
    "C:\\Work"
  ],
  "blocked_extensions": [".pst", ".ost", ".bak"]
}
```

## Policy Schema Reference

### Top-Level Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `managed` | boolean | Yes | Enables managed mode |
| `schema_version` | string | Yes | Policy schema version |
| `locks` | array | No | List of setting locks |
| `allowed_locations` | array | No | Allowed index locations |
| `blocked_extensions` | array | No | Blocked file extensions |
| `update_channel` | string | No | Update channel restriction |
| `update_deferral_days` | number | No | Update deferral period |

### Lock Object Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `setting_id` | string | Yes | Setting identifier |
| `locked_value` | any | Yes | Enforced value |
| `reason` | string | Yes | Explanation for lock |
| `source` | string | Yes | Policy source identifier |

## Troubleshooting

### Policy Not Applying
1. Check policy file syntax (validate JSON)
2. Verify file is in correct location
3. Restart Vantus after applying policy
4. Check event logs for policy loading errors

### Settings Still Editable
1. Verify `managed: true` is set
2. Check if setting supports locking (`policy_lockable: true` in schema)
3. Confirm lock includes correct `setting_id`

### Policy Conflicts
When multiple policies apply, the most restrictive lock takes precedence. Last-loaded policy wins for conflicting values.

## Programmatic Policy Access

```csharp
var policyEngine = new PolicyEngine(dataPath);
await policyEngine.InitializeAsync();

var state = policyEngine.GetCurrentState();
var locks = policyEngine.GetAllLocks();
var isLocked = policyEngine.IsSettingLocked("privacy.encrypt_index_db");
```

## Security Considerations

1. Policy files should be protected from unauthorized modification
2. Locked settings cannot be overridden by users
3. Policy reasons are visible to users (for transparency)
4. Audit logging tracks policy changes
5. Sensitive values in policies should be encrypted where supported
