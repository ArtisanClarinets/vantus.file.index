# AGENT.md — Vantus File Indexer Developer Guide

**For AI Agents:** Please refer to [AGENTS.md](./AGENTS.md) for strict technical specifications and implementation rules.

This document provides general guidelines, conventions, and context for developers and agents working on the **Vantus File Indexer** repository.

---

## 1. Project Overview

Vantus File Indexer is a Windows desktop application comprising:
-   **Vantus.App:** A **WPF (.NET 8+)** settings shell.
-   **Vantus.Core:** Shared logic (models, registry, presets, policy).
-   **Vantus.Tests:** xUnit test suite.
-   **Engine:** Background service (accessed via IPC).

**Key Principle:** The Settings UI is **metadata-driven**. Pages and controls are rendered dynamically from `settings_definitions.json`, not hardcoded in XAML.

---

## 2. Technology Stack

-   **Platform:** Windows (.NET 8)
-   **UI Framework:** **WPF** (Windows Presentation Foundation)
-   **Architecture:** MVVM (`CommunityToolkit.Mvvm`) + DI (`Microsoft.Extensions.Hosting`)
-   **Styling:** **WPF UI** (Recommended) or **ModernWpf**
-   **Persistence:** `System.Text.Json`

> **Strict Constraint:** Do **NOT** use Windows App SDK (WinUI 3) for the Settings application.

---

## 3. Developer Workflow

### Build & Run
```powershell
dotnet build Vantus.FileIndexer.sln
dotnet run --project Vantus.App/Vantus.App.csproj
```

### Testing
```powershell
dotnet test Vantus.Tests/Vantus.Tests.csproj
```

### Branching
-   Use feature branches: `feature/<topic>`
-   Update documentation and tests for every feature.

---

## 4. Architecture & Patterns

### Settings Registry
The `settings_definitions.json` file is the source of truth for:
-   UI Labels & Helper Text
-   Default values (per preset)
-   Control types & validation logic

### Effective Value Resolution
When determining a setting's value, the system follows this precedence:
1.  **Policy Lock** (Enterprise/Managed enforcement)
2.  **User Value** (Local persistence)
3.  **Preset Default** (Based on active preset)
4.  **Schema Fallback** (Hardstop default)

### UI Thread Safety
-   **Never** block the UI thread.
-   Perform IO, heavy computation, and JSON parsing on background threads.
-   Marshal updates back using `Dispatcher.InvokeAsync`.

---

## 5. Documentation & References

-   **[AGENTS.md](./AGENTS.md):** **Technical Specification & Agent Rules** (Read this for implementation details).
-   **[PROMPT.MD](./PROMPT.MD):** Original Master Prompt / Requirements.
-   **docs/**: Detailed design documents (IA, Presets, Policy).

For detailed implementation instructions, strict constraints, and exact deliverables, please consult **[AGENTS.md](./AGENTS.md)**.
