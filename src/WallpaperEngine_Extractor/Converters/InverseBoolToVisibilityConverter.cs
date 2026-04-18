using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WallpaperEngine_Extractor.Converters;

/// <summary>
/// 布尔值取反转换器
/// 将布尔值转换为可视性（Visibility）
/// True -> Collapsed（隐藏）
/// False -> Visible（显示）
/// 用于实现"取反"的显示逻辑，例如文件列表为空时显示拖放提示
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 正向转换：bool -> Visibility
    /// True返回Collapsed，False返回Visible
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    /// <summary>
    /// 反向转换：Visibility -> bool
    /// Visible返回False，Collapsed返回True
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }
        return false;
    }
}