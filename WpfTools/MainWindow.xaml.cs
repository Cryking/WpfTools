using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
    /// 包含JSON格式化和图片/Base64转换功能
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage _currentImage;
        private NotifyIcon _notifyIcon;
        private DispatcherTimer _reminderTimer;

        // Win32 API 用于闪烁窗口
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public MainWindow()
        {
            InitializeComponent();
            InitializeEventHandlers();
            InitializeSystemTray();
            RequestNotificationPermission();
        }

        /// <summary>
        /// 请求通知权限
        /// </summary>
        private void RequestNotificationPermission()
        {
            try
            {
                // 在Windows 10/11中，应用程序需要通知权限才能显示通知
                // 这是一个简单的实现，实际应用中可能需要更复杂的处理
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                if (helper.Handle != IntPtr.Zero)
                {
                    // 确保窗口已经创建
                    this.Show();
                    this.Hide(); // 立即隐藏，不影响启动时的显示逻辑
                }
            }
            catch
            {
                // 忽略权限请求失败
            }
        }

        /// <summary>
        /// 初始化事件处理器
        /// </summary>
        private void InitializeEventHandlers()
        {
            // 为文本框添加拖拽支持
            txtJsonInput.AllowDrop = true;
            txtJsonInput.Drop += TxtJsonInput_Drop;
            txtBase64.AllowDrop = true;
            txtBase64.Drop += TxtBase64_Drop;

            // 为JSON输出文本框添加键盘事件处理
            txtJsonOutput.KeyDown += TxtJsonOutput_KeyDown;
        }

        #region JSON工具相关事件处理

        /// <summary>
        /// 清空JSON输入框
        /// </summary>
        private void BtnClearJsonInput_Click(object sender, RoutedEventArgs e)
        {
            txtJsonInput.Clear();
            SetRichTextBoxText(txtJsonOutput, "");
        }

        /// <summary>
        /// 格式化JSON字符串
        /// </summary>
        private void BtnFormatJson_Click(object sender, RoutedEventArgs e)
        {
            string input = txtJsonInput.Text;
            string result = JsonFormatter.FormatJson(input);
            SetRichTextBoxText(txtJsonOutput, result);
        }

        /// <summary>
        /// 压缩JSON字符串
        /// </summary>
        private void BtnCompressJson_Click(object sender, RoutedEventArgs e)
        {
            string input = txtJsonInput.Text;
            string result = JsonFormatter.CompressJson(input);
            SetRichTextBoxText(txtJsonOutput, result);
        }

        /// <summary>
        /// 验证JSON字符串
        /// </summary>
        private void BtnValidateJson_Click(object sender, RoutedEventArgs e)
        {
            string input = txtJsonInput.Text;
            string result = JsonFormatter.ValidateJson(input);

            // 如果验证成功，显示格式化后的结果
            if (result.Contains("✓"))
            {
                string formatted = JsonFormatter.FormatJson(input);
                SetRichTextBoxText(txtJsonOutput, formatted);
            }
            else
            {
                SetRichTextBoxText(txtJsonOutput, result);
            }
        }

        /// <summary>
        /// 复制JSON处理结果到剪贴板
        /// </summary>
        private void BtnCopyJsonResult_Click(object sender, RoutedEventArgs e)
        {
            string text = GetRichTextBoxText(txtJsonOutput);
            if (!string.IsNullOrWhiteSpace(text))
            {
                System.Windows.Clipboard.SetText(text);
                System.Windows.MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    try
                    {
                        string content = File.ReadAllText(files[0]);
                        txtJsonInput.Text = content;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"读取文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
            try
            {
                string text = txtTimestampResult.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    System.Windows.Clipboard.SetText(text);
                    System.Windows.MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("没有内容可复制", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 判断输入是否为时间戳
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>是否为时间戳</returns>
        private bool IsTimestamp(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // 移除可能的空格
            input = input.Trim();

            // 检查是否全为数字
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            // 检查长度，通常时间戳是10位（秒级）或13位（毫秒级）
            return input.Length >= 10 && input.Length <= 13;
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
            try
            {
                string text = txtStringResult.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    System.Windows.Clipboard.SetText(text);
                    System.Windows.MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("没有内容可复制", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region 查找功能相关事件处理

        /// <summary>
        /// 处理JSON输出文本框的键盘事件
        /// </summary>
        private void TxtJsonOutput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // 检测Ctrl+F组合键
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                OpenFindWindow();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 打开查找窗口
        /// </summary>
        private void OpenFindWindow()
        {
            // 创建一个简单的查找对话框
            var dialog = new FindDialog();
            dialog.Owner = this;
            dialog.TargetRichTextBox = txtJsonOutput;
            dialog.ShowDialog();
        }

        #endregion

        #region 图片/Base64转换相关事件处理

        /// <summary>
        /// 选择图片文件
        /// </summary>
        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            string? imagePath = ImageBase64Converter.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(imagePath))
            {
                LoadImageFromFile(imagePath);
            }
        }

        /// <summary>
        /// 粘贴图片
        /// </summary>
        private void BtnPasteImage_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Clipboard.ContainsImage())
            {
                try
                {
                    var image = System.Windows.Clipboard.GetImage();
                    if (image != null)
                    {
                        imgPreview.Source = image;
                        _currentImage = image as BitmapImage;

                        // 自动转换为Base64
                        AutoConvertImageToBase64();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"粘贴图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("剪贴板中没有图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (!string.IsNullOrWhiteSpace(txtBase64.Text))
            {
                System.Windows.Clipboard.SetText(txtBase64.Text);
                System.Windows.MessageBox.Show("Base64字符串已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 处理Base64文本框的文件拖拽
        /// </summary>
        private void TxtBase64_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string extension = Path.GetExtension(files[0]).ToLower();
                    if (IsImageFile(extension))
                    {
                        LoadImageFromFile(files[0]);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("请拖拽图片文件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
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

        /// <summary>
        /// 设置RichTextBox的文本内容
        /// </summary>
        /// <param name="richTextBox">RichTextBox控件</param>
        /// <param name="text">文本内容</param>
        private void SetRichTextBoxText(System.Windows.Controls.RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            if (!string.IsNullOrEmpty(text))
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(text));
                richTextBox.Document.Blocks.Add(paragraph);
            }
        }

        /// <summary>
        /// 获取RichTextBox的文本内容
        /// </summary>
        /// <param name="richTextBox">RichTextBox控件</param>
        /// <returns>文本内容</returns>
        private string GetRichTextBoxText(System.Windows.Controls.RichTextBox richTextBox)
        {
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            return textRange.Text.TrimEnd('\r', '\n');
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
        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                HideToTray();
            }
        }

        /// <summary>
        /// 窗口关闭事件处理
        /// </summary>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
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
            //if (_notifyIcon != null)
            //{
            //    _notifyIcon.BalloonTipTitle = "多功能工具箱";
            //    _notifyIcon.BalloonTipText = "已最小化到系统托盘";
            //    _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            //    _notifyIcon.ShowBalloonTip(3000);
            //}
        }

        /// <summary>
        /// 退出应用程序
        /// </summary>
        private void ExitApplication()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            if (_reminderTimer != null)
            {
                _reminderTimer.Stop();
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
        /// 设置提醒定时器
        /// </summary>
        private void SetupReminderTimer(int intervalMinutes, string message)
        {
            // 停止现有定时器
            if (_reminderTimer != null)
            {
                _reminderTimer.Stop();
            }

            if (intervalMinutes <= 0)
            {
                return;
            }

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
        /// 显示提醒通知
        /// </summary>
        private void ShowReminderNotification(string message)
        {
            if (_notifyIcon != null)
            {
                try
                {
                    _notifyIcon.BalloonTipTitle = "定时提醒";
                    _notifyIcon.BalloonTipText = message;
                    _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    _notifyIcon.ShowBalloonTip(10000);
                }
                catch
                {
                }
            }
        }
        #endregion
    }
}