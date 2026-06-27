using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WallpaperEngine_Extractor.Converters;

/// <summary>
/// 集合非空 → Visible，否则 Collapsed
/// 用于显示"有内容时显示，空时隐藏"的区域（如大文件整理面板）
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int count = 0;
        if (value is int i) count = i;
        else if (value is ICollection col) count = col.Count;
        else if (value is IEnumerable enumerable)
        {
            foreach (var _ in enumerable) { count++; break; }
        }
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}