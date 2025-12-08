using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfTools.Tools;

namespace WpfTools
{
    /// <summary>
    /// 主窗口类 - 多功能工具箱应用主界面
    /// 包含JSON格式化和图片/Base64转换功能
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage? _currentImage;

        public MainWindow()
        {
            InitializeComponent();
            InitializeEventHandlers();
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
        }

        #region JSON工具相关事件处理

        /// <summary>
        /// 清空JSON输入框
        /// </summary>
        private void BtnClearJsonInput_Click(object sender, RoutedEventArgs e)
        {
            txtJsonInput.Clear();
            txtJsonOutput.Clear();
        }

        /// <summary>
        /// 格式化JSON字符串
        /// </summary>
        private void BtnFormatJson_Click(object sender, RoutedEventArgs e)
        {
            string input = txtJsonInput.Text;
            string result = JsonFormatter.FormatJson(input);
            txtJsonOutput.Text = result;
        }

        /// <summary>
        /// 压缩JSON字符串
        /// </summary>
        private void BtnCompressJson_Click(object sender, RoutedEventArgs e)
        {
            string input = txtJsonInput.Text;
            string result = JsonFormatter.CompressJson(input);
            txtJsonOutput.Text = result;
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
                txtJsonOutput.Text = $"{formatted}";
            }
            else
            {
                txtJsonOutput.Text = result;
            }
        }

        /// <summary>
        /// 复制JSON处理结果到剪贴板
        /// </summary>
        private void BtnCopyJsonResult_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtJsonOutput.Text))
            {
                Clipboard.SetText(txtJsonOutput.Text);
                MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 处理JSON输入框的文件拖拽
        /// </summary>
        private void TxtJsonInput_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    try
                    {
                        string content = File.ReadAllText(files[0]);
                        txtJsonInput.Text = content;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"读取文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
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
            if (Clipboard.ContainsImage())
            {
                try
                {
                    var image = Clipboard.GetImage();
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
                    MessageBox.Show($"粘贴图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("剪贴板中没有图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("请先选择或粘贴图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show("Base64字符串无效或转换失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("请输入Base64字符串", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 复制Base64字符串到剪贴板
        /// </summary>
        private void BtnCopyBase64_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtBase64.Text))
            {
                Clipboard.SetText(txtBase64.Text);
                MessageBox.Show("Base64字符串已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 处理Base64文本框的文件拖拽
        /// </summary>
        private void TxtBase64_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string extension = Path.GetExtension(files[0]).ToLower();
                    if (IsImageFile(extension))
                    {
                        LoadImageFromFile(files[0]);
                    }
                    else
                    {
                        MessageBox.Show("请拖拽图片文件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}