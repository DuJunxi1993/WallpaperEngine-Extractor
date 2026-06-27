# AGENTS.md — WallpaperEngine Extractor

## Quick start

```powershell
git submodule init && git submodule update   # required — libs/repkg is a pinned submodule
dotnet restore WallpaperEngine_Extractor.sln
dotnet build WallpaperEngine_Extractor.sln   # or: dotnet build -c Release
dotnet run --project src/WallpaperEngine_Extractor
```

## Critical gotchas

- **Submodule required**: `libs/repkg` (commit `8005eb2`, upstream `https://github.com/notscuffed/repkg`) must be checked out before building — run `git submodule init && git submodule update`.
- **Windows-only**: `net10.0-windows` with WPF; cannot build/run on non-Windows.
- **No tests**, no CI, no linter/formatter config.

## Project layout

- `src/WallpaperEngine_Extractor/` — single WPF app (MVVM via CommunityToolkit.Mvvm 8.4.2, WinUI via WPF-UI 4.3.0)
- `libs/repkg/` — RePKG submodule (PKG/TEX parsing, 3 projects inside)
- Entrypoint: `App.xaml.cs` → `MainWindow.xaml.cs` (FluentWindow) → `ExtractPage` by default
- ViewModel: `ExtractViewModel.cs` owns extraction logic, `SettingsViewModel.cs` manages persisted options

## Build & run

| Command | Purpose |
|---|---|
| `dotnet restore WallpaperEngine_Extractor.sln` | Restore NuGet packages |
| `dotnet build WallpaperEngine_Extractor.sln` | Debug build (x64) |
| `dotnet build -c Release WallpaperEngine_Extractor.sln` | Release build (x64) |
| `dotnet run --project src/WallpaperEngine_Extractor/WallpaperEngine_Extractor.csproj` | Run the app |
| `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true` | Self-contained single EXE |

## Key dependencies

| Package | Version |
|---|---|
| CommunityToolkit.Mvvm | 8.4.2 |
| WPF-UI | 4.3.0 |
| RePKG.Core / RePKG.Application | in-repo (submodule) |
| SixLabors.ImageSharp | 2.1.9 |

## Settings persistence

Settings stored as key=value at `%APPDATA%\WallpaperEngine_Extractor\settings.txt`:
`RecursiveSearch` (true), `ConvertTexToPng` (true), `CopyProjectFiles` (true), `OpenOutputAfterExtract` (false), `ThemeMode` (System). ThemeMode is one of `System|Light|Dark`; default is `System` (follows OS theme). No in-app toggle — edit settings.txt or change via WPF-UI `ApplicationThemeManager` programmatically.

## Theming

- WPF-UI's built-in theme dictionary is the primary theming system; `Themes/Dark.xaml` and `Themes/Light.xaml` add custom design tokens (`Spacing.*`, `Radius.*`, `FontSize.*`, `ControlHeight.*`, status brushes `SuccessBrush`/`WarningBrush`/`ErrorBrush`/`InfoBrush`) — keep both even though the UI toggle is gone, since the runtime swaps dictionaries based on system theme.
- Title bar uses WPF-UI `TitleBar` (no app icon, just text title). Trailing content is currently empty.
- Window backdrop is `Mica`.

## Style conventions

- File-scoped namespaces, `[ObservableProperty]` source generators, `[RelayCommand]` for commands
- Chinese comments throughout (the README and code comments are bilingual)
- WPF-UI `FluentWindow` base class, not plain `Window`
- All visual styling uses `DynamicResource` tokens from `Themes/*.xaml` — no hardcoded colors, sizes, or spacing literals in views
- Use `ui:SymbolIcon` (Fluent Icons) instead of legacy `Segoe MDL2 Assets` glyph codes
- Log output uses `LogLevel` enum (`Info`/`Success`/`Warning`/`Error`) for color coding via `LogLevelToBrushConverter`
