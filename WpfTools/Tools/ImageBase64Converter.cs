using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WpfTools.Tools
{
    /// <summary>
    /// 图片与Base64字符串转换工具类
    /// 提供图片文件与Base64编码之间的双向转换功能
    /// </summary>
    public static class ImageBase64Converter
    {
        /// <summary>
        /// 将图片转换为Base64字符串
        /// </summary>
        /// <param name="imagePath">图片文件路径</param>
        /// <returns>Base64字符串，转换失败返回错误信息</returns>
        public static string ImageToBase64(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                    return "图片文件不存在";

                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string base64String = Convert.ToBase64String(imageBytes);
                
                // 添加数据URI前缀，方便在HTML中使用
                string extension = Path.GetExtension(imagePath).ToLower();
                string mimeType = GetMimeType(extension);
                
                return $"data:{mimeType};base64,{base64String}";
            }
            catch (Exception ex)
            {
                return $"转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 将Base64字符串保存为图片文件
        /// </summary>
        /// <param name="base64String">Base64字符串</param>
        /// <param name="savePath">保存路径</param>
        /// <returns>操作结果消息</returns>
        public static string Base64ToImageFile(string base64String, string savePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64String))
                    return "Base64字符串不能为空";

                // 移除数据URI前缀（如果存在）
                string cleanBase64 = RemoveDataUriPrefix(base64String);
                
                byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                File.WriteAllBytes(savePath, imageBytes);
                
                return $"图片已保存到: {savePath}";
            }
            catch (FormatException)
            {
                return "Base64字符串格式无效";
            }
            catch (Exception ex)
            {
                return $"转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 将Base64字符串转换为BitmapImage对象
        /// </summary>
        /// <param name="base64String">Base64字符串</param>
        /// <returns>BitmapImage对象，转换失败返回null</returns>
        public static BitmapImage? Base64ToBitmapImage(string base64String)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64String))
                    return null;

                // 移除数据URI前缀
                string cleanBase64 = RemoveDataUriPrefix(base64String);
                
                byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                
                using (var stream = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将BitmapImage转换为Base64字符串
        /// </summary>
        /// <param name="bitmap">BitmapImage对象</param>
        /// <param name="format">图片格式（默认为png）</param>
        /// <returns>Base64字符串</returns>
        public static string BitmapImageToBase64(BitmapImage bitmap, string format = "png")
        {
            try
            {
                if (bitmap == null)
                    return "图片对象为空";

                byte[] imageBytes;
                string encoderType = GetEncoderType(format);

                using (var stream = new MemoryStream())
                {
                    var encoder = GetBitmapEncoder(encoderType);
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                    imageBytes = stream.ToArray();
                }

                string base64String = Convert.ToBase64String(imageBytes);
                string mimeType = GetMimeType($".{format}");
                
                return $"data:{mimeType};base64,{base64String}";
            }
            catch (Exception ex)
            {
                return $"转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 显示文件选择对话框
        /// </summary>
        /// <param name="filter">文件过滤器</param>
        /// <returns>选择的文件路径，未选择返回null</returns>
        public static string? ShowOpenFileDialog(string filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.ico")
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Title = "选择图片文件"
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        /// <summary>
        /// 显示文件保存对话框
        /// </summary>
        /// <param name="defaultFileName">默认文件名</param>
        /// <param name="filter">文件过滤器</param>
        /// <returns>保存的文件路径，未选择返回null</returns>
        public static string? ShowSaveFileDialog(string defaultFileName = "image", string filter = "PNG图片|*.png|JPEG图片|*.jpg|BMP图片|*.bmp")
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = "保存图片文件",
                FileName = defaultFileName
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }

        /// <summary>
        /// 根据文件扩展名获取MIME类型
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <returns>MIME类型字符串</returns>
        private static string GetMimeType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".ico" => "image/x-icon",
                ".webp" => "image/webp",
                _ => "image/png"
            };
        }

        /// <summary>
        /// 移除Base64字符串中的数据URI前缀
        /// </summary>
        /// <param name="base64String">可能包含前缀的Base64字符串</param>
        /// <returns>纯净的Base64字符串</returns>
        private static string RemoveDataUriPrefix(string base64String)
        {
            if (base64String.Contains(","))
            {
                return base64String.Substring(base64String.IndexOf(',') + 1);
            }
            return base64String;
        }

        /// <summary>
        /// 根据格式获取编码器类型
        /// </summary>
        /// <param name="format">图片格式</param>
        /// <returns>编码器类型字符串</returns>
        private static string GetEncoderType(string format)
        {
            return format.ToLower() switch
            {
                "jpg" or "jpeg" => "jpeg",
                "png" => "png",
                "bmp" => "bmp",
                "gif" => "gif",
                _ => "png"
            };
        }

        /// <summary>
        /// 获取对应的BitmapEncoder
        /// </summary>
        /// <param name="encoderType">编码器类型</param>
        /// <returns>BitmapEncoder实例</returns>
        private static System.Windows.Media.Imaging.BitmapEncoder GetBitmapEncoder(string encoderType)
        {
            return encoderType.ToLower() switch
            {
                "jpeg" => new JpegBitmapEncoder(),
                "png" => new PngBitmapEncoder(),
                "bmp" => new BmpBitmapEncoder(),
                "gif" => new GifBitmapEncoder(),
                _ => new PngBitmapEncoder()
            };
        }
    }
}