using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RePKG.Application.Package;
using RePKG.Application.Texture;
using RePKG.Core.Texture;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WallpaperEngine_Extractor.ViewModels;

/// <summary>
/// 提取页面视图模型
/// 负责PKG文件的提取逻辑、大文件扫描、图片预览等功能
/// 使用MVVM模式，通过CommunityToolkit.Mvvm实现数据绑定和命令
/// </summary>
public enum SortMode { Name, Size }  // 排序模式：按名称或按大小

public partial class ExtractViewModel : ObservableObject
{
    // ==================== 公开属性 ====================

    /// <summary>输出目录路径，默认为用户下载文件夹</summary>
    [ObservableProperty]
    private string _outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    /// <summary>是否已选择文件</summary>
    [ObservableProperty]
    private bool _hasSelectedFiles;

    /// <summary>是否正在提取中，用于禁用提取按钮</summary>
    [ObservableProperty]
    private bool _isExtracting;

    /// <summary>提取进度百分比 (0-100)</summary>
    [ObservableProperty]
    private int _progress;

    /// <summary>状态文本，显示当前操作状态</summary>
    [ObservableProperty]
    private string _statusText = "就绪";

    /// <summary>日志文本，显示所有操作日志</summary>
    [ObservableProperty]
    private string _logText = "";

    /// <summary>整理器是否可见，当存在大图片时显示</summary>
    [ObservableProperty]
    private bool _isOrganizerVisible = true;

    /// <summary>整理器状态文本，显示找到的大图片数量</summary>
    [ObservableProperty]
    private string _organizerStatusText = "";

    /// <summary>当前排序模式</summary>
    [ObservableProperty]
    private SortMode _currentSortMode = SortMode.Name;

    /// <summary>预览图片，用于在界面右侧显示选中图片的缩略图</summary>
    [ObservableProperty]
    private BitmapImage? _previewImage;

    // ==================== 集合属性 ====================

    /// <summary>日志消息集合，用于实时显示日志</summary>
    public ObservableCollection<string> LogMessages { get; } = new();

    /// <summary>大文件列表，提取完成后显示大于1MB的图片</summary>
    public ObservableCollection<LargeFileInfo> LargeFiles { get; } = new();

    /// <summary>已选择的文件列表，显示用户选择的文件/文件夹</summary>
    public ObservableCollection<SelectedFileInfo> SelectedFiles { get; } = new();

    /// <summary>设置视图模型，管理提取选项</summary>
    public SettingsViewModel Settings { get; } = new();

    // ==================== 私有字段 ====================

    /// <summary>待处理的PKG文件队列</summary>
    private string[]? _pendingFiles;

    // ==================== 构造函数 ====================

    public ExtractViewModel()
    {
        // 监听大文件列表变化，当添加第一个文件时更新预览
        LargeFiles.CollectionChanged += LargeFiles_CollectionChanged;
    }

    // ==================== 事件处理 ====================

    /// <summary>
    /// 大文件列表集合变化事件处理
    /// 当添加第一个大文件时，自动更新预览图片
    /// </summary>
    private void LargeFiles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && LargeFiles.Count == 1)
        {
            UpdatePreviewImage(LargeFiles[0]);
        }
    }

    /// <summary>
    /// 排序模式改变事件处理
    /// 自动重新排序文件列表
    /// </summary>
    partial void OnCurrentSortModeChanged(SortMode value)
    {
        SortFiles();
    }

    // ==================== 文件操作方法 ====================

    /// <summary>
    /// 设置文件列表（替换模式）
    /// 用于首次选择文件或清除后重新选择
    /// </summary>
    /// <param name="files">文件/文件夹路径数组</param>
    public void SetFiles(string[] files)
    {
        if (files == null || files.Length == 0) return;

        var allFiles = files.ToList();
        if (_pendingFiles != null)
        {
            allFiles.InsertRange(0, _pendingFiles);
        }
        _pendingFiles = allFiles.ToArray();

        SelectedFiles.Clear();
        foreach (var f in _pendingFiles)
        {
            var info = new FileInfo(f);
            SelectedFiles.Add(new SelectedFileInfo
            {
                FilePath = f,
                FileName = Path.GetFileName(f),
                IsDirectory = Directory.Exists(f),
                Size = info.Exists ? info.Length : 0
            });
        }

        HasSelectedFiles = true;
        AddLog($"已选择 {_pendingFiles.Length} 个文件/文件夹");
    }

    /// <summary>
    /// 添加文件到列表（追加模式）
    /// 用于继续添加更多文件
    /// </summary>
    /// <param name="files">文件/文件夹路径数组</param>
    public void AddFiles(string[] files)
    {
        if (files == null || files.Length == 0) return;

        var allFiles = _pendingFiles?.ToList() ?? new List<string>();
        allFiles.AddRange(files);
        _pendingFiles = allFiles.ToArray();

        foreach (var f in files)
        {
            var info = new FileInfo(f);
            SelectedFiles.Add(new SelectedFileInfo
            {
                FilePath = f,
                FileName = Path.GetFileName(f),
                IsDirectory = Directory.Exists(f),
                Size = info.Exists ? info.Length : 0
            });
        }

        HasSelectedFiles = true;
        AddLog($"已添加 {files.Length} 个文件，当前共 {_pendingFiles.Length} 个");
    }

    // ==================== 命令实现 ====================

    /// <summary>
    /// 清除文件命令
    /// 清空所有已选择的文件和待处理队列
    /// </summary>
    [RelayCommand]
    private void ClearFiles()
    {
        _pendingFiles = null;
        SelectedFiles.Clear();
        HasSelectedFiles = false;
        AddLog("已清除选择");
    }

    /// <summary>
    /// 浏览输出目录命令
    /// 打开文件夹选择对话框
    /// </summary>
    [RelayCommand]
    private void BrowseOutput()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "选择输出目录"
        };
        if (dialog.ShowDialog() == true)
        {
            OutputDirectory = dialog.FolderName;
            AddLog($"输出目录: {OutputDirectory}");
        }
    }

    /// <summary>
    /// 开始提取命令
    /// 异步执行PKG文件提取，显示进度，支持取消
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExtract))]
    private async Task ExtractAsync()
    {
        if (_pendingFiles == null || _pendingFiles.Length == 0)
        {
            AddLog("错误: 未选择任何文件");
            return;
        }

        IsExtracting = true;
        Progress = 0;
        AddLog("开始提取...");

        try
        {
            // 获取所有待处理的PKG文件
            var filesToProcess = await Task.Run(() => GetPkgFiles(_pendingFiles, Settings.RecursiveSearch));
            var totalFiles = filesToProcess.Length;
            var processedFiles = 0;

            if (totalFiles == 0)
            {
                AddLog("未找到 PKG 文件");
                return;
            }

            AddLog($"找到 {totalFiles} 个 PKG 文件");

            // 逐个处理PKG文件
            foreach (var pkgFile in filesToProcess)
            {
                try
                {
                    // 创建输出子文件夹
                    var outputDir = Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(pkgFile));
                    Directory.CreateDirectory(outputDir);

                    AddLog($"正在提取: {Path.GetFileName(pkgFile)}");

                    // 执行提取
                    await Task.Run(() => ExtractPkg(pkgFile, outputDir));

                    processedFiles++;
                    Progress = (int)((double)processedFiles / totalFiles * 100);
                    StatusText = $"正在提取... {processedFiles}/{totalFiles}";
                }
                catch (Exception ex)
                {
                    AddLog($"提取失败 {Path.GetFileName(pkgFile)}: {ex.Message}");
                }
            }

            AddLog("提取完成!");
            StatusText = "提取完成";

            // 提取完成后扫描大文件
            await ScanLargeFilesAsync();

            // 如果设置了自动打开，则打开输出文件夹
            if (Settings.OpenOutputAfterExtract)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = OutputDirectory,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            AddLog($"错误: {ex.Message}");
            StatusText = "提取失败";
        }
        finally
        {
            IsExtracting = false;
        }
    }

    // ==================== 大文件扫描 ====================

    /// <summary>
    /// 扫描大文件异步任务
    /// 在提取完成后扫描输出目录，找出大于1MB的图片文件
    /// </summary>
    private async Task ScanLargeFilesAsync()
    {
        try
        {
            LargeFiles.Clear();
            var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };
            var minSize = 1024 * 1024;  // 1MB

            await Task.Run(() =>
            {
                // 遍历输出目录下的所有子文件夹
                foreach (var dir in Directory.GetDirectories(OutputDirectory))
                {
                    foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var ext = Path.GetExtension(file).ToLower();
                            var info = new FileInfo(file);
                            // 只收集大于1MB的图片文件
                            if (info.Length >= minSize && imageExtensions.Contains(ext))
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    LargeFiles.Add(new LargeFileInfo
                                    {
                                        FilePath = file,
                                        FileName = Path.GetFileName(file),
                                        Size = info.Length,
                                        RelativePath = Path.GetRelativePath(OutputDirectory, file)
                                    });
                                });
                            }
                        }
                        catch { }
                    }
                }
            });

            SortFiles();
            OrganizerStatusText = $"找到 {LargeFiles.Count} 个大图片 (>1MB)";
            if (LargeFiles.Count > 0)
            {
                IsOrganizerVisible = true;
                AddLog($"整理: 发现 {LargeFiles.Count} 个大于1MB的图片文件");
                UpdatePreviewImage(LargeFiles[0]);
            }
        }
        catch (Exception ex)
        {
            AddLog($"扫描大文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新预览图片
    /// 加载选中文件的缩略图进行预览
    /// </summary>
    /// <param name="fileInfo">大文件信息</param>
    private void UpdatePreviewImage(LargeFileInfo? fileInfo)
    {
        if (fileInfo == null || !File.Exists(fileInfo.FilePath))
        {
            PreviewImage = null;
            return;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(fileInfo.FilePath);
            bitmap.DecodePixelWidth = 400;  // 解码为较小尺寸以提升性能
            bitmap.EndInit();
            bitmap.Freeze();
            PreviewImage = bitmap;
        }
        catch
        {
            PreviewImage = null;
        }
    }

    // ==================== 排序命令 ====================

    /// <summary>
    /// 按名称排序命令
    /// </summary>
    [RelayCommand]
    private void SortByName()
    {
        CurrentSortMode = SortMode.Name;
    }

    /// <summary>
    /// 按大小排序命令
    /// </summary>
    [RelayCommand]
    private void SortBySize()
    {
        CurrentSortMode = SortMode.Size;
    }

    /// <summary>
    /// 执行文件排序
    /// 根据当前排序模式对LargeFiles集合进行排序
    /// </summary>
    private void SortFiles()
    {
        var sorted = CurrentSortMode == SortMode.Name
            ? LargeFiles.OrderByDescending(f => f.FileName).ToList()
            : LargeFiles.OrderByDescending(f => f.Size).ToList();

        LargeFiles.Clear();
        foreach (var item in sorted)
        {
            LargeFiles.Add(item);
        }
    }

    /// <summary>
    /// 大文件列表选择改变回调
    /// 由View调用以更新预览图
    /// </summary>
    public void OnLargeFileSelected(LargeFileInfo? fileInfo)
    {
        UpdatePreviewImage(fileInfo);
    }

    // ==================== 保存选中文件 ====================

    /// <summary>
    /// 保存选中文件命令
    /// 将用户勾选的大文件复制到指定位置
    /// </summary>
    [RelayCommand]
    private async Task SaveSelectedFilesAsync()
    {
        var selected = LargeFiles.Where(f => f.IsSelected).ToList();
        if (selected.Count == 0)
        {
            AddLog("请先选择要保存的文件");
            return;
        }

        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "选择保存位置"
        };
        if (dialog.ShowDialog() != true) return;

        var saveDir = dialog.FolderName;
        var saved = 0;

        await Task.Run(() =>
        {
            foreach (var file in selected)
            {
                try
                {
                    var dest = Path.Combine(saveDir, file.FileName);
                    File.Copy(file.FilePath, dest, true);
                    saved++;
                }
                catch { }
            }
        });

        AddLog($"已保存 {saved} 个文件到: {saveDir}");
        Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = saveDir, UseShellExecute = true });
    }

    /// <summary>
    /// 全选命令
    /// 选中列表中的所有大文件
    /// </summary>
    [RelayCommand]
    private void SelectAllLargeFiles()
    {
        foreach (var f in LargeFiles) f.IsSelected = true;
    }

    /// <summary>
    /// 取消全选命令
    /// 取消选中所有大文件
    /// </summary>
    [RelayCommand]
    private void DeselectAllLargeFiles()
    {
        foreach (var f in LargeFiles) f.IsSelected = false;
    }

    // ==================== 私有辅助方法 ====================

    /// <summary>
    /// 判断是否可以执行提取
    /// </summary>
    private bool CanExtract() => !IsExtracting;

    /// <summary>
    /// 获取PKG文件列表
    /// 从输入的文件/文件夹中找出所有PKG文件
    /// </summary>
    /// <param name="inputs">输入的文件/文件夹路径数组</param>
    /// <param name="recursive">是否递归搜索子文件夹</param>
    /// <returns>PKG文件路径数组</returns>
    private string[] GetPkgFiles(string[] inputs, bool recursive)
    {
        var result = new System.Collections.Generic.List<string>();
        foreach (var input in inputs)
        {
            if (Directory.Exists(input))
            {
                var files = recursive
                    ? Directory.GetFiles(input, "*.pkg", SearchOption.AllDirectories)
                    : Directory.GetFiles(input, "*.pkg", SearchOption.TopDirectoryOnly);
                result.AddRange(files);
            }
            else if (File.Exists(input) && input.EndsWith(".pkg", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(input);
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// 提取单个PKG文件
    /// 读取PKG格式并解压所有条目到输出目录
    /// 支持将TEX格式转换为PNG图片
    /// </summary>
    /// <param name="pkgPath">PKG文件路径</param>
    /// <param name="outputDir">输出目录路径</param>
    private void ExtractPkg(string pkgPath, string outputDir)
    {
        try
        {
            using var stream = File.OpenRead(pkgPath);
            using var reader = new BinaryReader(stream);

            var packageReader = new PackageReader { ReadEntryBytes = true };
            var package = packageReader.ReadFrom(reader);

            AddLog($"PKG 格式: {package.Magic}, 条目数: {package.Entries.Count}");

            var totalEntries = package.Entries.Count;
            var processedEntries = 0;

            // 遍历所有条目并提取
            foreach (var entry in package.Entries)
            {
                try
                {
                    var entryPath = Path.Combine(outputDir, entry.FullPath);
                    var entryDir = Path.GetDirectoryName(entryPath);
                    if (!string.IsNullOrEmpty(entryDir))
                        Directory.CreateDirectory(entryDir);

                    if (entry.Bytes == null || entry.Bytes.Length == 0)
                    {
                        AddLog($"跳过空条目: {entry.FullPath}");
                        continue;
                    }

                    var ext = Path.GetExtension(entry.FullPath);
                    // 如果启用TEX转PNG选项且文件是TEX格式
                    if (ext.Equals(".tex", StringComparison.OrdinalIgnoreCase) && Settings.ConvertTexToPng)
                    {
                        try
                        {
                            // 读取TEX纹理
                            using var texStream = new MemoryStream(entry.Bytes);
                            using var texReaderBinary = new BinaryReader(texStream);
                            var tex = TexReader.Default.ReadFrom(texReaderBinary);

                            // 转换为图片
                            var converter = new TexToImageConverter();
                            var imageResult = converter.ConvertToImage(tex);

                            // 根据格式确定扩展名
                            var targetExt = imageResult.Format switch
                            {
                                MipmapFormat.ImageGIF => ".gif",
                                MipmapFormat.VideoMp4 => ".mp4",
                                _ => ".png"
                            };

                            var pngPath = Path.ChangeExtension(entryPath, targetExt);
                            File.WriteAllBytes(pngPath, imageResult.Bytes);
                        }
                        catch (Exception ex)
                        {
                            // 转换失败时保存原始文件
                            AddLog($"TEX 转换失败: {entry.FullPath}, 保存原始文件: {ex.Message}");
                            File.WriteAllBytes(entryPath, entry.Bytes);
                        }
                    }
                    else
                    {
                        File.WriteAllBytes(entryPath, entry.Bytes);
                    }

                    processedEntries++;
                }
                catch (Exception ex)
                {
                    AddLog($"提取条目失败: {entry.FullPath}: {ex.Message}");
                }
            }

            AddLog($"已提取 {processedEntries}/{totalEntries} 个文件");
        }
        catch (Exception ex)
        {
            AddLog($"解析 PKG 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加日志消息
    /// 线程安全地添加日志到界面
    /// </summary>
    /// <param name="message">日志消息</param>
    private void AddLog(string message)
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogMessages.Add($"[{time}] {message}");
            LogText = string.Join(Environment.NewLine, LogMessages);
        });
    }
}

/// <summary>
/// 大文件信息类
/// 实现INotifyPropertyChanged以支持UI绑定
/// </summary>
public partial class LargeFileInfo : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public long Size { get; set; }
    public string RelativePath { get; set; } = "";

    /// <summary>
    /// 文件大小的人类可读格式
    /// 自动根据大小选择B/KB/MB单位
    /// </summary>
    public string SizeText => Size switch
    {
        < 1024 => $"{Size} B",
        < 1024 * 1024 => $"{Size / 1024.0:F1} KB",
        _ => $"{Size / 1024.0 / 1024.0:F1} MB"
    };
}

/// <summary>
/// 已选择文件信息类
/// 显示用户选择的文件/文件夹信息
/// </summary>
public class SelectedFileInfo
{
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public bool IsDirectory { get; set; }
    public long Size { get; set; }

    /// <summary>
    /// 文件大小的人类可读格式
    /// </summary>
    public string SizeText => Size switch
    {
        < 1024 => $"{Size} B",
        < 1024 * 1024 => $"{Size / 1024.0:F1} KB",
        _ => $"{Size / 1024.0 / 1024.0:F1} MB"
    };

    /// <summary>
    /// 显示文本，包含文件类型和大小信息
    /// </summary>
    public string DisplayText => IsDirectory ? $"[文件夹] {FileName}" : $"[文件] {FileName} ({SizeText})";
}