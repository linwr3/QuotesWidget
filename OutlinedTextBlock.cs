using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using FlowDirection = System.Windows.FlowDirection;

namespace QuotesWidget
{
    public class OutlinedTextBlock : FrameworkElement
    {
        // 依赖属性：文本、字体大小、粗体、斜体、填充色、描边色、描边粗细
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsStrokeEnabledProperty = DependencyProperty.Register("IsStrokeEnabled", typeof(bool), typeof(OutlinedTextBlock), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
        public FontWeight FontWeight { get => (FontWeight)GetValue(FontWeightProperty); set => SetValue(FontWeightProperty, value); }
        public FontStyle FontStyle { get => (FontStyle)GetValue(FontStyleProperty); set => SetValue(FontStyleProperty, value); }
        public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }
        public Brush Stroke { get => (Brush)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
        public double StrokeThickness { get => (double)GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }
        public bool IsStrokeEnabled { get => (bool)GetValue(IsStrokeEnabledProperty); set => SetValue(IsStrokeEnabledProperty, value); }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (string.IsNullOrEmpty(Text)) return;

            // 创建格式化文本
            //var formattedText = new FormattedText(
            //    Text,
            //    CultureInfo.CurrentCulture,
            //    FlowDirection.LeftToRight,
            //    new Typeface(new FontFamily("Microsoft YaHei"), FontStyle, FontWeight, FontStretches.Normal),
            //    FontSize,
            //    Brushes.Black, // 这里的Brush不重要，因为我们用Geometry
            //    VisualTreeHelper.GetDpi(this).PixelsPerDip);
            var formattedText = GetFormattedText(this.ActualWidth);

            formattedText.TextAlignment = TextAlignment.Center; // 居中

            // 转换为几何图形
            var geometry = formattedText.BuildGeometry(new Point(0, 0));

            // 绘制
            var stroke = IsStrokeEnabled ? Stroke : Brushes.Transparent;
            var strokeThick = IsStrokeEnabled ? StrokeThickness : 0;

            // 为了保证描边在文字外围，通常先画描边
            drawingContext.DrawGeometry(Fill, new Pen(stroke, strokeThick), geometry);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //var formattedText = new FormattedText(
            //    Text ?? "",
            //    CultureInfo.CurrentCulture,
            //    FlowDirection.LeftToRight,
            //    new Typeface(new FontFamily("Microsoft YaHei"), FontStyle, FontWeight, FontStretches.Normal),
            //    FontSize,
            //    Brushes.Black,
            //    VisualTreeHelper.GetDpi(this).PixelsPerDip);
            var formattedText = GetFormattedText(availableSize.Width);
            return new Size(formattedText.Width + 5, formattedText.Height + 5); // 稍微留点余量
        }
        private FormattedText GetFormattedText(double width)
        {
            return new FormattedText(
                Text ?? "",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Microsoft YaHei"), FontStyle, FontWeight, FontStretches.Normal),
                FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip)
            {
                // 设置最大宽度，以实现自动换行和居中对齐
                MaxTextWidth = width,
                TextAlignment = TextAlignment.Center
            };
        }
    }
}