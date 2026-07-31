using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing;
using WpfTools.Tools;
using System.Windows.Threading;

namespace WpfTools
{
    /// <summary>
    /// 主窗口类 - 多功能工具箱应用主界面
    /// 包含JSON格式化、时间戳转换、字符串/图片Base64转换及定时提醒功能
    /// </summary>
    public partial class MainWindow : Window
    {
        // 剪贴板粘贴的图片是InteropBitmap，故使用基类BitmapSource存储
        private BitmapSource _currentImage;
        private NotifyIcon _notifyIcon;
        private DispatcherTimer _reminderTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeEventHandlers();
            InitializeSystemTray();
        }

        /// <summary>
        /// 初始化事件处理器（为文本框启用文件拖拽）
        /// </summary>
        private void InitializeEventHandlers()
        {
            // TextBox默认拦截拖拽，需在PreviewDragOver中标记已处理才Drop才能触发
            txtJsonInput.AllowDrop = true;
            txtJsonInput.PreviewDragOver += OnFilePreviewDragOver;
            txtJsonInput.PreviewDrop += TxtJsonInput_Drop;
            txtBase64.AllowDrop = true;
            txtBase64.PreviewDragOver += OnFilePreviewDragOver;
            txtBase64.PreviewDrop += TxtBase64_Drop;
        }

        /// <summary>
        /// 拖拽经过时显示复制光标并接管默认行为
        /// </summary>
        private void OnFilePreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)
                ? System.Windows.DragDropEffects.Copy
                : System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        /// 复制文本到剪贴板并给出提示（统一处理空内容和异常）
        /// </summary>
        private static void CopyToClipboard(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                System.Windows.MessageBox.Show("没有内容可复制", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                System.Windows.Clipboard.SetText(text);
                System.Windows.MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 处理TreeView中TextBlock的鼠标左键点击事件
        /// 用于实现长字符串的折叠/展开功能（节点内部通知UI自动刷新）
        /// </summary>
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((sender as TextBlock)?.Tag as JsonTreeNode)?.ToggleLongText();
        }

        #region JSON工具相关事件处理

        /// <summary>
        /// 清空JSON输入框
        /// </summary>
        private void BtnClearJsonInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtJsonInput.Clear();
                tvJsonOutput.ItemsSource = null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"清空失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 格式化JSON字符串并显示到TreeView
        /// </summary>
        private void BtnFormatJson_Click(object sender, RoutedEventArgs e)
        {
            string input = txtJsonInput.Text;
            if (!JsonFormatter.TryParse(input, out string error))
            {
                System.Windows.MessageBox.Show(error, "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DisplayJsonInTreeView(input);
        }

        /// <summary>
        /// 压缩JSON字符串（结果回填到输入框，便于直接复制）
        /// </summary>
        private void BtnCompressJson_Click(object sender, RoutedEventArgs e)
        {
            string input = txtJsonInput.Text;
            if (!JsonFormatter.TryParse(input, out string error))
            {
                System.Windows.MessageBox.Show(error, "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            txtJsonInput.Text = JsonFormatter.CompressJson(input);
            DisplayJsonInTreeView(input);
        }

        /// <summary>
        /// 验证JSON字符串
        /// </summary>
        private void BtnValidateJson_Click(object sender, RoutedEventArgs e)
        {
            string input = txtJsonInput.Text;
            bool isValid = JsonFormatter.TryParse(input, out _);

            if (isValid)
                DisplayJsonInTreeView(input);

            System.Windows.MessageBox.Show(JsonFormatter.ValidateJson(input), "验证结果",
                MessageBoxButton.OK, isValid ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        /// <summary>
        /// 复制JSON处理结果到剪贴板
        /// </summary>
        private void BtnCopyJsonResult_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(GetFormattedJsonResult());
        }

        /// <summary>
        /// 将JSON字符串显示到TreeView
        /// </summary>
        /// <param name="jsonString">JSON字符串</param>
        private void DisplayJsonInTreeView(string jsonString)
        {
            try
            {
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
                var rootNode = JsonTreeNode.CreateFromJsonElement(jsonDoc.RootElement, "Root");
                tvJsonOutput.ItemsSource = new System.Collections.Generic.List<JsonTreeNode> { rootNode };
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"显示JSON失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 获取当前格式化结果（TreeView有内容时按输入重新美化输出）
        /// </summary>
        /// <returns>格式化后的JSON字符串，无内容时返回空字符串</returns>
        private string GetFormattedJsonResult()
        {
            string input = txtJsonInput.Text;
            return tvJsonOutput.ItemsSource != null && JsonFormatter.TryParse(input, out _)
                ? JsonFormatter.FormatJson(input)
                : string.Empty;
        }

        /// <summary>
        /// 打开提醒设置窗口
        /// </summary>
        private void BtnReminderSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenReminderDialog();
        }

        /// <summary>
        /// 处理JSON输入框的文件拖拽
        /// </summary>
        private void TxtJsonInput_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            try
            {
                txtJsonInput.Text = File.ReadAllText(files[0]);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"读取文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 时间戳转换相关事件处理

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        private void BtnGetCurrentTimestamp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string result = TimestampConverter.GetCurrentTimestamp();
                txtTimestampResult.Text = result;
            }
            catch (Exception ex)
            {
                txtTimestampResult.Text = $"获取时间戳失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 清空时间戳输入框
        /// </summary>
        private void BtnClearTimestampInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtTimestampInput.Clear();
                txtTimestampResult.Clear();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"清空输入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 自动检测并转换时间戳或日期时间
        /// </summary>
        private void BtnAutoDetectAndConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = txtTimestampInput.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    txtTimestampResult.Text = "请输入时间戳或日期时间";
                    return;
                }

                string result = "自动检测结果:\n\n";
                
                // 尝试判断是否为时间戳
                if (IsTimestamp(input))
                {
                    result += "检测到输入为时间戳:\n";
                    result += TimestampConverter.AutoDetectTimestamp(input) + "\n\n";
                }
                
                // 尝试判断是否为日期时间
                result += "尝试将输入作为日期时间转换为时间戳:\n";
                result += TimestampConverter.DateTimeToTimestamp(input);
                
                txtTimestampResult.Text = result;
            }
            catch (Exception ex)
            {
                txtTimestampResult.Text = $"转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 时间戳转日期时间
        /// </summary>
        private void BtnTimestampToDateTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = txtTimestampInput.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    txtTimestampResult.Text = "请输入时间戳";
                    return;
                }

                string result = TimestampConverter.AutoDetectTimestamp(input);
                txtTimestampResult.Text = result;
            }
            catch (Exception ex)
            {
                txtTimestampResult.Text = $"时间戳转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 日期时间转时间戳
        /// </summary>
        private void BtnDateTimeToTimestamp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = txtTimestampInput.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    txtTimestampResult.Text = "请输入日期时间字符串";
                    return;
                }

                string result = TimestampConverter.DateTimeToTimestamp(input);
                txtTimestampResult.Text = result;
            }
            catch (Exception ex)
            {
                txtTimestampResult.Text = $"日期时间转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 复制时间戳转换结果到剪贴板
        /// </summary>
        private void BtnCopyTimestampResult_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(txtTimestampResult.Text);
        }
        
        /// <summary>
        /// 判断输入是否为时间戳（纯数字且10~13位）
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>是否为时间戳</returns>
        private static bool IsTimestamp(string input)
        {
            string trimmed = input?.Trim() ?? string.Empty;
            return trimmed.Length >= 10 && trimmed.Length <= 13 && long.TryParse(trimmed, out _);
        }

        #endregion

        #region 字符串Base64转换相关事件处理

        /// <summary>
        /// 清空字符串Base64转换输入框
        /// </summary>
        private void BtnClearStringInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStringInput.Clear();
                txtStringResult.Clear();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"清空输入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 自动检测并转换字符串或Base64
        /// </summary>
        private void BtnAutoDetectStringConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = txtStringInput.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    txtStringResult.Text = "请输入字符串或Base64";
                    return;
                }

                string encodingName = (cmbStringEncoding.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "UTF-8";
                string result = "自动检测结果:\n\n";
                
                // 尝试判断是否为Base64
                if (StringBase64Converter.IsBase64String(input))
                {
                    result += "检测到输入可能为Base64，尝试解码:\n";
                    result += StringBase64Converter.DecodeFromBase64(input, encodingName) + "\n\n";
                    
                    // 也提供多编码尝试解码的结果
                    result += "多编码尝试解码结果:\n";
                    result += StringBase64Converter.TryDecodeWithMultipleEncodings(input);
                }
                else
                {
                    result += "检测到输入可能为普通字符串，尝试编码为Base64:\n";
                    result += StringBase64Converter.EncodeToBase64(input, encodingName);
                }
                
                txtStringResult.Text = result;
            }
            catch (Exception ex)
            {
                txtStringResult.Text = $"自动转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 将字符串编码为Base64
        /// </summary>
        private void BtnStringToBase64_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = txtStringInput.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    txtStringResult.Text = "请输入要编码的字符串";
                    return;
                }

                string encodingName = (cmbStringEncoding.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "UTF-8";
                string result = StringBase64Converter.EncodeToBase64(input, encodingName);
                txtStringResult.Text = result;
            }
            catch (Exception ex)
            {
                txtStringResult.Text = $"Base64编码失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 将Base64字符串解码为普通字符串
        /// </summary>
        private void BtnBase64ToString_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = txtStringInput.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    txtStringResult.Text = "请输入要解码的Base64字符串";
                    return;
                }

                string encodingName = (cmbStringEncoding.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "UTF-8";
                string result = StringBase64Converter.DecodeFromBase64(input, encodingName);
                txtStringResult.Text = result;
            }
            catch (Exception ex)
            {
                txtStringResult.Text = $"Base64解码失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 尝试使用多种编码解码Base64字符串
        /// </summary>
        private void BtnTryMultipleDecodings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = txtStringInput.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    txtStringResult.Text = "请输入要解码的Base64字符串";
                    return;
                }

                string result = StringBase64Converter.TryDecodeWithMultipleEncodings(input);
                txtStringResult.Text = result;
            }
            catch (Exception ex)
            {
                txtStringResult.Text = $"多编码尝试解码失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 复制字符串Base64转换结果到剪贴板
        /// </summary>
        private void BtnCopyStringResult_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(txtStringResult.Text);
        }

        #endregion

        #region 图片/Base64转换相关事件处理

        /// <summary>
        /// 选择图片文件
        /// </summary>
        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            string imagePath = ImageBase64Converter.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(imagePath))
            {
                LoadImageFromFile(imagePath);
            }
        }

        /// <summary>
        /// 粘贴图片（剪贴板图片为BitmapSource，直接存储避免类型转换丢失）
        /// </summary>
        private void BtnPasteImage_Click(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Clipboard.ContainsImage())
            {
                System.Windows.MessageBox.Show("剪贴板中没有图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var image = System.Windows.Clipboard.GetImage();
                if (image != null)
                {
                    imgPreview.Source = image;
                    _currentImage = image;
                    AutoConvertImageToBase64();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"粘贴图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 清空图片
        /// </summary>
        private void BtnClearImage_Click(object sender, RoutedEventArgs e)
        {
            imgPreview.Source = null;
            _currentImage = null;
            txtBase64.Clear();
        }

        /// <summary>
        /// 图片转Base64
        /// </summary>
        private void BtnImageToBase64_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImage != null)
            {
                string base64 = ImageBase64Converter.BitmapImageToBase64(_currentImage);
                txtBase64.Text = base64;
            }
            else
            {
                System.Windows.MessageBox.Show("请先选择或粘贴图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Base64转图片
        /// </summary>
        private void BtnBase64ToImage_Click(object sender, RoutedEventArgs e)
        {
            string base64 = txtBase64.Text;
            if (!string.IsNullOrWhiteSpace(base64))
            {
                var bitmap = ImageBase64Converter.Base64ToBitmapImage(base64);
                if (bitmap != null)
                {
                    imgPreview.Source = bitmap;
                    _currentImage = bitmap;
                }
                else
                {
                    System.Windows.MessageBox.Show("Base64字符串无效或转换失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("请输入Base64字符串", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 复制Base64字符串到剪贴板
        /// </summary>
        private void BtnCopyBase64_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(txtBase64.Text);
        }

        /// <summary>
        /// 处理Base64文本框的文件拖拽（仅接受图片文件）
        /// </summary>
        private void TxtBase64_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            e.Handled = true;
            if (IsImageFile(Path.GetExtension(files[0]).ToLower()))
                LoadImageFromFile(files[0]);
            else
                System.Windows.MessageBox.Show("请拖拽图片文件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 从文件加载图片
        /// </summary>
        /// <param name="imagePath">图片文件路径</param>
        private void LoadImageFromFile(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new System.Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                imgPreview.Source = bitmap;
                _currentImage = bitmap;

                // 自动转换为Base64
                AutoConvertImageToBase64();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 自动将当前图片转换为Base64
        /// </summary>
        private void AutoConvertImageToBase64()
        {
            if (_currentImage != null)
            {
                string base64 = ImageBase64Converter.BitmapImageToBase64(_currentImage);
                txtBase64.Text = base64;
            }
        }

        /// <summary>
        /// 判断是否为图片文件
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <returns>是否为图片文件</returns>
        private static bool IsImageFile(string extension)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".webp" };
            return imageExtensions.Contains(extension);
        }

        #endregion

        #region 系统托盘功能

        /// <summary>
        /// 初始化系统托盘
        /// </summary>
        private void InitializeSystemTray()
        {
            try
            {
                // 使用项目中的icon.ico文件
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                var icon = new System.Drawing.Icon(iconPath);
                
                _notifyIcon = new NotifyIcon
                {
                    Icon = icon,
                    Text = "多功能工具箱",
                    Visible = true,
                    BalloonTipTitle = "多功能工具箱",
                    BalloonTipText = "应用程序已启动",
                    BalloonTipIcon = ToolTipIcon.Info
                };
            }
            catch
            {
                // 如果加载自定义图标失败，使用默认图标
                _notifyIcon = new NotifyIcon
                {
                    Icon = SystemIcons.Application,
                    Text = "多功能工具箱",
                    Visible = true,
                    BalloonTipTitle = "多功能工具箱",
                    BalloonTipText = "应用程序已启动",
                    BalloonTipIcon = ToolTipIcon.Info
                };
            }

            // 设置托盘菜单
            var contextMenu = new ContextMenuStrip();

            var showMenuItem = new ToolStripMenuItem("显示主窗口", null, (s, e) => ShowMainWindow());
            var reminderMenuItem = new ToolStripMenuItem("定时提醒", null, (s, e) => OpenReminderDialog());
            var exitMenuItem = new ToolStripMenuItem("退出程序", null, (s, e) => ExitApplication());

            contextMenu.Items.Add(showMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(reminderMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);

            _notifyIcon.ContextMenuStrip = contextMenu;

            // 双击托盘图标显示主窗口
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

            // 订阅窗口事件
            this.StateChanged += MainWindow_StateChanged;
            this.Closing += MainWindow_Closing;
        }

        /// <summary>
        /// 窗口状态变化事件处理
        /// </summary>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                HideToTray();
            }
        }

        /// <summary>
        /// 窗口关闭事件处理
        /// </summary>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 如果是用户点击关闭按钮，则隐藏到系统托盘而不是关闭应用
            e.Cancel = true;
            HideToTray();
        }

        /// <summary>
        /// 显示主窗口
        /// </summary>
        private void ShowMainWindow()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            this.Show();
            this.Activate();
        }

        /// <summary>
        /// 隐藏到系统托盘
        /// </summary>
        private void HideToTray()
        {
            this.Hide();
        }

        /// <summary>
        /// 退出应用程序（释放托盘图标和定时器资源）
        /// </summary>
        private void ExitApplication()
        {
            _reminderTimer?.Stop();
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region 定时提醒功能

        /// <summary>
        /// 打开提醒设置对话框
        /// </summary>
        private void OpenReminderDialog()
        {
            var dialog = new ReminderDialog();
            dialog.Owner = this;
            dialog.ReminderSettingsUpdated += OnReminderSettingsUpdated;
            dialog.ShowDialog();
        }

        /// <summary>
        /// 提醒设置更新事件处理
        /// </summary>
        private void OnReminderSettingsUpdated(int intervalMinutes, string message)
        {
            SetupReminderTimer(intervalMinutes, message);
        }

        /// <summary>
        /// 设置提醒定时器（intervalMinutes为0时仅停止）
        /// </summary>
        private void SetupReminderTimer(int intervalMinutes, string message)
        {
            // 停止现有定时器
            _reminderTimer?.Stop();

            if (intervalMinutes <= 0) return;

            // 创建新定时器
            _reminderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(intervalMinutes)
            };

            _reminderTimer.Tick += (s, e) => ShowReminderNotification(message);
            _reminderTimer.Start();

            System.Windows.MessageBox.Show($"提醒已设置，每{intervalMinutes}分钟提醒一次{Environment.NewLine}提醒内容:{message}", "提醒设置成功",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 显示提醒通知（托盘气泡提示）
        /// </summary>
        private void ShowReminderNotification(string message)
        {
            if (_notifyIcon == null) return;

            try
            {
                _notifyIcon.BalloonTipTitle = "定时提醒";
                _notifyIcon.BalloonTipText = message;
                _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                _notifyIcon.ShowBalloonTip(10000);
            }
            catch
            {
                // 气泡提示失败不影响主流程，忽略
            }
        }
        #endregion
    }
}
