# Vantus File Indexer - Master Test Plan

## 1. Installation & Deployment
| ID | Test Case | Steps | Expected Result | Status |
| :--- | :--- | :--- | :--- | :--- |
| **INST-01** | Clean Install | Run `VantusSetup.msi` on fresh VM. | Installs to `%ProgramFiles%`, Shortcut on Desktop/Start. | 🟢 Pass |
| **INST-02** | Upgrade | Install v1.0, then Install v1.1. | Old version removed, settings preserved. | ⚪ Untested |
| **INST-03** | Silent Install | `msiexec /i VantusSetup.msi /qn` | Installs without UI. | 🟢 Pass |
| **INST-04** | Uninstall | Remove via Settings > Apps. | Files removed, User Data preserved (optional). | 🟢 Pass |

## 2. Settings & Configuration
| ID | Test Case | Steps | Expected Result | Status |
| :--- | :--- | :--- | :--- | :--- |
| **CFG-01** | Persistence | Change "App Theme" to "Dark". Restart App. | Theme remains "Dark". | 🟢 Pass |
| **CFG-02** | Policy Lock | Add `policies.json` with `{"general.theme": {"locked": true}}`. | Theme dropdown disabled in UI. | 🟢 Pass |
| **CFG-03** | Preset Switch | Change Preset to "Pro". | Default values update (e.g. CPU Limit). | 🟢 Pass |
| **CFG-04** | Invalid Data | Manually corrupt `settings.json`. | App starts with defaults (Resilience). | 🟢 Pass |

## 3. UI Navigation
| ID | Test Case | Steps | Expected Result | Status |
| :--- | :--- | :--- | :--- | :--- |
| **NAV-01** | Category Nav | Click "Indexing". | Expands to show sub-pages. | 🟢 Pass |
| **NAV-02** | Page Load | Click "Status". | Right pane loads Settings UI. | 🟢 Pass |
| **NAV-03** | Search | Type in Shell Search Box. | (Currently) Nothing happens. | 🔴 Fail |

## 4. Core Engine (Planned)
| ID | Test Case | Steps | Expected Result | Status |
| :--- | :--- | :--- | :--- | :--- |
| **ENG-01** | File Detect | Add `test.pdf` to watched folder. | Log shows "File Detected". | 🔴 Fail |
| **ENG-02** | Text Extract | (As above). | Index contains text content. | 🔴 Fail |
| **ENG-03** | Search | Query "test". | `test.pdf` appears in results. | 🔴 Fail |
