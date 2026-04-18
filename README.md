# WallpaperEngine Extractor

A Wallpaper Engine PKG file extractor with Windows 11 UI style (WinUI 3/WPF).

## Features

- **PKG Extraction**: Extract resources from Wallpaper Engine PKG packages
- **Batch Processing**: Process multiple PKG files or folders at once
- **Recursive Search**: Option to search subfolders for PKG files
- **TEX to PNG**: Built-in conversion from TEX texture format to PNG images
- **Drag & Drop**: Drag files or folders directly into the application window
- **Windows 11 UI**: Fluent Design with Mica backdrop effect
- **Auto Theme**: Automatically follows system dark/light mode settings
- **Large Image Organizer**: Scan and preview large images after extraction

## Requirements

### Runtime
- Windows 10 (1809+) or Windows 11
- .NET 8.0 Runtime (not needed for self-contained version)

### Development
- .NET 8.0 SDK
- Visual Studio 2022/2024 (recommended)
- .NET desktop development workload

## Quick Start

### Download Release
Download the latest release from the releases page and run `WallpaperEngine_Extractor.exe`.

### Build from Source

```bash
# Clone the repository
git clone https://github.com/your-repo/WallpaperEngine_Extractor.git
cd WallpaperEngine_Extractor

# Restore dependencies
dotnet restore WallpaperEngine_Extractor.sln

# Build
dotnet build -c Release WallpaperEngine_Extractor.sln

# Run
dotnet run --project src/WallpaperEngine_Extractor/WallpaperEngine_Extractor.csproj
```

### Publish as Single File

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Project Structure

```
WallpaperEngine_Extractor/
├── WallpaperEngine_Extractor.sln
├── README.md
├── README_zh.md
├── .gitignore
│
├── src/
│   └── WallpaperEngine_Extractor/
│       ├── App.xaml(.cs)
│       ├── MainWindow.xaml(.cs)
│       ├── Views/
│       │   ├── ExtractPage.xaml(.cs)
│       │   └── AboutPage.xaml(.cs)
│       ├── ViewModels/
│       │   ├── ExtractViewModel.cs
│       │   └── SettingsViewModel.cs
│       └── Converters/
│           └── InverseBoolToVisibilityConverter.cs
│
└── libs/
    └── repkg/          # RePKG library (PKG/TEX parsing)
        ├── RePKG.Core/
        ├── RePKG.Application/
        └── RePKG/
```

## Usage

1. **Select Files**: Drag PKG files/folders into the window, or click "选择文件"/"选择文件夹"
2. **Set Output**: Default output is your Downloads folder, click "..." to change
3. **Configure Options**: Check "递归搜索子文件夹" for recursive search, "转换 TEX 为 PNG" for conversion
4. **Extract**: Click "开始提取" to start extraction
5. **Organize**: After extraction, large images (>1MB) are listed for quick saving

## Tech Stack

| Component | Technology | Version |
|-----------|------------|---------|
| UI Framework | WPF + WinUI 3 (WPF-UI) | 4.2.0 |
| MVVM Framework | CommunityToolkit.Mvvm | 8.2.2 |
| PKG Parsing | RePKG.Core + RePKG.Application | - |
| Image Processing | SixLabors.ImageSharp | 2.1.9 |
| .NET | .NET | 8.0 |

## Settings

Settings are saved to `%APPDATA%\WallpaperEngine_Extractor\settings.txt`:

| Setting | Default | Description |
|---------|---------|-------------|
| RecursiveSearch | true | Search subfolders for PKG files |
| ConvertTexToPng | true | Convert TEX textures to PNG |
| CopyProjectFiles | true | Copy project files (reserved) |
| OpenOutputAfterExtract | false | Open output folder after extraction |

## License

MIT License.

## Credits

- [RePKG](https://github.com/notscuffed/repkg) - PKG and TEX format reverse engineering
- [WPF-UI](https://github.com/lepoco/wpfui) - WinUI 3 for WPF
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM Toolkit