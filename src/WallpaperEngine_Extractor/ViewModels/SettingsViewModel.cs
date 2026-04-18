using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace WallpaperEngine_Extractor.ViewModels;

/// <summary>
/// 设置视图模型
/// 管理应用程序的设置选项，包括提取选项和界面偏好设置
/// 设置保存在用户AppData目录下
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    // 设置文件路径：%APPDATA%\WallpaperEngine_Extractor\settings.txt
    private static readonly string SettingsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WallpaperEngine_Extractor",
        "settings.txt");

    // ==================== 设置属性 ====================

    /// <summary>
    /// 递归搜索子文件夹
    /// 选择文件夹时是否递归搜索其中的PKG文件
    /// </summary>
    [ObservableProperty]
    private bool _recursiveSearch = true;

    /// <summary>
    /// 将TEX转换为PNG
    /// 提取时是否将TEX纹理格式转换为PNG图片格式
    /// </summary>
    [ObservableProperty]
    private bool _convertTexToPng = true;

    /// <summary>
    /// 复制项目文件
    /// 是否复制额外的项目文件（暂未使用）
    /// </summary>
    [ObservableProperty]
    private bool _copyProjectFiles = true;

    /// <summary>
    /// 提取后打开输出文件夹
    /// 提取完成后是否自动打开输出目录
    /// </summary>
    [ObservableProperty]
    private bool _openOutputAfterExtract;

    // ==================== 构造函数 ====================

    public SettingsViewModel()
    {
        LoadSettings();
    }

    // ==================== 设置持久化 ====================

    /// <summary>
    /// 加载设置
    /// 从设置文件读取并应用保存的设置
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var lines = File.ReadAllLines(SettingsFile);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        switch (parts[0].Trim())
                        {
                            case "RecursiveSearch":
                                RecursiveSearch = bool.Parse(parts[1].Trim());
                                break;
                            case "ConvertTexToPng":
                                ConvertTexToPng = bool.Parse(parts[1].Trim());
                                break;
                            case "CopyProjectFiles":
                                CopyProjectFiles = bool.Parse(parts[1].Trim());
                                break;
                            case "OpenOutputAfterExtract":
                                OpenOutputAfterExtract = bool.Parse(parts[1].Trim());
                                break;
                        }
                    }
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// 保存设置
    /// 将当前设置写入到设置文件
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsFile);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var lines = new[]
            {
                $"RecursiveSearch={RecursiveSearch}",
                $"ConvertTexToPng={ConvertTexToPng}",
                $"CopyProjectFiles={CopyProjectFiles}",
                $"OpenOutputAfterExtract={OpenOutputAfterExtract}"
            };
            File.WriteAllLines(SettingsFile, lines);
        }
        catch { }
    }
}