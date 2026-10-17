using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using WallpaperEngine_Extractor.ViewModels;

namespace WallpaperEngine_Extractor.Views;

/// <summary>
/// 提取页面 - PKG文件提取功能的主要界面
/// 支持拖放文件/文件夹、选择多个文件、设置输出目录等功能
/// </summary>
public partial class ExtractPage : Page
{
    public ExtractPage()
    {
        InitializeComponent();

        // 日志面板自动滚动：新日志进来时滚到底部，始终显示最新一行。
        Loaded += ExtractPage_Loaded;
        Unloaded += ExtractPage_Unloaded;
    }

    private void ExtractPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ExtractViewModel vm)
            return;

        // 先退订再订阅，避免 Loaded 多次触发导致重复订阅
        vm.LogEntries.CollectionChanged -= LogEntries_CollectionChanged;
        vm.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
    }

    private void ExtractPage_Unloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ExtractViewModel vm)
            vm.LogEntries.CollectionChanged -= LogEntries_CollectionChanged;
    }

    /// <summary>
    /// 日志集合变化：追加新条目后滚动到底部。
    /// 用 Background 优先级延后一帧，等 ItemsControl 把新项布局进可视树后再滚，
    /// 否则 ScrollToEnd 可能滚不到最后一项。
    /// </summary>
    private void LogEntries_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add)
            return;

        Dispatcher.BeginInvoke(
            new Action(() => LogScrollViewer?.ScrollToEnd()),
            DispatcherPriority.Background);
    }

    // ============== 拖放事件 ==============

    private void FileSelectionBorder_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            HighlightDragBorder(true);
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void FileSelectionBorder_DragLeave(object sender, DragEventArgs e)
    {
        HighlightDragBorder(false);
        e.Handled = true;
    }

    private void FileSelectionBorder_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void FileSelectionBorder_Drop(object sender, DragEventArgs e)
    {
        HighlightDragBorder(false);

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items != null && DataContext is ExtractViewModel vm)
            {
                if (vm.HasSelectedFiles)
                    vm.AddFiles(items);
                else
                    vm.SetFiles(items);
            }
        }
        e.Handled = true;
    }

    /// <summary>
    /// 高亮/恢复拖放边框，使用主题感知画笔
    /// </summary>
    private void HighlightDragBorder(bool highlight)
    {
        if (highlight)
        {
            FileSelectionBorder.BorderBrush = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            FileSelectionBorder.BorderThickness = new Thickness(2);
        }
        else
        {
            FileSelectionBorder.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
            FileSelectionBorder.BorderThickness = new Thickness(1);
        }
    }

    // ============== 浏览按钮 ==============

    /// <summary>
    /// "添加"按钮：打开选择文件 / 选择文件夹的上下文菜单。
    /// OpenFileDialog 与 OpenFolderDialog 无法同时选文件和目录，所以用一个菜单让用户二选一。
    /// </summary>
    private void ShowAddMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.ContextMenu is not ContextMenu menu)
            return;

        menu.PlacementTarget = element;
        menu.Placement = PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    private void BrowseFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择 PKG 文件",
            Filter = "PKG 文件 (*.pkg)|*.pkg|所有文件 (*.*)|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true && DataContext is ExtractViewModel vm)
        {
            if (vm.HasSelectedFiles)
                vm.AddFiles(dialog.FileNames);
            else
                vm.SetFiles(dialog.FileNames);
        }
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "选择包含 PKG 文件的文件夹",
            // 文件夹支持 Ctrl/Shift 多选，返回 FolderNames 数组
            Multiselect = true
        };

        if (dialog.ShowDialog() == true && DataContext is ExtractViewModel vm)
        {
            var folders = dialog.FolderNames;
            if (folders.Length == 0)
                return;

            if (vm.HasSelectedFiles)
                vm.AddFiles(folders);
            else
                vm.SetFiles(folders);
        }
    }

    // ============== 列表选择 ==============

    private void LargeFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ExtractViewModel vm)
        {
            // 同步 ListBox.SelectedItems 到每个 LargeFileInfo.IsSelected
            // 这样 Shift+Click 范围选取能反映到 VM 的选中状态
            foreach (var item in vm.LargeFiles)
                item.IsSelected = LargeFilesList.SelectedItems.Contains(item);

            // 单项预览（取第一个选中的）
            var firstSelected = LargeFilesList.SelectedItem as LargeFileInfo;
            vm.OnLargeFileSelected(firstSelected);
        }
    }

    // ============== 预览 ==============

    // 16:9 比例由 PreviewBorder 内的 <Viewbox Stretch="Uniform"> 自动保持，
    // 不需要在 SizeChanged 里手算高比例 —— 旧版本的 PreviewContainer_SizeChanged 已删除。
}