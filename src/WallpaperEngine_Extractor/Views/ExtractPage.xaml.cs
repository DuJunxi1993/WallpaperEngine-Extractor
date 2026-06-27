using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            Title = "选择包含 PKG 文件的文件夹"
        };

        if (dialog.ShowDialog() == true && DataContext is ExtractViewModel vm)
        {
            var files = new[] { dialog.FolderName };
            if (vm.HasSelectedFiles)
                vm.AddFiles(files);
            else
                vm.SetFiles(files);
        }
    }

    // ============== 列表选择 ==============

    private void LargeFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ExtractViewModel vm)
        {
            var selectedItem = LargeFilesList.SelectedItem as LargeFileInfo;
            vm.OnLargeFileSelected(selectedItem);
        }
    }

    // ============== 预览 16:9 比例 ==============

    private void PreviewContainer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (PreviewBorder == null) return;
        var w = e.NewSize.Width;
        if (double.IsNaN(w) || double.IsInfinity(w) || w <= 0) return;
        var h = w * 9.0 / 16.0;
        if (Math.Abs(PreviewBorder.Height - h) > 0.5)
            PreviewBorder.Height = h;
    }
}