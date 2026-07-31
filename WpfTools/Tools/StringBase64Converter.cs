using System;
using System.Text;
using System.Collections.Generic;

namespace WpfTools.Tools
{
    /// <summary>
    /// 字符串Base64编码/解码工具类
    /// 提供多种字符集编码的Base64转换功能
    /// </summary>
    public static class StringBase64Converter
    {
        /// <summary>
        /// 获取支持的编码列表
        /// </summary>
        /// <returns>支持的编码名称列表</returns>
        public static List<string> GetSupportedEncodings()
        {
            return new List<string>
            {
                "UTF-8",
                "GBK",
                "GB2312",
                "ASCII",
                "Unicode",
                "UTF-16",
                "UTF-32"
            };
        }

        /// <summary>
        /// 将字符串编码为Base64
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="encodingName">编码名称，默认为UTF-8</param>
        /// <returns>Base64编码字符串，如果编码失败返回错误信息</returns>
        public static string EncodeToBase64(string input, string encodingName = "UTF-8")
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return "请输入要编码的字符串";

                Encoding encoding = GetEncoding(encodingName);
                if (encoding == null)
                    return $"不支持的编码格式: {encodingName}";

                byte[] bytes = encoding.GetBytes(input);
                string base64 = Convert.ToBase64String(bytes);
                
                return base64;
            }
            catch (Exception ex)
            {
                return $"编码失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 将Base64字符串解码为普通字符串
        /// </summary>
        /// <param name="base64String">Base64编码字符串</param>
        /// <param name="encodingName">编码名称，默认为UTF-8</param>
        /// <returns>解码后的字符串，如果解码失败返回错误信息</returns>
        public static string DecodeFromBase64(string base64String, string encodingName = "UTF-8")
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                    return "请输入要解码的Base64字符串";

                Encoding encoding = GetEncoding(encodingName);
                if (encoding == null)
                    return $"不支持的编码格式: {encodingName}";

                // 尝试清理Base64字符串（移除可能的空白字符）
                string cleanedBase64 = base64String.Trim();
                
                // 检查Base64字符串长度是否为4的倍数，如果不是则添加填充字符
                if (cleanedBase64.Length % 4 != 0)
                {
                    int paddingLength = 4 - (cleanedBase64.Length % 4);
                    cleanedBase64 += new string('=', paddingLength);
                }

                byte[] bytes = Convert.FromBase64String(cleanedBase64);
                string result = encoding.GetString(bytes);
                
                return result;
            }
            catch (FormatException ex)
            {
                return $"Base64格式错误: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"解码失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 获取指定名称的编码
        /// </summary>
        /// <param name="encodingName">编码名称</param>
        /// <returns>编码对象，如果不支持则返回null</returns>
        private static Encoding GetEncoding(string encodingName)
        {
            try
            {
                // GBK/GB2312 依赖 CodePagesEncodingProvider（已在App启动时注册）
                return encodingName.ToUpperInvariant() switch
                {
                    "UTF-8" or "UTF8" => Encoding.UTF8,
                    "GBK" => Encoding.GetEncoding("GBK"),
                    "GB2312" => Encoding.GetEncoding("GB2312"),
                    "ASCII" => Encoding.ASCII,
                    "UNICODE" or "UTF-16" or "UTF16" => Encoding.Unicode,
                    "UTF-32" or "UTF32" => Encoding.UTF32,
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 尝试自动检测Base64编码
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>是否可能是Base64编码</returns>
        public static bool IsBase64String(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string trimmed = input.Trim();
            // 长度必须为4的倍数，使用TryFromBase64String避免异常开销
            return trimmed.Length % 4 == 0 &&
                   Convert.TryFromBase64String(trimmed, new byte[trimmed.Length / 4 * 3], out _);
        }

        /// <summary>
        /// 尝试使用多种编码解码Base64字符串
        /// </summary>
        /// <param name="base64String">Base64编码字符串</param>
        /// <returns>解码结果，包含多种编码的解码结果</returns>
        public static string TryDecodeWithMultipleEncodings(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return "请输入要解码的Base64字符串";

            if (!IsBase64String(base64String))
                return "输入的不是有效的Base64字符串";

            var result = new StringBuilder();
            result.AppendLine("尝试使用多种编码解码结果：");
            result.AppendLine();

            // Unicode与UTF-16等价，仅保留一个避免重复输出
            var encodings = new[] { "UTF-8", "GBK", "GB2312", "ASCII", "Unicode" };
            int successCount = 0;

            foreach (var encodingName in encodings)
            {
                try
                {
                    string decoded = DecodeFromBase64(base64String, encodingName);
                    // 如果是错误信息，跳过
                    if (decoded.Contains("失败") || decoded.Contains("错误"))
                        continue;

                    result.AppendLine($"{encodingName}: {decoded}");
                    successCount++;
                }
                catch
                {
                    // 忽略解码失败的情况
                }
            }

            if (successCount == 0)
                result.AppendLine("无法使用任何常见编码解码此Base64字符串");

            return result.ToString();
        }
    }
}