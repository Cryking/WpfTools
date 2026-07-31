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

            SetInputState(false);

            // 触发提醒设置更新事件
            ReminderSettingsUpdated?.Invoke(minutes, message);
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
            SetInputState(true);

            // 触发停止提醒事件（间隔为0表示停止）
            ReminderSettingsUpdated?.Invoke(0, string.Empty);

            System.Windows.MessageBox.Show("提醒已停止", "提醒设置", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 切换输入控件与按钮的启用状态
        /// </summary>
        /// <param name="editable">是否处于可编辑（未启动提醒）状态</param>
        private void SetInputState(bool editable)
        {
            btnStart.IsEnabled = editable;
            btnStop.IsEnabled = !editable;
            txtMinutes.IsEnabled = editable;
            txtMessage.IsEnabled = editable;
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