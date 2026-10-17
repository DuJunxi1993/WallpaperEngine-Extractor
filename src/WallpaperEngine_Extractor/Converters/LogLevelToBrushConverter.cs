using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WallpaperEngine_Extractor.ViewModels;

namespace WallpaperEngine_Extractor.Converters;

/// <summary>
/// 将 <see cref="LogLevel"/> 映射到主题状态笔刷。
/// Info    →  InfoBrush
/// Success →  SuccessBrush
/// Warning →  WarningBrush
/// Error   →  ErrorBrush
/// </summary>
public class LogLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = value switch
        {
            LogLevel.Success => "SuccessBrush",
            LogLevel.Warning => "WarningBrush",
            LogLevel.Error   => "ErrorBrush",
            _                => "InfoBrush",
        };

        if (System.Windows.Application.Current?.Resources[key] is Brush brush)
            return brush;

        // 兜底颜色
        return key switch
        {
            "SuccessBrush" => new SolidColorBrush(Color.FromRgb(0x6C, 0xCB, 0x5F)),
            "WarningBrush" => new SolidColorBrush(Color.FromRgb(0xFC, 0xE1, 0x00)),
            "ErrorBrush"   => new SolidColorBrush(Color.FromRgb(0xFF, 0x99, 0xA4)),
            _              => new SolidColorBrush(Color.FromRgb(0x60, 0xCD, 0xFF)),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
