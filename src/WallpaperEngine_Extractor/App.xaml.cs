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
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 全局当前主题模式（系统/浅色/深色），由 MainWindow 启动时从设置中读取并应用
    /// </summary>
    public static AppThemeMode CurrentThemeMode { get; set; } = AppThemeMode.System;

    private static bool _changedEventHooked;

    protected override void OnStartup(StartupEventArgs e)
    {
        ApplyTheme(CurrentThemeMode);
        base.OnStartup(e);
    }

    /// <summary>
    /// 根据主题模式切换应用主题与资源字典
    /// </summary>
    public static void ApplyTheme(AppThemeMode mode)
    {
        CurrentThemeMode = mode;

        // 切换 WPF-UI 内置主题字典
        if (mode == AppThemeMode.System)
            ApplicationThemeManager.ApplySystemTheme();
        else
            ApplicationThemeManager.Apply(mode == AppThemeMode.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light);

        // 切换自定义主题字典（Dark.xaml / Light.xaml）
        SwapThemeDictionary();

        // 订阅 Changed 事件，确保 WPF-UI 主题变更时（系统主题切换）同步自定义字典
        if (!_changedEventHooked)
        {
            ApplicationThemeManager.Changed += OnApplicationThemeChanged;
            _changedEventHooked = true;
        }
    }

    /// <summary>
    /// WPF-UI 主题变更回调：WPF-UI 已自动切换内置字典，这里只需同步自定义字典
    /// （仅在 System 模式下响应；Light/Dark 是用户主动设置，不应被系统事件覆盖）
    /// </summary>
    private static void OnApplicationThemeChanged(ApplicationTheme theme, System.Windows.Media.Color accent)
    {
        if (CurrentThemeMode == AppThemeMode.System)
        {
            SwapThemeDictionary();
        }
    }

    private static void SwapThemeDictionary()
    {
        var resources = Current.Resources;
        var merged = resources.MergedDictionaries;
        const string darkPath = "pack://application:,,,/Themes/Dark.xaml";
        const string lightPath = "pack://application:,,,/Themes/Light.xaml";

        var isDark = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;
        var targetPath = isDark ? darkPath : lightPath;

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