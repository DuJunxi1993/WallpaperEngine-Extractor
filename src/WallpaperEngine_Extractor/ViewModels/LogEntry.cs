using CommunityToolkit.Mvvm.ComponentModel;

namespace WallpaperEngine_Extractor.ViewModels;

/// <summary>
/// 日志条目等级。<see cref="LogLevelToBrushConverter"/> 会按该枚举查主题状态笔刷。
/// </summary>
public enum LogLevel
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>
/// 一条提取日志。包含时间戳、等级与原文。
/// </summary>
public partial class LogEntry : ObservableObject
{
    public DateTime Timestamp { get; }
    public LogLevel Level { get; }
    public string Message { get; }

    public LogEntry(LogLevel level, string message, DateTime? timestamp = null)
    {
        Level = level;
        Message = message;
        Timestamp = timestamp ?? DateTime.Now;
    }

    /// <summary>用于 UI 显示的格式化时间戳（HH:mm:ss）</summary>
    public string TimeText => Timestamp.ToString("HH:mm:ss");
}
