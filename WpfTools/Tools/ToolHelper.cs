using System.Text;

namespace WpfTools.Tools
{
    /// <summary>
    /// 工具辅助类
    /// 提供通用的辅助方法
    /// </summary>
    public static class ToolHelper
    {
        /// <summary>
        /// 检查字符串是否为空或仅包含空白字符
        /// </summary>
        /// <param name="value">要检查的字符串</param>
        /// <returns>如果为空或仅包含空白字符返回true</returns>
        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 获取文件大小的友好显示格式
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>友好的大小格式字符串</returns>
        public static string GetFriendlyFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 限制字符串长度，超出部分用省略号表示
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>截取后的字符串</returns>
        public static string Truncate(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;
                
            return str.Substring(0, maxLength - 3) + "...";
        }
    }
}