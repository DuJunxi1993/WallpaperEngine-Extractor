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
/// 窗口采用自定义标题栏 (ExtendsContentIntoTitleBar) + 不透明面板
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

        // 注意：故意不调用 SystemThemeWatcher.Watch()。
        // 用户已经在标题栏手动 Light → Dark → System 循环了；如果再让系统主题变化
        // 触发 WPF-UI 字典切换，会出现"内置字典换了 / 自定义字典没换"的颜色错配。

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
            // vm.Settings 是启动时创建并从磁盘读出的实例，ThemeMode 还是旧值；
            // 标题栏切换后的最新主题只存在 App.CurrentThemeMode 里，
            // 关闭前必须同步，否则保存会把用户刚选的主题覆盖回启动值。
            vm.Settings.ThemeMode = App.CurrentThemeMode;
            vm.Settings.SaveSettings();
        }
        base.OnClosing(e);
    }
}
