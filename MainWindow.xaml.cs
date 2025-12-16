using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace QuotesWidget
{
    public partial class MainWindow : Window
    {
        private bool _isLocked = false;
        private bool _isPinned = false;
        private DispatcherTimer _timer;
        private List<string> _quotes = new List<string>();
        private int _currentQuoteIndex = 0;
        public static AppSettings Settings = new AppSettings();

        private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigFile = Path.Combine(AppDirectory, "config.json");
        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            LoadQuotes();

            // 初始化定时器
            _timer = new DispatcherTimer();
            _timer.Tick += (s, e) => NextQuote();
            UpdateTimerInterval();
            _timer.Start();

            // 初始显示第一条
            NextQuote();

            // 鼠标拖动逻辑
            this.MouseLeftButtonDown += (s, e) =>
            {
                if (!_isLocked)
                    this.DragMove();
            };
        }

        // --- 核心逻辑 ---

        private void LoadSettings()
        {
            if (File.Exists(ConfigFile))
            {
                try { Settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(ConfigFile)) ?? new AppSettings(); }
                catch { }
            }
            QuoteText.FontSize = Settings.FontSize;
            QuoteText.FontWeight = Settings.FontWeight;
            QuoteText.FontStyle = Settings.FontStyle;
            QuoteText.IsStrokeEnabled = Settings.IsStrokeEnabled;
            BtnOutlineToggle.IsChecked = Settings.IsStrokeEnabled; // 同步 ToggleButton 状态

            try
            {
                // 从十六进制字符串创建颜色对象
                QuoteText.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(Settings.TextColorHex);
                QuoteText.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom(Settings.StrokeColorHex);
            }
            catch
            {
                // 遇到错误时使用默认值
                QuoteText.Fill = Brushes.White;
                QuoteText.Stroke = Brushes.Black;
            }

            // 确保描边颜色按钮的可见性正确同步
            BtnStrokeColor.Visibility = Settings.IsStrokeEnabled ? Visibility.Visible : Visibility.Collapsed;

            // 确保定时器和文本加载基于当前设置
            UpdateTimerInterval();
        }

        public void SaveSettings()
        {
            // 修正点 3 & 5: 保存设置到EXE同级目录
            File.WriteAllText(ConfigFile, JsonSerializer.Serialize(Settings));
            UpdateTimerInterval();
            LoadQuotes(); // 重新加载内容
            _currentQuoteIndex = -1;
            NextQuote(); // 立即刷新
        }

        private void LoadQuotes()
        {
            _quotes.Clear();
            if (File.Exists(Settings.FilePath))
            {
                var content = File.ReadAllText(Settings.FilePath);
                // 根据分隔符分割，并去除空白行
                _quotes = content.Split(new string[] { Settings.Separator }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => s.Trim())
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .ToList();
            }

            if (_quotes.Count == 0) _quotes.Add("请点击设置\n选择文本文件");
        }

        private void NextQuote()
        {
            if (_quotes.Count == 0) return;
            // 修正：确保第一次调用 NextQuote 时显示第一段文本
            _currentQuoteIndex = (_currentQuoteIndex + 1) % _quotes.Count;
            QuoteText.Text = _quotes[_currentQuoteIndex];
        }

        private void UpdateTimerInterval()
        {
            if(_timer == null) return; // fixed
            // 限制在 10s 到 30分钟 (1800s)
            int sec = Math.Clamp(Settings.IntervalSeconds, 10, 1800);
            _timer.Interval = TimeSpan.FromSeconds(sec);
        }
        private void SyncAndSaveStyles()
        {
            // 字体样式
            Settings.FontSize = QuoteText.FontSize;
            Settings.FontWeight = QuoteText.FontWeight;
            Settings.FontStyle = QuoteText.FontStyle;

            // 描边状态
            Settings.IsStrokeEnabled = QuoteText.IsStrokeEnabled;

            // 颜色（转换为十六进制字符串）
            if (QuoteText.Fill is SolidColorBrush fillBrush)
            {
                Settings.TextColorHex = fillBrush.Color.ToString();
            }
            if (QuoteText.Stroke is SolidColorBrush strokeBrush)
            {
                Settings.StrokeColorHex = strokeBrush.Color.ToString();
            }

            SaveSettings(); // 调用 SaveSettings 方法，将 Settings 对象写入 config.json
        }
        // --- 按钮事件 ---

        private void BtnLock_Click(object sender, RoutedEventArgs e)
        {
            _isLocked = !_isLocked;
            if (_isLocked)
            {
                // 锁定状态：隐藏除锁/置顶外的所有控制，背景变透明/模糊
                TopControlsPanel.Visibility = Visibility.Collapsed;
                StyleControlsPanel.Visibility = Visibility.Collapsed;
                ManualSwitchPanel.Visibility = Visibility.Collapsed;

                // 背景变透明，模拟玻璃效果（纯透明）
                MainBorder.Background = Brushes.Transparent;
                BtnLock.Content = "🔒";
            }
            else
            {
                UnlockUI();
            }
        }

        // 双击窗口任意位置解锁
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_isLocked) UnlockUI();
        }

        private void UnlockUI()
        {
            _isLocked = false;
            // 恢复所有控制的可见性
            TopControlsPanel.Visibility = Visibility.Visible;
            StyleControlsPanel.Visibility = Visibility.Visible;
            ManualSwitchPanel.Visibility = Visibility.Visible;

            // 恢复深灰色背景
            MainBorder.Background = new SolidColorBrush(Color.FromArgb(230, 51, 51, 51));
            BtnLock.Content = "🔓";
        }

        private void BtnPin_Click(object sender, RoutedEventArgs e)
        {
            _isPinned = !_isPinned;
            this.Topmost = _isPinned;
            BtnPin.Background = _isPinned ? Brushes.Orange : Brushes.White;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var win = new SettingsWindow(this);
            win.ShowDialog();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        // --- 样式调整 ---
        private void BtnBold_Click(object sender, RoutedEventArgs e)
        {
            QuoteText.FontWeight = QuoteText.FontWeight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold;
            SyncAndSaveStyles(); // <--- 每次修改后立即保存
        }
        private void BtnItalic_Click(object sender, RoutedEventArgs e)
        {
            QuoteText.FontStyle = QuoteText.FontStyle == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic;
            SyncAndSaveStyles(); // <--- 每次修改后立即保存
        }
        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            QuoteText.FontSize += 2;
            SyncAndSaveStyles(); // <--- 每次修改后立即保存
        }
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            QuoteText.FontSize = Math.Max(10, QuoteText.FontSize - 2);
            SyncAndSaveStyles(); // <--- 每次修改后立即保存
        }

        private void BtnOutlineToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isOn = BtnOutlineToggle.IsChecked ?? false;
            QuoteText.IsStrokeEnabled = isOn;
            BtnStrokeColor.Visibility = isOn ? Visibility.Visible : Visibility.Collapsed;
            SyncAndSaveStyles();
        }

        // 调用 Windows Forms 的颜色选择器
        private void BtnTextColor_Click(object sender, RoutedEventArgs e)
        {
            var color = PickColor();
            if (color.HasValue)
            {
                QuoteText.Fill = new SolidColorBrush(color.Value);
                SyncAndSaveStyles(); // <--- 每次修改后立即保存
            }
        }

        private void BtnStrokeColor_Click(object sender, RoutedEventArgs e)
        {
            var color = PickColor();
            if (color.HasValue) QuoteText.Stroke = new SolidColorBrush(color.Value);
        }

        private Color? PickColor()
        {
            // 需在项目属性引用 System.Windows.Forms
            using (var dialog = new System.Windows.Forms.ColorDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
                }
            }
            return null;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_quotes.Count == 0) return;
            _currentQuoteIndex = (_currentQuoteIndex - 1 + _quotes.Count) % _quotes.Count;
            QuoteText.Text = _quotes[_currentQuoteIndex];
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            NextQuote(); // 复用 NextQuote 逻辑
        }
    }
}