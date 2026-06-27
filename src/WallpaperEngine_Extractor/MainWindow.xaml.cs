using System.ComponentModel;
using System.Windows;
using WallpaperEngine_Extractor.ViewModels;
using WallpaperEngine_Extractor.Views;

namespace WallpaperEngine_Extractor;

/// <summary>
/// 主窗口类
/// 使用WPF-UI的FluentWindow实现Windows 11风格界面
/// 窗口采用Mica背景效果和自定义标题栏
/// </summary>
public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        // 从设置中读取主题模式并应用（默认 System = 跟随系统）
        var settings = new SettingsViewModel();
        App.ApplyTheme(settings.ThemeMode);

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new ExtractPage());
    }

    /// <summary>
    /// 窗口关闭时保存设置
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        if (MainFrame.Content is ExtractPage page && page.DataContext is ExtractViewModel vm)
        {
            vm.Settings.SaveSettings();
        }
        base.OnClosing(e);
    }
}