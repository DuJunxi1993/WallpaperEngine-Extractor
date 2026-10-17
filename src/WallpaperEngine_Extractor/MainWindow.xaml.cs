using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Appearance;
using WallpaperEngine_Extractor.ViewModels;
using WallpaperEngine_Extractor.Views;

namespace WallpaperEngine_Extractor;

/// <summary>
/// 主窗口类
/// 使用 WPF-UI 的 FluentWindow 实现 Windows 11 风格界面
/// 窗口采用 Mica 背景效果与自定义标题栏
/// </summary>
public partial class MainWindow : Wpf.Ui.Controls.FluentWindow, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private AppThemeMode _currentThemeMode = AppThemeMode.System;

    /// <summary>
    /// 当前主题模式。绑定到 TitleBar 主题按钮的图标。
    /// </summary>
    public AppThemeMode CurrentThemeMode
    {
        get => _currentThemeMode;
        set
        {
            if (_currentThemeMode == value) return;
            _currentThemeMode = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentThemeMode)));
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        // 从设置中读取主题模式并应用（默认 System = 跟随系统）
        var settings = new SettingsViewModel();
        CurrentThemeMode = settings.ThemeMode;
        App.ApplyTheme(CurrentThemeMode);

        // 启用系统主题变化实时监听
        SystemThemeWatcher.Watch(this as System.Windows.Window);

        Loaded += MainWindow_Loaded;
        DataContext = this;
    }

    /// <summary>
    /// 主题切换命令：循环 Light → Dark → System
    /// </summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        var next = CurrentThemeMode switch
        {
            AppThemeMode.System => AppThemeMode.Light,
            AppThemeMode.Light  => AppThemeMode.Dark,
            AppThemeMode.Dark   => AppThemeMode.System,
            _ => AppThemeMode.System,
        };
        CurrentThemeMode = next;
        App.ApplyTheme(next);
        // 同步到 settings.txt，下次启动保留选择
        new SettingsViewModel { ThemeMode = next }.SaveSettings();
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
