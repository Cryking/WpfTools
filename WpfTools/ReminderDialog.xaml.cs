using System;
using System.Windows;

namespace WpfTools
{
    /// <summary>
    /// ReminderDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ReminderDialog : Window
    {
        public event Action<int, string> ReminderSettingsUpdated;
        
        private bool _isReminderActive = false;

        public ReminderDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 开始提醒
        /// </summary>
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtMinutes.Text, out int minutes) || minutes <= 0)
            {
                System.Windows.MessageBox.Show("请输入有效的分钟数", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string message = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                System.Windows.MessageBox.Show("请输入提醒内容", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isReminderActive = true;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            txtMinutes.IsEnabled = false;
            txtMessage.IsEnabled = false;

            // 触发提醒设置更新事件
            if (ReminderSettingsUpdated != null)
            {
                ReminderSettingsUpdated(minutes, message);
            }            
        }

        /// <summary>
        /// 停止提醒
        /// </summary>
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopReminder();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 停止提醒功能
        /// </summary>
        private void StopReminder()
        {
            _isReminderActive = false;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            txtMinutes.IsEnabled = true;
            txtMessage.IsEnabled = true;

            // 触发停止提醒事件
            if (ReminderSettingsUpdated != null)
            {
                ReminderSettingsUpdated(0, string.Empty);
            }
            
            System.Windows.MessageBox.Show("提醒已停止", "提醒设置", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 窗口加载时设置按钮状态
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
        }
    }
}