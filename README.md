# WallpaperEngine Extractor

## 简介

WallpaperEngine Extractor 是一款基于 Windows 11 UI 风格（WinUI 3/WPF）的 Wallpaper Engine PKG 文件提取工具。本工具提供直观的图形界面，让用户能够轻松地从 Wallpaper Engine 的 PKG 包中提取壁纸资源文件。

## 功能特性
![Uploading image.png…]()


### 核心功能

- **PKG 文件提取**：支持提取 Wallpaper Engine 格式的 PKG 安装包文件
- **批量处理**：支持同时处理多个 PKG 文件或包含 PKG 文件的文件夹
- **递归搜索**：可选择是否递归搜索子文件夹中的 PKG 文件
- **TEX 转 PNG**：内置 TEX 纹理格式到 PNG 图片的转换功能
- **拖放支持**：支持直接将文件或文件夹拖放到程序窗口

### 用户界面

- **Windows 11 风格**：采用 WinUI 3/Fluent Design 设计语言
- **Mica 背景效果**：使用 Windows 11 原生 Mica 模糊背景效果
- **深色/浅色主题**：自动跟随系统主题设置
- **实时预览**：大图片整理功能支持缩略图预览
- **操作日志**：实时显示提取进度和日志信息

### 文件整理

- **大图片扫描**：提取完成后自动扫描大于 1MB 的图片文件
- **排序功能**：支持按名称或文件大小排序
- **批量选择**：支持全选/取消全选功能
- **快速保存**：可将选中的大图片快速保存到指定位置

## 系统要求

### 运行环境

- **操作系统**：Windows 10 (1809+) / Windows 11
- **.NET 运行时**：.NET 8.0 Runtime（或使用自包含版本无需安装）

### 开发环境

- **.NET SDK**：.NET 8.0 SDK
- **IDE**：Visual Studio 2022/2024（推荐）
- **工作负载**：.NET 桌面开发

## 安装与构建

### 方式一：直接运行（推荐）

下载 release 版本压缩包并解压，直接运行 `WallpaperEngine_Extractor.exe`。

### 方式二：从源码构建

#### 1. 克隆仓库

```bash
git clone https://github.com/your-repo/WallpaperEngine_Extractor.git
cd WallpaperEngine_Extractor
```

#### 2. 还原依赖

```bash
dotnet restore WallpaperEngine_Extractor.sln
```

#### 3. 构建项目

```bash
# Debug 版本
dotnet build WallpaperEngine_Extractor.sln

# Release 版本
dotnet build -c Release WallpaperEngine_Extractor.sln
```

#### 4. 运行程序

```bash
dotnet run --project src/WallpaperEngine_Extractor/WallpaperEngine_Extractor.csproj
```

### 方式三：发布为自包含 EXE

```powershell
# 单文件发布（包含 .NET 运行时）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 项目结构

```
WallpaperEngine_Extractor/
├── WallpaperEngine_Extractor.sln    # Visual Studio 解决方案文件
├── README.md                         # 英文说明文档
├── README_zh.md                      # 中文说明文档
├── .gitignore                        # Git 忽略配置
│
├── src/
│   └── WallpaperEngine_Extractor/    # 主项目
│       ├── App.xaml(.cs)             # 应用程序入口
│       ├── MainWindow.xaml(.cs)      # 主窗口
│       ├── WallpaperEngine_Extractor.csproj  # 项目文件
│       │
│       ├── Views/                    # 页面视图
│       │   ├── ExtractPage.xaml(.cs) # 提取页面
│       │   └── AboutPage.xaml(.cs)   # 关于页面
│       │
│       ├── ViewModels/               # 视图模型 (MVVM)
│       │   ├── ExtractViewModel.cs   # 提取功能视图模型
│       │   └── SettingsViewModel.cs  # 设置视图模型
│       │
│       ├── Converters/               # 值转换器
│       │   └── InverseBoolToVisibilityConverter.cs
│       │
│       └── Themes/                   # 主题资源
│           ├── Dark.xaml
│           └── Light.xaml
│
└── libs/
    └── repkg/                        # RePKG 库（PKG/TEX 解析）
        ├── RePKG.sln
        ├── RePKG/                    # 命令行工具源码
        ├── RePKG.Core/               # 核心解析库
        ├── RePKG.Application/         # 应用层解析
        └── RePKG.Tests/              # 单元测试
```

## 使用指南

### 基本使用

1. **选择文件**：将 PKG 文件或文件夹拖入程序窗口，或点击"选择文件"/"选择文件夹"按钮
2. **设置输出目录**：默认输出到用户下载文件夹，可点击"..."按钮更改
3. **配置选项**：根据需要勾选"递归搜索子文件夹"和"转换 TEX 为 PNG"
4. **开始提取**：点击"开始提取"按钮，程序将显示提取进度和日志
5. **查看结果**：提取完成后，可在输出目录查看提取的文件

### 大图片整理

提取完成后，程序会自动扫描大于 1MB 的图片文件：

1. **排序**：点击"按名称"或"按大小"按钮排序文件列表
2. **选择**：勾选需要保存的图片文件
3. **全选/取消**：使用"全选"和"取消"按钮快速操作
4. **保存**：点击"保存"按钮，选择保存位置

### 快捷操作

- **拖放**：直接将文件或文件夹拖入文件选择区域
- **打开输出文件夹**：点击输出目录区域的"打开文件夹"按钮

## 技术架构

### 技术栈

| 组件 | 技术 | 版本 |
|------|------|------|
| UI 框架 | WPF + WinUI 3 (WPF-UI) | 4.2.0 |
| MVVM 框架 | CommunityToolkit.Mvvm | 8.2.2 |
| PKG 解析 | RePKG.Core + RePKG.Application | - |
| 图片转换 | SixLabors.ImageSharp | 2.1.9 |
| .NET | .NET | 8.0 |

### 设计模式

- **MVVM 模式**：使用 CommunityToolkit.Mvvm 实现数据绑定和命令
- **观察者模式**：ObservableObject 实现属性变更通知
- **命令模式**：RelayCommand 实现命令与UI的解耦

### 主题系统

程序使用 WPF-UI 库的 Fluent Design System：

- **Mica 背景**：Windows 11 原生模糊背景效果
- **自动主题**：跟随系统设置自动切换深色/浅色模式
- **动态资源**：使用 DynamicResource 实时响应主题变更

## 设置说明

程序设置保存在 `%APPDATA%\WallpaperEngine_Extractor\settings.txt`

| 设置项 | 默认值 | 说明 |
|--------|--------|------|
| RecursiveSearch | true | 递归搜索子文件夹中的 PKG 文件 |
| ConvertTexToPng | true | 将 TEX 纹理转换为 PNG 图片 |
| CopyProjectFiles | true | 复制项目文件（保留功能） |
| OpenOutputAfterExtract | false | 提取完成后打开输出文件夹 |

## 常见问题

### Q: 提取的 TEX 文件无法打开？
A: 请确保勾选了"转换 TEX 为 PNG"选项，程序会自动将 TEX 纹理转换为 PNG 格式。

### Q: 为什么提取速度很慢？
A: 大量文件或启用 TEX 转 PNG 时会比较耗时，属正常现象。

### Q: 设置没有保存？
A: 当前版本设置在退出程序时不会自动保存，正在计划后续版本修复。

### Q: 提示 "无法找到 PKG 文件"？
A: 请确认选择的是 Wallpaper Engine 的 PKG 文件（非普通压缩包），文件扩展名应为 .pkg。

## 注意事项

1. 本工具仅用于提取您拥有合法使用权的 Wallpaper Engine 壁纸资源
2. 提取的文件仅供个人学习研究使用，请遵守相关版权法规
3. TEX 转 PNG 功能基于 RePKG 库的逆向工程实现
4. 建议在提取前备份原始 PKG 文件

## 许可证

本项目使用 MIT 许可证开源。

RePKG 库及第三方组件各有其独立的许可证，详见项目中的 THIRD-PARTY-NOTICES.txt 文件。

## 更新日志

### v1.0.0
- 初始版本发布
- 支持 PKG 文件提取
- 支持 TEX 转 PNG
- Windows 11 Fluent Design 界面
- 深色/浅色主题自动切换

## 致谢

- [RePKG](https://github.com/notscuffed/repkg) - PKG 和 TEX 格式逆向工程库
- [WPF-UI](https://github.com/lepoco/wpfui) - WinUI 3 for WPF 组件库
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM 工具包
