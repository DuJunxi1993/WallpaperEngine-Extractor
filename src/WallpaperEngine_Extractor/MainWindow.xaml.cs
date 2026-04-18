using System.Windows;
using Wpf.Ui.Controls;
using WallpaperEngine_Extractor.Views;

namespace WallpaperEngine_Extractor;

/// <summary>
/// 主窗口类
/// 使用WPF-UI的FluentWindow实现Windows 11风格界面
/// 窗口采用Mica背景效果和自定义标题栏
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        // 窗口加载完成后导航到提取页面
        Loaded += MainWindow_Loaded;
    }

    /// <summary>
    /// 窗口加载完成事件处理
    /// 默认导航到提取页面 (ExtractPage)
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new ExtractPage());
    }
}