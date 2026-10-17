using System;
using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace WallpaperEngine_Extractor.Converters;

/// <summary>
/// 将 <see cref="AppThemeMode"/> 映射到 WPF-UI 的 <see cref="SymbolRegular"/>。
/// Light → 太阳 (WeatherSunny24)
/// Dark  → 月亮 (WeatherMoon24)
/// Auto  → 调色板 (Color24) — 代表"自动跟随系统"
/// </summary>
public class ThemeModeToSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AppThemeMode mode)
        {
            return mode switch
            {
                AppThemeMode.Light => SymbolRegular.WeatherSunny24,
                AppThemeMode.Dark  => SymbolRegular.WeatherMoon24,
                _                  => SymbolRegular.Color24, // Auto
            };
        }
        return SymbolRegular.Color24;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
