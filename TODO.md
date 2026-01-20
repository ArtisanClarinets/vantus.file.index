# Vantus File Indexer - Production Readiness TODO

## Overview
This document provides a complete, file-by-file roadmap for implementing all features and bringing Vantus File Indexer to production readiness. The current state has basic UI scaffolding with non-functional search and incomplete backend services.

---

## Phase 1: Core Engine & Infrastructure (Backend)

### 1.1 Engine Service Implementation
**Priority: HIGH**

#### `Vantus.Engine/Services/EngineService.cs`
- [ ] **Implement real indexing status**: Replace hardcoded `GetIndexStatus` with actual index state from DB
- [ ] **Implement Pause/Resume indexing**: Wire to actual indexing pipeline
- [ ] **Implement SetComputePreference**: Wire to embedding service for hardware selection
- [ ] **Implement ReindexLocation**: Trigger file change monitoring for path
- [ ] **Implement RebuildIndex**: Clear and re-run full indexing

#### `Vantus.Engine/Services/Indexing/FileIndexerService.cs` - **CREATE**
- [ ] Create `FileIndexerService` to scan and index files
- [ ] Implement recursive directory scanning
- [ ] Implement content extraction integration
- [ ] Implement vector embedding generation
- [ ] Implement database persistence of file metadata
- [ ] Add progress reporting interface

#### `Vantus.Engine/Services/Indexing/FileMonitorService.cs` - **CREATE**
- [ ] Create `FileMonitorService` using `FileSystemWatcher`
- [ ] Implement change detection (create/modify/delete/move)
- [ ] Implement debouncing for rapid changes
- [ ] Implement incremental re-indexing on changes
- [ ] Handle permission errors gracefully

#### `Vantus.Engine/Services/Extraction/CompositeContentExtractor.cs`
- [ ] **Fix existing implementation**: Complete `CompositeContentExtractor`
- [ ] Implement Office document extraction (Word, Excel, PowerPoint)
- [ ] Implement PDF extraction
- [ ] Implement image OCR (Tesseract or Windows Vision Runtime)
- [ ] Implement code file extraction
- [ ] Implement archive extraction
- [ ] Implement email extraction (Outlook PST, MSG)

#### `Vantus.Engine/Services/AI/OnnxEmbeddingService.cs`
- [ ] **Fix existing implementation**: Complete `OnnxEmbeddingService`
- [ ] Integrate ONNX Runtime for embedding generation
- [ ] Implement sentence-transformers model loading
- [ ] Implement batch processing for efficiency
- [ ] Add GPU acceleration (CUDA/DirectML)

#### `Vantus.Engine/Services/Search/VectorSearchService.cs`
- [ ] **Fix existing implementation**: Complete `VectorSearchService`
- [ ] Implement vector similarity search
- [ ] Add hybrid search (keyword + vector)
- [ ] Implement result ranking
- [ ] Add pagination support

#### `Vantus.Engine/Data/VantusDbContext.cs` - **CREATE**
- [ ] Create Entity Framework Core DbContext
- [ ] Define FileEntity for file metadata
- [ ] Define IndexQueueEntity for pending files
- [ ] Configure SQLite provider
- [ ] Add migrations support

### 1.2 gRPC Proto Definition
**Priority: HIGH**

#### `Vantus.Engine/Protos/engine.proto` - **CREATE**
- [ ] Define Search RPC with query, limit, offset
- [ ] Define GetIndexStatus RPC
- [ ] Define Pause/Resume indexing RPCs
- [ ] Define SetComputePreference RPC
- [ ] Define ReindexLocation RPC
- [ ] Define RebuildIndex RPC
- [ ] Define TestExtraction RPC

---

## Phase 2: App UI - Dashboard

### 2.1 Dashboard ViewModel Enhancement
**Priority: MEDIUM**

#### `Vantus.App/ViewModels/DashboardViewModel.cs`
- [ ] Add `IndexStatus` real-time updates via gRPC streaming
- [ ] Implement manual refresh command
- [ ] Add pause/resume commands
- [ ] Add rebuild index command
- [ ] Implement status polling with configurable interval
- [ ] Add error handling and user feedback

### 2.2 Dashboard Page Enhancement
**Priority: MEDIUM**

#### `Vantus.App/Views/DashboardPage.xaml`
- [ ] Add file count statistics (indexed, pending, failed)
- [ ] Add indexing activity indicator
- [ ] Add pause/resume button
- [ ] Add rebuild index button
- [ ] Add last successful scan timestamp
- [ ] Add current scan location with progress
- [ ] Implement visual polish with animations

---

## Phase 3: App UI - Search

### 3.1 Search ViewModel Enhancement
**Priority: HIGH**

#### `Vantus.App/ViewModels/SearchViewModel.cs`
- [ ] Wire search to actual `IEngineClient.SearchAsync()`
- [ ] Add debouncing for search input
- [ ] Add search result selection
- [ ] Add "open file" command
- [ ] Add "open file location" command
- [ ] Add copy path to clipboard command
- [ ] Add search result filtering (by type, date, etc.)
- [ ] Implement search suggestions/autocomplete
- [ ] Add loading states and error handling

### 3.2 Search Page Enhancement
**Priority: HIGH**

#### `Vantus.App/Views/SearchPage.xaml`
- [ ] Add filter panel (file type, date range, size)
- [ ] Add result preview panel (content snippet)
- [ ] Add sorting options (relevance, date, name)
- [ ] Implement result cards with icons based on file type
- [ ] Add keyboard navigation
- [ ] Add "no results" empty state with suggestions
- [ ] Add search history
- [ ] Implement responsive layout

---

## Phase 4: App UI - Settings

### 4.1 Settings PageViewModel Enhancements
**Priority: MEDIUM**

#### `Vantus.App/ViewModels/SettingsPageViewModel.cs`
- [ ] Add settings categories to navigation
- [ ] Implement full settings definitions from JSON
- [ ] Add setting change notifications
- [ ] Implement preset application
- [ ] Add settings search within page
- [ ] Add unsaved changes indicator
- [ ] Implement settings export/import

#### `Vantus.App/ViewModels/SettingsPageViewModel.cs` - **ADD NEW CONTROL TYPES**
- [ ] Add `TextBoxSettingViewModel` for string values
- [ ] Add `PathPickerSettingViewModel` for folder/file paths
- [ ] Add `ColorPickerSettingViewModel` for color settings
- [ ] Add `SliderSettingViewModel` with proper formatting
- [ ] Add `RadioGroupSettingViewModel` for mutually exclusive options
- [ ] Add `NumericUpDownSettingViewModel` for integer values

### 4.2 Settings Page XAML Enhancements
**Priority: MEDIUM**

#### `Vantus.App/Views/SettingsPage.xaml`
- [ ] Add setting category headers
- [ ] Implement proper DataTemplate selectors
- [ ] Add helper text with icons
- [ ] Add validation error display
- [ ] Add restart required indicator
- [ ] Implement grouped settings sections
- [ ] Add scroll indicator for long pages
- [ ] Implement smooth animations on setting changes

### 4.3 Theme and Language Implementation
**Priority: HIGH**

#### `Vantus.App/App.xaml.cs`
- [ ] **Implement theme switching**: Wire `general.theme` setting to WPF UI theme
- [ ] **Implement language switching**: Add localization support
- [ ] Add theme change notification
- [ ] Implement restart prompt on theme change

#### `Vantus.App/Resources/` - **CREATE**
- [ ] Create localized string resources (en, es, fr, de, ja)
- [ ] Create theme resource dictionaries (Light, Dark)
- [ ] Implement dynamic theme switching

---

## Phase 5: App UI - Rules Editor

### 5.1 Rules Service Implementation
**Priority: MEDIUM**

#### `Vantus.Core/Services/RuleService.cs`
- [ ] **Fix existing implementation**: Complete `RuleService`
- [ ] Implement rule serialization/deserialization
- [ ] Implement rule validation
- [ ] Add rule execution engine (when files are indexed)
- [ ] Add rule simulation mode
- [ ] Implement rule priorities and ordering

### 5.2 Rules Editor ViewModel Enhancements
**Priority: MEDIUM**

#### `Vantus.App/ViewModels/RulesEditorViewModel.cs`
- [ ] Add rule condition operators validation
- [ ] Add action parameter validation
- [ ] Implement rule testing/simulation
- [ ] Add rule import/export
- [ ] Add rule enable/disable toggle visual feedback
- [ ] Implement rule duplication

### 5.3 Rules Editor Page Enhancements
**Priority: MEDIUM**

#### `Vantus.App/Views/RulesEditor.xaml`
- [ ] Add rule description editor
- [ ] Add condition builder with better UX
- [ ] Add action parameter helper tooltips
- [ ] Add rule priority control
- [ ] Implement rule preview
- [ ] Add keyboard shortcuts for common actions
- [ ] Implement drag-and-drop reordering

---

## Phase 6: Navigation and Shell

### 6.1 MainWindow Enhancements
**Priority: MEDIUM**

#### `Vantus.App/MainWindow.xaml`
- [ ] **Add application icon**: Use `Logo.png` converted to proper icon format
- [ ] Add window title branding
- [ ] Add status bar with connection state
- [ ] Add global search box in navigation
- [ ] Implement proper window sizing/positioning persistence

#### `Vantus.App/MainWindow.xaml.cs`
- [ ] Implement window state persistence
- [ ] Add global keyboard shortcuts
- [ ] Implement about dialog
- [ ] Add help/accessibility features
- [ ] Implement proper cleanup on close

### 6.2 MainWindowViewModel Enhancements
**Priority: MEDIUM**

#### `Vantus.App/ViewModels/MainWindowViewModel.cs`
- [ ] Implement navigation history
- [ ] Add recent pages navigation
- [ ] Implement breadcrumb navigation
- [ ] Add settings page hierarchy population

---

## Phase 7: Engine Communication

### 7.1 GrpcEngineClient Implementation
**Priority: HIGH**

#### `Vantus.App/Services/GrpcEngineClient.cs`
- [ ] **Fix hardcoded localhost**: Make engine endpoint configurable
- [ ] Add connection retry logic
- [ ] Add connection state monitoring
- [ ] Implement graceful degradation when engine offline
- [ ] Add timeout configuration
- [ ] Implement gRPC channel health checking

### 7.2 Engine Service Management
**Priority: HIGH**

#### `Vantus.App/Services/EngineLifecycleManager.cs` - **CREATE**
- [ ] Create service to manage engine process
- [ ] Implement automatic engine startup
- [ ] Implement graceful shutdown
- [ ] Add engine crash recovery
- [ ] Implement port conflict detection
- [ ] Add engine log viewing

### 7.3 StubEngineClient for Development
**Priority: MEDIUM**

#### `Vantus.App/Services/StubEngineClient.cs` - **CREATE**
- [ ] Create `StubEngineClient` for offline development
- [ ] Implement mock search with sample data
- [ ] Implement mock indexing status
- [ ] Add UI toggle to switch between real/stub engine

---

## Phase 8: Settings Definitions Complete

### 8.1 Complete Settings Definitions
**Priority: MEDIUM**

#### `Vantus.App/settings_definitions.json`
- [ ] **Add settings for all defined pages**:
  - [ ] `general.startup` (launch on startup, tray icon)
  - [ ] `general.modes` (presets: personal, pro, enterprise)
  - [ ] `general.power` (battery saver, background indexing)
  - [ ] `general.data` (cache location, data directory)
  - [ ] `locations.included` (indexed paths)
  - [ ] `locations.exclusions` (excluded paths)
  - [ ] `locations.policy` (location policies)
  - [ ] `indexing.change` (change detection mode)
  - [ ] `indexing.performance` (CPU, memory limits)
  - [ ] `indexing.limits` (file size, content limits)
  - [ ] `ai.runtime` (hardware selection)
  - [ ] `ai.models` (model selection)
  - [ ] `ai.quality` (embedding quality settings)
  - [ ] Extraction settings for all file types
  - [ ] Organize mode and safety settings
  - [ ] Tagging and taxonomy settings
  - [ ] Search settings
  - [ ] Windows integration settings
  - [ ] Privacy and security settings
  - [ ] Compliance settings
  - [ ] Notification settings
  - [ ] Storage and maintenance settings
  - [ ] Diagnostics settings

### 8.2 Settings UI Pages
**Priority: MEDIUM**

#### `Vantus.App/Views/Settings/` - **CREATE MULTIPLE PAGES**
- [ ] Create dedicated XAML pages for each settings category
- [ ] Create specialized controls for:
  - [ ] Path picker control for locations
  - [ ] File type selector
  - [ ] Schedule picker for indexing
  - [ ] Rule condition builder UI
  - [ ] Tag editor
  - [ ] Policy viewer

---

## Phase 9: Branding and Installer

### 9.1 Application Icon and Branding
**Priority: HIGH**

#### `Vantus.App/Assets/` - **CREATE**
- [ ] Convert `Logo.png` to proper sizes:
  - [ ] `Resources/app.ico` (256x256, with multiple sizes)
  - [ ] `Resources/logo-16.png` (16x16)
  - [ ] `Resources/logo-32.png` (32x32)
  - [ ] `Resources/logo-48.png` (48x48)
  - [ ] `Resources/logo-256.png` (256x256)
- [ ] Create splash screen image
- [ ] Create about dialog logo

#### `Vantus.App/App.xaml`
- [ ] Set application icon reference
- [ ] Configure splash screen

### 9.2 Installer Enhancements
**Priority: HIGH**

#### `Vantus.Installer/Setup/Logo.ico` - **CREATE**
- [ ] Create proper installer icon from Logo.png

#### `Vantus.Installer/Build-Installer.ps1`
- [ ] **Add version bump logic**: Auto-increment version numbers
- [ ] **Add icon embedding**: Embed Logo.ico into installer
- [ ] **Add branding**: Apply visual customization
- [ ] **Add certificate signing**: Support for code signing
- [ ] Add pre-requisite checks (.NET runtime)
- [ ] Add post-install configuration

#### `Vantus.Installer/Package.wxs`
- [ ] **Add application icon**: Reference Logo.ico for Add/Remove Programs
- [ ] Add splash screen configuration
- [ ] Configure installer UI theme
- [ ] Add EULA acceptance

#### `Vantus.Installer/Shortcuts.wxs`
- [ ] **Add desktop icon with custom icon**: Use Logo.ico for shortcut
- [ ] Add Start Menu hierarchy
- [ ] Add quick launch bar option
- [ ] Configure shortcut properties

#### `Vantus.Installer/License.rtf` - **CREATE**
- [ ] Create professional EULA document
- [ ] Add privacy policy reference
- [ ] Add third-party licenses

### 9.3 Inno Setup Script
**Priority: MEDIUM**

#### `installer.iss`
- [ ] **Update SetupIconFile**: Point to branded Logo.ico
- [ ] Add custom installer images (banner, dialog)
- [ ] Add pre-install checks
- [ ] Add post-install tasks (create shortcuts)
- [ ] Configure silent install options

---

## Phase 10: Polish and Production Readiness

### 10.1 Error Handling and Feedback
**Priority: HIGH**

#### Global Error Handler - **CREATE**
- [ ] `Vantus.App/Services/ErrorHandlingService.cs`
- [ ] Implement global application exception handler
- [ ] Add error reporting dialog
- [ ] Implement crash dump generation
- [ ] Add telemetry for error tracking (opt-in)

#### `Vantus.App/MainWindow.xaml.cs`
- [ ] Add connection status indicator
- [ ] Add indexing status notification
- [ ] Implement notification toast system

### 10.2 Accessibility
**Priority: MEDIUM**

- [ ] Add keyboard navigation throughout
- [ ] Implement proper focus management
- [ ] Add ARIA labels for screen readers
- [ ] Ensure color contrast compliance (WCAG 2.1 AA)
- [ ] Add keyboard shortcuts for power users
- [ ] Implement high contrast theme support

### 10.3 Performance
**Priority: MEDIUM**

- [ ] Optimize startup time (lazy load settings pages)
- [ ] Implement virtualized list for search results
- [ ] Add progress indicators for long operations
- [ ] Implement background work with cancellation
- [ ] Add memory usage monitoring UI

### 10.4 Localization
**Priority: MEDIUM**

#### `Vantus.App/Resources/Strings/` - **CREATE**
- [ ] Create `Strings.en.resx` (English - default)
- [ ] Create `Strings.es.resx` (Spanish)
- [ ] Create `Strings.fr.resx` (French)
- [ ] Create `Strings.de.resx` (German)
- [ ] Create `Strings.ja.resx` (Japanese)
- [ ] Implement localized date/number formats
- [ ] Add RTL support consideration

### 10.5 Testing
**Priority: HIGH**

#### `Vantus.Tests/` - **EXPAND**
- [ ] Add integration tests for settings persistence
- [ ] Add tests for rule service
- [ ] Add tests for settings schema
- [ ] Add UI tests (Appium/WinAppDriver)
- [ ] Add performance benchmarks
- [ ] Implement CI/CD pipeline

### 10.6 Documentation
**Priority: MEDIUM**

- [ ] Create user guide (docs/user-guide.md)
- [ ] Create administrator guide (docs/admin-guide.md)
- [ ] Create API documentation (docs/api.md)
- [ ] Add inline code comments
- [ ] Create troubleshooting guide

---

## File Manifest - Files to CREATE

### New C# Files
```
Vantus.Engine/
├── Services/
│   ├── Indexing/
│   │   ├── FileIndexerService.cs
│   │   └── FileMonitorService.cs
│   └── Extraction/
│       └── ContentExtractorImpl.cs
├── Data/
│   └── VantusDbContext.cs
└── Protos/
    └── engine.proto

Vantus.App/
├── Services/
│   ├── EngineLifecycleManager.cs
│   ├── StubEngineClient.cs
│   ├── ThemeService.cs
│   ├── LocalizationService.cs
│   └── NotificationService.cs
├── Controls/
│   ├── PathPicker.xaml.cs
│   ├── FileTypeSelector.xaml.cs
│   ├── SchedulePicker.xaml.cs
│   ├── TagEditor.xaml.cs
│   └── Settings/
│       ├── TextBoxSettingControl.xaml.cs
│       ├── PathPickerSettingControl.xaml.cs
│       ├── ColorPickerSettingControl.xaml.cs
│       └── RadioGroupSettingControl.xaml.cs
├── Views/Settings/
│   ├── LocationsPage.xaml.cs
│   ├── IndexingPage.xaml.cs
│   ├── AIPage.xaml.cs
│   ├── ExtractionPage.xaml.cs
│   ├── OrganizePage.xaml.cs
│   ├── TagsPage.xaml.cs
│   ├── SearchPage.xaml.cs
│   ├── WindowsPage.xaml.cs
│   ├── PrivacyPage.xaml.cs
│   ├── CompliancePage.xaml.cs
│   ├── NotificationsPage.xaml.cs
│   ├── StoragePage.xaml.cs
│   ├── DiagnosticsPage.xaml.cs
│   └── AboutPage.xaml.cs
├── Assets/
│   ├── Resources/
│   │   ├── app.ico
│   │   ├── logo-16.png
│   │   ├── logo-32.png
│   │   ├── logo-48.png
│   │   └── logo-256.png
│   └── splashscreen.png
└── Resources/
    └── Strings/
        ├── Strings.en.resx
        ├── Strings.es.resx
        ├── Strings.fr.resx
        ├── Strings.de.resx
        └── Strings.ja.resx
```

### New Configuration Files
```
Vantus.Installer/
├── Setup/
│   ├── Logo.ico
│   ├── banner.bmp
│   ├── dialog.bmp
│   └── License.rtf
└── Config/
    └── installer-config.json
```

### New Documentation Files
```
docs/
├── user-guide.md
├── admin-guide.md
├── troubleshooting.md
└── api/
    └── settings-schema.md
```

---

## Implementation Priority Order

### Sprint 1: Core Engine (Week 1)
1. FileIndexerService implementation
2. FileMonitorService implementation
3. VantusDbContext and database setup
4. CompositeContentExtractor completion
5. OnnxEmbeddingService completion
6. VectorSearchService completion
7. EngineService wiring

### Sprint 2: Search Experience (Week 2)
1. SearchViewModel real gRPC integration
2. SearchPage enhancement with filters
3. Result preview panel
4. Search result actions (open, copy path)
5. Search suggestions

### Sprint 3: Settings Framework (Week 3)
1. Complete settings_definitions.json
2. All settings control types
3. Theme switching implementation
4. Language switching framework
5. Settings pages for all categories

### Sprint 4: Dashboard and Rules (Week 4)
1. Dashboard real-time updates
2. Dashboard controls (pause/resume/rebuild)
3. RulesService completion
4. RulesEditor enhancements
5. Rule validation and simulation

### Sprint 5: Polish (Week 5)
1. Engine lifecycle management
2. Error handling and recovery
3. Accessibility improvements
4. Performance optimization
5. Localization

### Sprint 6: Installer and Branding (Week 6)
1. Icon conversion and embedding
2. Installer branding
3. Shortcut creation
4. Code signing setup
5. Final testing

---

## Estimated Effort

| Phase | Files to Modify | Files to Create | Complexity |
|-------|-----------------|-----------------|------------|
| 1. Core Engine | 5 | 8 | High |
| 2. Dashboard | 2 | 0 | Medium |
| 3. Search | 2 | 0 | Medium |
| 4. Settings | 3 | 12 | High |
| 5. Rules | 2 | 0 | Medium |
| 6. Shell | 3 | 0 | Medium |
| 7. Engine Comm | 1 | 2 | Medium |
| 8. Settings Full | 1 | 20 | High |
| 9. Branding | 4 | 8 | Medium |
| 10. Polish | 5 | 8 | Medium |
| **Total** | **28** | **58** | **~6 weeks** |

---

## Success Criteria

- [ ] Search returns real results from indexed files
- [ ] Dashboard shows real indexing status
- [ ] All settings categories are functional
- [ ] Theme switching works without restart
- [ ] Language switching works
- [ ] Rules editor creates and saves rules
- [ ] Installer creates branded shortcuts with Logo.png
- [ ] Application is stable (no crashes)
- [ ] UI is accessible
- [ ] Performance is acceptable (startup < 5s)
