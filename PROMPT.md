# PROMPT.md — Vantus File Indexer Production Readiness Master Prompt

You are building the **Vantus File Indexer** production-ready Windows desktop application. This is a comprehensive task requiring implementation of a full indexing engine, production-grade WPF UI, branded installer, and all supporting infrastructure.

## Executive Summary

The Vantus File Indexer is a Windows desktop app that:
1. Indexes files on the user's system using a background engine service
2. Provides semantic search via vector embeddings
3. Offers automation rules for file organization
4. Features a professional WPF settings experience

**Your mission**: Implement ALL missing functionality to achieve production readiness in a single comprehensive implementation sweep.

---

## 0) Critical Success Criteria

The implementation is complete ONLY when:

### Functionality
- [ ] Search actually returns results from indexed files (not mock data)
- [ ] Dashboard shows real indexing status with live updates
- [ ] All 18 settings categories from settings_definitions.json are functional
- [ ] Theme switching (Light/Dark/System) works without restart
- [ ] Language switching works with at least EN supported
- [ ] Rules editor creates, validates, and saves rules
- [ ] Engine auto-starts when app opens
- [ ] App recovers gracefully when engine is unavailable

### Quality
- [ ] App compiles without errors
- [ ] All unit tests pass
- [ ] No crashes during normal operation
- [ ] Startup time < 5 seconds
- [ ] Memory usage < 200MB at idle
- [ ] UI is fully accessible (keyboard nav, screen reader support)

### Branding
- [ ] App shortcut uses Logo.png as icon
- [ ] Installer is branded with Logo.png styling
- [ ] Professional splash screen on startup
- [ ] Consistent visual design throughout

### Documentation
- [ ] User guide explains all features
- [ ] Admin guide covers enterprise deployment
- [ ] Code comments explain complex logic

---

## 1) Project Context & Architecture

### Solution Structure
```
Vantus.FileIndexer.sln
├── Vantus.Core/              # Shared models, services, interfaces
├── Vantus.App/               # WPF desktop application
├── Vantus.Engine/            # Background indexing engine (gRPC service)
├── Vantus.Tests/             # xUnit unit tests
├── Vantus.Installer/         # WiX MSI installer
└── docs/                     # Documentation
```

### Technology Stack (MANDATORY - NO Windows App SDK)

- **.NET 8** with C# 12
- **WPF** (Windows Presentation Foundation) - NOT WinUI 3, NOT Windows App SDK
- **Wpf.Ui** (Wpf.Ui) for modern controls library (version 3.x)
- **CommunityToolkit.Mvvm** for MVVM
- **Microsoft.Extensions.Hosting** for DI
- **gRPC** for IPC
- **Entity Framework Core** with SQLite
- **System.Text.Json** for serialization
- **xUnit** for testing
- **WiX Toolset 4** for MSI installer
- **ONNX Runtime** for ML inference

### IMPORTANT: WPF + Wpf.Ui Only

**ABSOLUTELY NO Windows App SDK or WinUI 3 allowed.**

The application must use:
- Pure WPF for the UI framework
- Wpf.Ui (Wpf.Ui library from NuGet) for modern controls
- All windows must inherit from `Window` not `FluentWindow` (unless using Wpf.Ui.FluentWindow)
- Navigation via `Frame` and `Page`, not NavigationView
- Use Wpf.Ui's `Navigation` control or custom navigation

### Current State Assessment

**Already Implemented (DO NOT REIMPLEMENT):**
- Basic WPF shell with navigation
- SearchPage XAML and basic ViewModel skeleton
- DashboardPage XAML and basic ViewModel skeleton
- RulesEditor XAML and ViewModel
- SettingsPage XAML and ViewModel with toggle/dropdown/slider support
- MainWindow navigation routing
- Basic SettingsSchema and SettingsStore
- Basic RuleService and AutomationRule model
- IEngineClient interface and EngineClientStub
- GrpcEngineClient (needs endpoint configuration)
- settings_definitions.json with partial definitions
- WiX installer infrastructure
- Wpf.Ui integration is already set up

**NOT Implemented (YOU MUST BUILD):**
- FileIndexerService (core indexing engine)
- FileMonitorService (file system watching)
- VantusDbContext (database layer)
- Complete content extraction (Office, PDF, images, etc.)
- OnnxEmbeddingService (vector embeddings)
- VectorSearchService (semantic search)
- EngineService gRPC implementations
- Engine lifecycle management (auto-start, crash recovery)
- Theme switching service
- Localization framework
- All settings pages (18 categories × multiple pages)
- Additional setting control types (path picker, color picker, etc.)
- Icon assets from Logo.png
- Branded installer

---

## 2) Detailed Implementation Requirements

### PHASE 1: Core Engine Implementation

#### 1.1 FileIndexerService (CREATE)
**File**: `Vantus.Engine/Services/Indexing/FileIndexerService.cs`

Implement the core indexing service that:
- Scans configured directories recursively
- Detects file changes (new, modified, deleted)
- Extracts text content from files
- Generates vector embeddings
- Persists file metadata to database
- Reports progress via events
- Respects CPU/memory limits
- Handles permission errors gracefully

Key methods:
```csharp
public class FileIndexerService
{
    event EventHandler<IndexingProgressEventArgs> ProgressChanged;
    event EventHandler<FileIndexedEventArgs> FileIndexed;
    
    Task StartIndexingAsync(IEnumerable<string> paths);
    Task StopIndexingAsync();
    Task ReindexPathAsync(string path);
    Task RebuildIndexAsync();
    bool IsIndexing { get; }
    int FilesIndexed { get; }
    int FilesRemaining { get; }
}
```

#### 1.2 FileMonitorService (CREATE)
**File**: `Vantus.Engine/Services/Indexing/FileMonitorService.cs`

Implement file system monitoring that:
- Uses FileSystemWatcher for real-time change detection
- Debounces rapid changes (configurable, default 5 seconds)
- Handles folder renames/moves recursively
- Automatically triggers re-indexing for changed files
- Persists watch paths across restarts
- Handles permission errors gracefully

Key methods:
```csharp
public class FileMonitorService
{
    Task StartMonitoring(string path);
    Task StopMonitoring(string path);
    IEnumerable<string> GetWatchedPaths();
    bool IsPathWatched(string path);
}
```

#### 1.3 Content Extraction (COMPLETE)
**Files**:
- `Vantus.Engine/Services/Extraction/OfficeExtractor.cs`
- `Vantus.Engine/Services/Extraction/PdfExtractor.cs`
- `Vantus.Engine/Services/Extraction/CompositeContentExtractor.cs`

Complete implementation for:
- Word documents (.docx, .doc)
- Excel spreadsheets (.xlsx, .xls)
- PowerPoint presentations (.pptx, .ppt)
- PDF files (using PdfPig or similar)
- Plain text files
- Code files (syntax extraction)
- Markdown files
- Email files (.msg, .eml)

#### 1.4 OnnxEmbeddingService (COMPLETE)
**File**: `Vantus.Engine/Services/AI/OnnxEmbeddingService.cs`

Complete implementation that:
- Loads sentence-transformers model via ONNX
- Generates embeddings for file content
- Supports GPU acceleration (CUDA/DirectML)
- Batches requests for efficiency
- Caches embeddings for re-use
- Handles model loading errors gracefully

#### 1.5 VectorSearchService (COMPLETE)
**File**: `Vantus.Engine/Services/Search/VectorSearchService.cs`

Complete implementation that:
- Performs vector similarity search
- Supports hybrid search (keyword + vector)
- Ranks results by relevance
- Supports pagination
- Filters by file type, date, path
- Returns snippets with highlights

#### 1.6 VantusDbContext (CREATE)
**File**: `Vantus.Engine/Data/VantusDbContext.cs`

Create Entity Framework Core DbContext with:
```csharp
public class VantusDbContext : DbContext
{
    public DbSet<FileEntity> Files { get; set; }
    public DbSet<EmbeddingEntity> Embeddings { get; set; }
    public DbSet<IndexQueueItem> IndexQueue { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options);
    protected override void OnModelCreating(ModelBuilder model);
}
```

#### 1.7 EngineService gRPC Implementation
**File**: `Vantus.Engine/Services/EngineService.cs`

Complete all gRPC methods with real implementation:
- `Search`: Call VectorSearchService
- `GetIndexStatus`: Return real status from indexer
- `PauseIndexing`: Pause FileIndexerService
- `ResumeIndexing`: Resume FileIndexerService
- `SetComputePreference`: Configure embedding hardware
- `ReindexLocation`: Trigger re-indexing
- `RebuildIndex`: Clear and rebuild
- `TestExtraction`: Return real extraction result

---

### PHASE 2: Search Functionality

#### 2.1 SearchViewModel Enhancement
**File**: `Vantus.App/ViewModels/SearchViewModel.cs`

Enhance to:
- Wire to actual `IEngineClient.SearchAsync()`
- Add debouncing (300ms) for search input
- Add result selection
- Add "open file" command (uses Process.Start)
- Add "open file location" command
- Add "copy path" command
- Add filtering (type, date, size)
- Add search suggestions from recent searches
- Add loading/error states
- Handle engine disconnection gracefully

#### 2.2 SearchPage Enhancement
**File**: `Vantus.App/Views/SearchPage.xaml`

Enhance to include:
- Filter panel (file type dropdown, date range picker, size slider)
- Sort options (relevance, date, name, size)
- Result preview panel with content snippet
- File type icons based on extension
- Keyboard navigation (arrows, enter, escape)
- "No results" state with suggestions
- Search history dropdown
- Responsive layout for different window sizes

Use Wpf.Ui controls:
- `ui:TextBox` for search input
- `ui:Button` for actions
- `ui:Card` for result items
- `ui:ProgressRing` for loading state
- `ui:SymbolIcon` for icons

---

### PHASE 3: Dashboard

#### 3.1 DashboardViewModel Enhancement
**File**: `Vantus.App/ViewModels/DashboardViewModel.cs`

Enhance to:
- Poll `IEngineClient.GetIndexStatusAsync()` every 2 seconds
- Add manual refresh command
- Add pause/resume indexing commands
- Add rebuild index command
- Track and display: indexed files count, pending count, failed count
- Show current scan location with progress
- Show last successful scan timestamp
- Display active compute device (CPU/NPU/iGPU)

#### 3.2 DashboardPage Enhancement
**File**: `Vantus.App/Views/DashboardPage.xaml`

Enhance to include:
- File count statistics cards
- Indexing activity indicator (animated)
- Pause/Resume button
- Rebuild Index button with confirmation dialog
- Current scan location with progress bar
- Last successful scan timestamp
- Compute device indicator
- Visual polish with animations

Use Wpf.Ui controls:
- `ui:CardControl` for statistics
- `ui:ProgressBar` for progress
- `ui:Button` for actions
- `ui:SymbolIcon` for status indicators

---

### PHASE 4: Settings Framework Complete

#### 4.1 Complete settings_definitions.json
**File**: `Vantus.App/settings_definitions.json`

Add settings for ALL defined pages:

**General** (already has 5, add more):
- `general.startup`: Launch on startup, minimize to tray, show notification
- `general.modes`: Active preset selector
- `general.power`: Battery saver mode, background indexing
- `general.data`: Cache location, data directory, clear data

**Workspaces**:
- `workspaces.switcher`: Workspace dropdown, switch command
- `workspaces.defaults`: Default workspace configuration
- `workspaces.import`: Import workspace from file

**Locations**:
- `locations.included`: List of indexed paths (path picker)
- `locations.exclusions`: List of excluded paths (path picker)
- `locations.policy`: Location-specific policies

**Indexing**:
- `indexing.change`: Change detection mode (realtime/scheduled/manual)
- `indexing.performance`: CPU limit slider, memory limit
- `indexing.limits`: Max file size, max content length

**AI Models**:
- `ai.runtime`: Hardware preference (Auto/NPU/iGPU/CPU)
- `ai.models`: Model selection dropdown
- `ai.quality`: Embedding quality (speed/quality tradeoff)

**Extraction** (all toggle switches):
- `extraction.docs`: Enable document extraction
- `extraction.images`: Enable image extraction (OCR)
- `extraction.code`: Enable code extraction
- `extraction.archives`: Enable archive extraction
- `extraction.media`: Enable media metadata extraction
- `extraction.email`: Enable email extraction

**Organize & Automations**:
- `organize.mode`: Organizing mode (manual/automatic/review)
- `organize.rules`: Rules management (link to Rules page)
- `organize.safety`: Safety level, undo window
- `organize.review`: Review queue management

**Tags & Taxonomy**:
- `tags.behavior`: Auto-tagging, tag suggestions
- `tags.vocab`: Custom vocabulary
- `tags.windows`: Windows file properties sync

**Partners**:
- `partners.directory`: Partner folder paths
- `partners.matching`: Matching logic configuration
- `partners.policies`: Partner-specific rules

**Search**:
- `search.mode`: Search mode (hybrid/vector/keyword)
- `search.results`: Results per page, preview length
- `search.ranking`: Ranking weights configuration

**Windows Integration**:
- `windows.surfaces`: Explorer columns, preview pane
- `windows.context`: Context menu actions
- `windows.search`: Windows Search integration

**Privacy & Security**:
- `privacy.storage`: Encryption toggle, storage location
- `privacy.access`: Access control settings
- `privacy.sensitive`: Sensitive data patterns
- `privacy.keys`: Key management

**Compliance & Audit**:
- `compliance.audit`: Audit log enabled
- `compliance.retention`: Retention policies
- `compliance.reports`: Report generation

**Notifications**:
- `notifications.alerts`: Alert settings
- `notifications.quiet`: Quiet hours

**Storage & Maintenance**:
- `storage.locations`: Storage paths
- `storage.cache`: Cache size limit, clear cache
- `storage.maintenance`: Maintenance schedule
- `storage.backup`: Backup settings

**Diagnostics**:
- `diagnostics.status`: System status display
- `diagnostics.trace`: Performance tracing
- `diagnostics.export`: Export diagnostics
- `diagnostics.advanced`: Advanced settings

**Admin (Managed)**:
- `admin.policy`: Policy overview (read-only)
- `admin.restrictions`: Enforced restrictions view
- `admin.deployment`: Deployment configuration

**About**:
- `about.version`: Version info (read-only)
- `about.reset`: Reset to defaults button

#### 4.2 Additional Setting Control Types
**Files**: `Vantus.App/ViewModels/SettingsPageViewModel.cs`

Add new ViewModel classes:
- `TextBoxSettingViewModel`: For string values
- `PathPickerSettingViewModel`: For folder/file paths with picker button
- `ColorPickerSettingViewModel`: For color settings
- `NumericUpDownSettingViewModel`: For integer values with stepper
- `RadioGroupSettingViewModel`: For mutually exclusive options
- `MultiSelectSettingViewModel`: For multiple selection
- `ButtonSettingViewModel`: For action buttons
- `InfoSettingViewModel`: For read-only information display

#### 4.3 Theme Switching Implementation
**File**: `Vantus.App/Services/ThemeService.cs` - CREATE

Implement theme service that:
- Reads `general.theme` setting
- Applies Wpf.Ui theme to Application.Resources
- Supports Light, Dark themes
- Persists theme preference
- Handles theme change without restart

Wpf.Ui theme configuration:
```csharp
var theme = Wpf.Ui.Appearance.Theme.GetAppTheme();
Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
```

**File**: `Vantus.App/App.xaml.cs`

Modify to wire theme service:
```csharp
var themeService = Services.GetRequiredService<ThemeService>();
await themeService.ApplyThemeAsync();
```

#### 4.4 Localization Framework
**File**: `Vantus.App/Services/LocalizationService.cs` - CREATE

Implement localization that:
- Reads `general.language` setting
- Loads strings from .resx files
- Supports EN, ES, FR, DE, JA
- Falls back to English

**Files**: `Vantus.App/Resources/Strings/`
- `Strings.en.resx` (default)
- `Strings.es.resx`
- `Strings.fr.resx`
- `Strings.de.resx`
- `Strings.ja.resx`

---

### PHASE 5: Rules System Complete

#### 5.1 RuleService Enhancement
**File**: `Vantus.Core/Services/RuleService.cs`

Enhance to:
- Implement full serialization/deserialization
- Add rule validation (name required, conditions/actions valid)
- Add rule execution during file indexing
- Add rule simulation mode (preview without executing)
- Add rule priorities and ordering
- Support rule import/export

#### 5.2 RulesEditorViewModel Enhancement
**File**: `Vantus.App/ViewModels/RulesEditorViewModel.cs`

Enhance to:
- Add condition/operator validation
- Add action parameter validation
- Add rule simulation/test command
- Add rule import/export
- Add rule duplicate command
- Show validation errors inline

#### 5.3 RulesEditorPage Enhancement
**File**: `Vantus.App/Views/RulesEditor.xaml`

Enhance to include:
- Rule description field
- Better condition builder UI with operator explanations
- Action parameter help tooltips
- Rule priority up/down controls
- Rule preview panel
- Keyboard shortcuts (Ctrl+N new, Ctrl+S save, Delete remove)

Use Wpf.Ui controls:
- `ui:Card` for rule items
- `ui:Button` for actions
- `ui:TextBox` for inputs
- `ui:ComboBox` for selections
- `ui:SymbolIcon` for icons

---

### PHASE 6: Engine Communication

#### 6.1 GrpcEngineClient Enhancement
**File**: `Vantus.App/Services/GrpcEngineClient.cs`

Enhance to:
- Make engine endpoint configurable (appsettings.json)
- Add connection retry logic (3 attempts, 1s delay)
- Add connection state monitoring
- Implement graceful degradation when engine offline
- Add timeout configuration (30s default)
- Add health check endpoint

#### 6.2 EngineLifecycleManager (CREATE)
**File**: `Vantus.App/Services/EngineLifecycleManager.cs`

Create service that:
- Starts engine process on app launch
- Waits for engine to be ready (gRPC health check)
- Monitors engine health (heartbeat)
- Restarts engine on crash (max 3 retries in 5 min)
- Handles port conflicts
- Provides engine logs for diagnostics
- Implements graceful shutdown

#### 6.3 StubEngineClient (CREATE)
**File**: `Vantus.App/Services/StubEngineClient.cs`

Create stub implementation for offline development:
- Implements `IEngineClient` interface
- Returns mock search results
- Returns mock indexing status
- Toggle between stub and real engine via settings

---

### PHASE 7: Branded Installer

#### 7.1 Icon Assets (CREATE)
**Files**: `Vantus.App/Assets/Resources/`

Convert `Logo.png` to proper formats:
- `app.ico` - 256x256 icon with multiple sizes (16, 32, 48, 256)
- `logo-16.png` - 16x16 PNG
- `logo-32.png` - 32x32 PNG
- `logo-48.png` - 48x48 PNG
- `logo-256.png` - 256x256 PNG
- `splashscreen.png` - Branded splash screen (1240x600)

#### 7.2 Installer Branding
**File**: `Vantus.Installer/Setup/Logo.ico` - CREATE

Create installer icon from Logo.png

**File**: `Vantus.Installer/Build-Installer.ps1`

Enhance script:
```powershell
# Add these features:
# 1. Auto-increment version numbers
# 2. Embed Logo.ico for installer UI
# 3. Add banner.bmp and dialog.bmp for branded installer
# 4. Add code signing support
# 5. Add pre-requisite check for .NET runtime
# 6. Add post-install configuration
```

**File**: `Vantus.Installer/Package.wxs`

Enhance package:
```xml
<!-- Add splash screen and custom UI -->
<Property Id="ARPPRODUCTICON" Value="AppIcon" />
<Property Id="WIXISEXE" Value="yes" />
<!-- Add splash screen bitmap references -->
```

**File**: `Vantus.Installer/Shortcuts.wxs`

Enhance shortcuts:
```xml
<!-- Add desktop icon with custom icon -->
<Shortcut Id="DesktopShortcut"
          Name="Vantus File Indexer"
          Description="Smart file organization and search"
          Target="[INSTALLFOLDER]Vantus.App.exe"
          Icon="AppIcon" />
```

**File**: `Vantus.Installer/License.rtf` - CREATE

Create professional EULA document

#### 7.3 Application Icon Integration
**File**: `Vantus.App/App.xaml`

Configure application resources:
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ui:Wpf.Ui.Appearance.ThemeDictionary Theme="Dark" />
            <ui:Wpf.Ui.Controls />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

**File**: `Vantus.App/Properties/Resources.resx` - CREATE

Add application metadata:
- Application icon reference
- Application description
- Company name
- Version

**File**: `Vantus.App/App.xaml.cs`

Set window icon:
```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set window icon from Logo.png
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets\\Resources\\app.ico");
        if (File.Exists(iconPath))
        {
            this.MainWindow.Icon = new BitmapImage(new Uri(iconPath));
        }
    }
}
```

---

### PHASE 8: Polish and Production Readiness

#### 8.1 Error Handling
**File**: `Vantus.App/Services/ErrorHandlingService.cs` - CREATE

Implement:
- Global application exception handler
- Error reporting dialog with "Send Feedback" option
- Crash dump generation
- Telemetry for error tracking (opt-in)
- Error log file location display

**File**: `Vantus.App/App.xaml.cs`

Wire up error handling:
```csharp
AppDomain.CurrentDomain.UnhandledException += (s, e) => 
    errorHandler.HandleException((Exception)e.ExceptionObject);
DispatcherUnhandledException += (s, e) => 
    errorHandler.HandleException(e.Exception);
```

#### 8.2 Notification Service
**File**: `Vantus.App/Services/NotificationService.cs` - CREATE

Implement toast notifications for:
- Indexing complete
- Search complete
- Rule triggered
- Error occurred
- Update available

Use Windows Toast Notifications API (works with WPF)

#### 8.3 Accessibility
Ensure throughout the app:
- All interactive elements have keyboard access (Tab/Shift+Tab)
- Focus indicators visible
- Screen reader support (AutomationProperties)
- Color contrast meets WCAG 2.1 AA
- High contrast theme support
- Keyboard shortcuts documented

#### 8.4 Performance Optimization
- Lazy load settings pages (only when navigated to)
- Virtualize search results (ItemsControl virtualization)
- Background work with proper cancellation
- Memory monitoring UI
- Startup optimization (defer non-critical initialization)

#### 8.5 Additional Settings Pages
**Files**: Create dedicated pages for complex settings:

- `LocationsPage.xaml.cs` - Path picker for included/excluded locations
- `IndexingPage.xaml.cs` - Indexing configuration
- `AIPage.xaml.cs` - AI model configuration
- `ExtractionPage.xaml.cs` - Extraction settings
- `OrganizePage.xaml.cs` - Organization settings
- `TagsPage.xaml.cs` - Tagging configuration
- `PrivacyPage.xaml.cs` - Privacy settings
- `AboutPage.xaml.cs` - About dialog with version info

All pages should inherit from `Page` and use Wpf.Ui controls.

---

## 3) File Manifest

### Files to CREATE (58 total)

#### C# Files - Engine
```
Vantus.Engine/Services/Indexing/FileIndexerService.cs
Vantus.Engine/Services/Indexing/FileMonitorService.cs
Vantus.Engine/Services/Extraction/OfficeExtractor.cs (complete)
Vantus.Engine/Services/Extraction/PdfExtractor.cs (complete)
Vantus.Engine/Services/AI/OnnxEmbeddingService.cs (complete)
Vantus.Engine/Services/Search/VectorSearchService.cs (complete)
Vantus.Engine/Data/VantusDbContext.cs
Vantus.Engine/Protos/engine.proto
```

#### C# Files - App Services
```
Vantus.App/Services/EngineLifecycleManager.cs
Vantus.App/Services/StubEngineClient.cs
Vantus.App/Services/ThemeService.cs
Vantus.App/Services/LocalizationService.cs
Vantus.App/Services/NotificationService.cs
Vantus.App/Services/ErrorHandlingService.cs
```

#### C# Files - ViewModels
```
Vantus.App/ViewModels/PathPickerSettingViewModel.cs
Vantus.App/ViewModels/TextBoxSettingViewModel.cs
Vantus.App/ViewModels/ColorPickerSettingViewModel.cs
Vantus.App/ViewModels/NumericUpDownSettingViewModel.cs
Vantus.App/ViewModels/RadioGroupSettingViewModel.cs
Vantus.App/ViewModels/MultiSelectSettingViewModel.cs
Vantus.App/ViewModels/ButtonSettingViewModel.cs
Vantus.App/ViewModels/InfoSettingViewModel.cs
```

#### C# Files - Views (WPF Pages)
```
Vantus.App/Views/LocationsPage.xaml.cs
Vantus.App/Views/IndexingPage.xaml.cs
Vantus.App/Views/AIPage.xaml.cs
Vantus.App/Views/ExtractionPage.xaml.cs
Vantus.App/Views/OrganizePage.xaml.cs
Vantus.App/Views/TagsPage.xaml.cs
Vantus.App/Views/PrivacyPage.xaml.cs
Vantus.App/Views/AboutPage.xaml.cs
```

#### XAML Resources
```
Vantus.App/Assets/Resources/app.ico
Vantus.App/Assets/Resources/logo-16.png
Vantus.App/Assets/Resources/logo-32.png
Vantus.App/Assets/Resources/logo-48.png
Vantus.App/Assets/Resources/logo-256.png
Vantus.App/Assets/splashscreen.png
```

#### Localization
```
Vantus.App/Resources/Strings/Strings.en.resx
Vantus.App/Resources/Strings/Strings.es.resx
Vantus.App/Resources/Strings/Strings.fr.resx
Vantus.App/Resources/Strings/Strings.de.resx
Vantus.App/Resources/Strings/Strings.ja.resx
```

#### Installer Files
```
Vantus.Installer/Setup/Logo.ico
Vantus.Installer/Setup/banner.bmp
Vantus.Installer/Setup/dialog.bmp
Vantus.Installer/License.rtf
```

#### Documentation
```
docs/user-guide.md
docs/admin-guide.md
docs/troubleshooting.md
```

### Files to MODIFY (28 total)

```
Vantus.App/ViewModels/SearchViewModel.cs
Vantus.App/Views/SearchPage.xaml
Vantus.App/ViewModels/DashboardViewModel.cs
Vantus.App/Views/DashboardPage.xaml
Vantus.App/ViewModels/SettingsPageViewModel.cs
Vantus.App/Views/SettingsPage.xaml
Vantus.App/ViewModels/RulesEditorViewModel.cs
Vantus.App/Views/RulesEditor.xaml
Vantus.App/Services/GrpcEngineClient.cs
Vantus.App/App.xaml.cs
Vantus.App/MainWindow.xaml
Vantus.App/MainWindow.xaml.cs
Vantus.App/App.xaml
Vantus.Core/Services/RuleService.cs
Vantus.Core/Services/IEngineClient.cs
Vantus.App/settings_definitions.json
Vantus.Installer/Build-Installer.ps1
Vantus.Installer/Package.wxs
Vantus.Installer/Shortcuts.wxs
installer.iss
Vantus.Engine/Services/EngineService.cs
Vantus.Engine/Worker.cs
Vantus.App/ViewModels/MainWindowViewModel.cs
Vantus.Core/Services/SettingsStore.cs
Vantus.Core/Services/SettingsSchema.cs
Vantus.App/Controls/ (create new controls as needed)
Vantus.Tests/ (add comprehensive tests)
```

---

## 4) Implementation Order

### Week 1: Core Engine
1. Create VantusDbContext with FileEntity, EmbeddingEntity
2. Implement FileIndexerService with basic scanning
3. Implement FileMonitorService with FileSystemWatcher
4. Complete CompositeContentExtractor
5. Complete OnnxEmbeddingService
6. Complete VectorSearchService
7. Wire EngineService gRPC methods to real implementations

### Week 2: Search Experience
1. Enhance SearchViewModel with real gRPC calls
2. Implement debouncing and result actions
3. Enhance SearchPage with filters and preview
4. Add search suggestions and history
5. Test search with real indexed files

### Week 3: Settings Framework
1. Complete settings_definitions.json with all settings
2. Add all setting control types
3. Implement ThemeService
4. Implement LocalizationService
5. Create settings pages for all categories
6. Wire settings to actual functionality

### Week 4: Dashboard and Rules
1. Enhance Dashboard with real status updates
2. Add pause/resume/rebuild controls
3. Complete RuleService implementation
4. Enhance RulesEditor with validation and simulation
5. Add rule import/export

### Week 5: Polish
1. Implement EngineLifecycleManager
2. Implement ErrorHandlingService
3. Implement NotificationService
4. Add accessibility features
5. Performance optimization
6. Add localization (EN, one additional language)

### Week 6: Branding and Installer
1. Convert Logo.png to all icon formats
2. Create splash screen
3. Brand WiX installer
4. Add code signing support
5. Create documentation
6. Final testing and bug fixes

---

## 5) Build and Test Commands

### Full Solution Build
```powershell
dotnet build Vantus.FileIndexer.sln
```

### Build with Warnings as Errors
```powershell
dotnet build Vantus.FileIndexer.sln /p:TreatWarningsAsErrors=true
```

### Clean and Rebuild
```powershell
dotnet clean
Remove-Item -Recurse -Force obj, bin -ErrorAction SilentlyContinue
dotnet restore
dotnet build Vantus.FileIndexer.sln
```

### Run the Application
```powershell
dotnet run --project Vantus.App/Vantus.App.csproj
```

### Run All Tests
```powershell
dotnet test Vantus.Tests/Vantus.Tests.csproj
```

### Run Single Test Class
```powershell
dotnet test Vantus.Tests/Vantus.Tests.csproj --filter "FullyQualifiedName~PresetManagerTests"
```

### Build Installer
```powershell
cd Vantus.Installer
.\Build-Installer.ps1 -Version "1.0.0.0"
```

---

## 6) Testing Requirements

### Unit Tests Required
- FileIndexerService unit tests
- FileMonitorService unit tests
- RuleService tests (add, delete, validate, execute)
- SettingsStore tests (load, save, migrate)
- SettingsSchema tests
- PresetManager tests
- PolicyEngine tests
- Import/Export tests

### Integration Tests Required
- gRPC communication tests
- Engine lifecycle tests
- End-to-end indexing workflow
- Search with real data
- Settings persistence workflow

### Manual Testing Checklist
- [ ] Clean install from MSI
- [ ] First launch experience (splash, initial index)
- [ ] Search returns results
- [ ] Theme switching works
- [ ] Language switching works
- [ ] Rules editor creates and saves rules
- [ ] Pause/resume indexing works
- [ ] Crash recovery works
- [ ] Uninstall removes all data

---

## 7) WPF + Wpf.Ui Code Style Guidelines

### Window Base Class
Use Wpf.Ui's FluentWindow:
```csharp
using Wpf.Ui.Controls;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

### Navigation
Use Frame and Page navigation:
```xml
<Grid>
    <Frame x:Name="ContentFrame" NavigationUIVisibility="Hidden"/>
</Grid>
```

```csharp
ContentFrame.Navigate(new DashboardPage());
```

### Wpf.Ui Controls Reference
```xml
xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"

<!-- Controls -->
<ui:Button />
<ui:TextBox />
<ui:ComboBox />
<ui:CheckBox />
<ui:Slider />
<ui:Card />
<ui:CardControl />
<ui:ProgressBar />
<ui:ProgressRing />
<ui:SymbolIcon />
<ui:InfoBar />
<ui:NavigationView />
<ui:FluentWindow />
```

### Theme Application
```csharp
using Wpf.Ui.Appearance;

// Apply dark theme
Wpf.Ui.Appearance.Theme.Apply(
    Wpf.Ui.Appearance.ThemeType.Dark,
    Wpf.Ui.Appearance.BackgroundType.Mica
);
```

### Naming Conventions
- PascalCase for public types and members
- _camelCase for private fields
- Async suffix for async methods
- Clear, descriptive names

### Nullability
- Enable `<Nullable>enable</Nullable>`
- Use `?` for nullable types
- Guard clauses with ArgumentNullException.ThrowIfNull

### Error Handling
- Catch specific exceptions
- Surface recoverable issues to UI
- Never swallow exceptions silently
- Log errors with context

### XAML Styling
- Use Wpf.Ui controls for consistent look
- Consistent spacing (16px standard)
- Proper DataBinding with INotifyPropertyChanged
- Resource dictionaries for shared styles
- Use `DynamicResource` for theme-aware resources

---

## 8) Special Instructions

### Logo.png Usage
The file `Logo.png` at the root is your brand logo. You MUST:
1. Convert it to proper ICO format with multiple sizes for app icon
2. Create PNG variants for different DPI
3. Use as installer icon
4. Use as splash screen (centered on branded background)
5. Use as shortcut icon

### Settings Definitions
The `settings_definitions.json` file is the SOURCE OF TRUTH for:
- All settings labels
- All settings helper text
- All default values
- All allowed values
- Control types
- Policy lockability

DO NOT hardcode these in XAML or code - use the registry pattern.

### Preset Values
When implementing presets, use these defaults from settings_definitions.json:
- **Personal**: User-focused, privacy-conscious defaults
- **Pro**: Performance-optimized, advanced features enabled
- **Enterprise-Private**: Security-focused, managed by IT
- **Enterprise-Automation**: Maximum automation, minimal user interaction

### Policy Locks
When policy locks a setting:
- UI control is disabled
- Show lock icon with tooltip explaining policy
- Show reason for lock
- Allow viewing but not editing

### NO Windows App SDK / WinUI 3
This is a pure WPF application. Under NO circumstances should you:
- Use WinUI 3 controls
- Use Windows App SDK APIs
- Reference Microsoft.WindowsAppSDK NuGet package
- Use any WinRT APIs
- Create XAML islands

All UI must be pure WPF using Wpf.Ui controls.

---

## 9) Final Verification Checklist

Before marking this task complete, verify:

### Functionality
- [ ] Search returns real results
- [ ] Dashboard shows live indexing status
- [ ] All 18 settings categories functional
- [ ] Theme switching works
- [ ] Language switching works
- [ ] Rules editor works
- [ ] Engine auto-starts
- [ ] Crash recovery works

### Quality
- [ ] Builds without errors
- [ ] All tests pass
- [ ] No crashes in testing
- [ ] Startup < 5 seconds
- [ ] Memory < 200MB at idle
- [ ] Accessibility verified
- [ ] NO Windows App SDK references in project

### Branding
- [ ] App shortcut uses Logo.png
- [ ] Installer is branded
- [ ] Splash screen displays
- [ ] Consistent visual design

### Documentation
- [ ] User guide complete
- [ ] Admin guide complete
- [ ] Code comments explain complex logic

---

## 10) Execution Command

You have full access to the codebase. Begin implementation immediately, working through the phases in order. Use parallel execution where independent tasks don't conflict.

**REMINDER**: This is a PURE WPF application. NO Windows App SDK, NO WinUI 3, NO WinRT.

**START NOW**: Begin with Phase 1 - Core Engine implementation.
