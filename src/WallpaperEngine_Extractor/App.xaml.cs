using System.Windows;
using Wpf.Ui.Appearance;

namespace WallpaperEngine_Extractor;

/// <summary>
/// 应用程序入口类
/// 负责初始化应用程序和系统主题设置
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // 应用系统主题 (根据Windows设置自动切换深色/浅色模式)
        ApplicationThemeManager.ApplySystemTheme();
        base.OnStartup(e);
    }
}