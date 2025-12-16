using System.Windows;
using FontStyle = System.Windows.FontStyle;

public class AppSettings
{
    public string FilePath { get; set; } = "";
    public string Separator { get; set; } = "%";
    public int IntervalSeconds { get; set; } = 60;
    public double FontSize { get; set; } = 24.0;
    public FontWeight FontWeight { get; set; } = FontWeights.Normal;
    public FontStyle FontStyle { get; set; } = FontStyles.Normal;

    // Color 和 Brush 无法直接被 System.Text.Json 序列化。
    // 我们将其存储为字符串（十六进制颜色代码 #AARRGGBB）。
    public string TextColorHex { get; set; } = "#FFFFFFFF"; // 默认白色
    public string StrokeColorHex { get; set; } = "#FF000000"; // 默认黑色
    public bool IsStrokeEnabled { get; set; } = true;
}