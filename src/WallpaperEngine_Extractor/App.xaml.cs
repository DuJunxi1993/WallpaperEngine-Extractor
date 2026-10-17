using System.Windows;
using Wpf.Ui.Appearance;

namespace WallpaperEngine_Extractor;

public enum AppThemeMode
{
    System,
    Light,
    Dark
}

/// <summary>
/// 应用程序入口类
/// 负责初始化应用程序和系统主题设置
///
/// 设计要点：
/// - 不在 App.xaml 硬编码 WPF-UI ThemesDictionary 或自定义字典 (避免首次启动颜色错)
/// - 不订阅 SystemThemeWatcher / ApplicationThemeManager.Changed (避免系统主题变化
///   把用户主动设的 Light/Dark 模式偷偷切换回去)
/// - 用户模式明确按 Light/Dark/System 处理；System 模式只在启动时根据 OS 检测一次，
///   之后跟随用户在标题栏按钮上做的选择
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 全局当前主题模式（系统/浅色/深色），由 MainWindow 启动时从设置中读取并应用
    /// </summary>
    public static AppThemeMode CurrentThemeMode { get; set; } = AppThemeMode.System;

    protected override void OnStartup(StartupEventArgs e)
    {
        ApplyTheme(CurrentThemeMode);
        base.OnStartup(e);
    }

    /// <summary>
    /// 根据主题模式切换应用主题与资源字典。
    /// 自定义字典的加载目标直接由本参数决定，不再读 GetAppTheme()，
    /// 避免硬编码 / 缓存状态导致首启动误判。
    /// </summary>
    public static void ApplyTheme(AppThemeMode mode)
    {
        CurrentThemeMode = mode;

        // 切换 WPF-UI 内置主题字典
        if (mode == AppThemeMode.System)
            ApplicationThemeManager.ApplySystemTheme();
        else
            ApplicationThemeManager.Apply(mode == AppThemeMode.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light);

        // 切换自定义主题字典（Dark.xaml / Light.xaml），目标由调用方显式给出
        SwapThemeDictionary(mode);
    }

    private static void SwapThemeDictionary(AppThemeMode mode)
    {
        var resources = Current.Resources;
        var merged = resources.MergedDictionaries;
        const string darkPath = "pack://application:,,,/Themes/Dark.xaml";
        const string lightPath = "pack://application:,,,/Themes/Light.xaml";

        // System 模式按 WPF-UI 检测到的实际系统主题选择
        // Light/Dark 模式直接按用户选择加载
        var effective = mode == AppThemeMode.System
            ? (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark
                ? AppThemeMode.Dark
                : AppThemeMode.Light)
            : mode;
        var targetPath = effective == AppThemeMode.Dark ? darkPath : lightPath;
        System.Diagnostics.Debug.WriteLine($"[Theme] SwapThemeDictionary: mode={mode}, effective={effective}, target={targetPath}");

        // 移除已有的自定义主题字典
        ResourceDictionary? toRemove = null;
        foreach (var dict in merged)
        {
            var src = dict.Source?.ToString();
            if (src != null && (src.EndsWith("/Themes/Dark.xaml") || src.EndsWith("/Themes/Light.xaml")))
            {
                toRemove = dict;
                break;
            }
        }
        if (toRemove != null) merged.Remove(toRemove);

        // 加入当前主题字典
        var newDict = new ResourceDictionary { Source = new Uri(targetPath, UriKind.Absolute) };
        merged.Add(newDict);
    }
}