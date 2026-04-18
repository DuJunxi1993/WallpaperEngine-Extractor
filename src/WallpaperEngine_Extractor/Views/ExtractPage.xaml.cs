using System.Diagnostics;
using System.Linq;
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

    /// <summary>
    /// 拖放进入事件 - 当文件拖入到文件选择区域时触发
    /// 高亮边框以提示用户可以释放文件
    /// </summary>
    private void FileSelectionBorder_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            FileSelectionBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215));
            FileSelectionBorder.BorderThickness = new Thickness(2);
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    /// <summary>
    /// 拖放离开事件 - 当文件拖离文件选择区域时触发
    /// 恢复边框原样
    /// </summary>
    private void FileSelectionBorder_DragLeave(object sender, DragEventArgs e)
    {
        FileSelectionBorder.BorderBrush = (Brush)FindResource("ControlStrokeColorDefaultBrush");
        FileSelectionBorder.BorderThickness = new Thickness(1);
        e.Handled = true;
    }

    /// <summary>
    /// 拖放悬停事件 - 持续触发，用于更新拖放效果
    /// </summary>
    private void FileSelectionBorder_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>
    /// 拖放释放事件 - 当用户在文件选择区域释放文件时触发
    /// 处理文件/文件夹的添加，支持批量添加
    /// </summary>
    private void FileSelectionBorder_Drop(object sender, DragEventArgs e)
    {
        // 恢复边框样式
        FileSelectionBorder.BorderBrush = (Brush)FindResource("ControlStrokeColorDefaultBrush");
        FileSelectionBorder.BorderThickness = new Thickness(1);

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items != null && DataContext is ExtractViewModel vm)
            {
                // 展开文件夹为PKG文件列表
                var files = ExpandFolders(items);
                if (vm.HasSelectedFiles)
                    vm.AddFiles(files);
                else
                    vm.SetFiles(files);
            }
        }
        e.Handled = true;
    }

    /// <summary>
    /// 浏览文件按钮点击事件
    /// 打开文件选择对话框，支持多选PKG文件
    /// </summary>
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

    /// <summary>
    /// 浏览文件夹按钮点击事件
    /// 打开文件夹选择对话框，选择包含PKG文件的文件夹
    /// </summary>
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

    /// <summary>
    /// 大文件列表选择改变事件
    /// 当用户选择列表中的文件时，更新预览图片
    /// </summary>
    private void LargeFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ExtractViewModel vm)
        {
            var selectedItem = LargeFilesList.SelectedItem as LargeFileInfo;
            vm.OnLargeFileSelected(selectedItem);
        }
    }

    /// <summary>
    /// 打开输出文件夹按钮点击事件
    /// 使用资源管理器打开当前设置的输出目录
    /// </summary>
    private void OpenOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ExtractViewModel vm)
        {
            if (System.IO.Directory.Exists(vm.OutputDirectory))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = vm.OutputDirectory,
                    UseShellExecute = true
                });
            }
        }
    }

    /// <summary>
    /// 展开文件夹为PKG文件数组
    /// 递归搜索所有子文件夹中的*.pkg文件
    /// </summary>
    /// <param name="items">文件或文件夹路径数组</param>
    /// <returns>PKG文件路径数组</returns>
    private string[] ExpandFolders(string[] items)
    {
        var result = new System.Collections.Generic.List<string>();
        foreach (var item in items)
        {
            if (System.IO.Directory.Exists(item))
            {
                // 递归获取所有PKG文件
                var files = System.IO.Directory.GetFiles(item, "*.pkg", System.IO.SearchOption.AllDirectories);
                result.AddRange(files);
            }
            else if (System.IO.File.Exists(item))
            {
                result.Add(item);
            }
        }
        return result.ToArray();
    }
}