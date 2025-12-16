using Microsoft.Win32; // 用于文件对话框和注册表
using System;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace QuotesWidget
{
    public partial class SettingsWindow : Window
    {
        private MainWindow _main;
        private const string AppName = "MyQuotesWidget";

        public SettingsWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;

            // 初始化UI数据
            TxtFilePath.Text = MainWindow.Settings.FilePath;
            TxtSeparator.Text = MainWindow.Settings.Separator;
            TxtInterval.Text = MainWindow.Settings.IntervalSeconds.ToString();

            // 检查开机自启状态
            ChkAutoStart.IsChecked = CheckAutoStart();
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Text files (*.txt)|*.txt";
            if (dlg.ShowDialog() == true)
            {
                TxtFilePath.Text = dlg.FileName;
            }
            // 修正点 3：确保在选择文件后立即更新设置对象，防止忘记点击保存
            MainWindow.Settings.FilePath = TxtFilePath.Text;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 修正点 3：从 UI 控件读取数据并更新设置对象
            MainWindow.Settings.FilePath = TxtFilePath.Text;
            MainWindow.Settings.Separator = TxtSeparator.Text;

            if (int.TryParse(TxtInterval.Text, out int val))
                MainWindow.Settings.IntervalSeconds = val;

            _main.SaveSettings(); // 调用主窗口保存并刷新
            this.Close();
        }

        // --- 开机自启逻辑 ---
        private void ChkAutoStart_Click(object sender, RoutedEventArgs e)
        {
            SetAutoStart(ChkAutoStart.IsChecked == true);
        }

        private bool CheckAutoStart()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                return key.GetValue(AppName) != null;
            }
        }

        private void SetAutoStart(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (enable)
                    {
                        // 修正：使用 EXE 的路径（确保在发布后路径是正确的）
                        string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                      AppDomain.CurrentDomain.FriendlyName.Replace(".dll", ".exe"));
                        // 对于单文件部署或打包后的exe，需要确保路径正确。最安全的方式是：
                        string finalPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                        key.SetValue(AppName, finalPath);
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置开机启动失败: " + ex.Message);
            }
        }
    }
}