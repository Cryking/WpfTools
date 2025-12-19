using System;
using System.Globalization;

namespace WpfTools.Tools
{
    /// <summary>
    /// 时间戳转换工具类
    /// 提供时间戳和日期时间字符串互相转换的功能
    /// </summary>
    public static class TimestampConverter
    {
        /// <summary>
        /// 将时间戳转换为日期时间字符串
        /// </summary>
        /// <param name="timestamp">时间戳（秒或毫秒）</param>
        /// <param name="isMilliseconds">是否为毫秒时间戳</param>
        /// <returns>格式化的日期时间字符串</returns>
        public static string TimestampToDateTime(long timestamp, bool isMilliseconds = false)
        {
            try
            {
                DateTime dateTime;
                
                // 判断是秒级还是毫秒级时间戳
                if (isMilliseconds)
                {
                    // 毫秒级时间戳（13位）
                    dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                }
                else
                {
                    // 秒级时间戳（10位）
                    dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                }
                
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                return $"转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 将日期时间字符串转换为时间戳
        /// </summary>
        /// <param name="dateTimeString">日期时间字符串</param>
        /// <param name="format">日期时间格式，默认为"yyyy-MM-dd HH:mm:ss"</param>
        /// <returns>时间戳（秒级和毫秒级）</returns>
        public static string DateTimeToTimestamp(string dateTimeString, string format = "yyyy-MM-dd HH:mm:ss")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dateTimeString))
                    return "请输入有效的日期时间字符串";

                DateTime dateTime;
                
                // 尝试使用指定格式解析
                if (DateTime.TryParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    var offset = new DateTimeOffset(dateTime);
                    long seconds = offset.ToUnixTimeSeconds();
                    long milliseconds = offset.ToUnixTimeMilliseconds();
                    
                    return $"秒级时间戳: {seconds}\n毫秒级时间戳: {milliseconds}";
                }
                
                // 如果指定格式失败，尝试常见格式
                string[] commonFormats = {
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy/MM/dd HH:mm:ss",
                    "yyyy-MM-dd",
                    "yyyy/MM/dd",
                    "MM/dd/yyyy",
                    "dd-MM-yyyy HH:mm:ss",
                    "dd/MM/yyyy HH:mm:ss"
                };
                
                foreach (var fmt in commonFormats)
                {
                    if (DateTime.TryParseExact(dateTimeString, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    {
                        var offset = new DateTimeOffset(dateTime);
                        long seconds = offset.ToUnixTimeSeconds();
                        long milliseconds = offset.ToUnixTimeMilliseconds();
                        
                        return $"秒级时间戳: {seconds}\n毫秒级时间戳: {milliseconds}";
                    }
                }
                
                return "无法解析日期时间字符串，请检查格式";
            }
            catch (Exception ex)
            {
                return $"转换失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 获取当前时间戳（秒级和毫秒级）
        /// </summary>
        /// <returns>当前时间戳字符串</returns>
        public static string GetCurrentTimestamp()
        {
            try
            {
                DateTimeOffset now = DateTimeOffset.Now;
                long seconds = now.ToUnixTimeSeconds();
                long milliseconds = now.ToUnixTimeMilliseconds();
                
                return $"当前时间: {now:yyyy-MM-dd HH:mm:ss}\n秒级时间戳: {seconds}\n毫秒级时间戳: {milliseconds}";
            }
            catch (Exception ex)
            {
                return $"获取时间戳失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 自动检测时间戳是秒级还是毫秒级
        /// </summary>
        /// <param name="timestamp">时间戳字符串</param>
        /// <returns>转换结果</returns>
        public static string AutoDetectTimestamp(string timestamp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(timestamp))
                    return "请输入有效的时间戳";

                if (!long.TryParse(timestamp, out long ts))
                    return "时间戳格式不正确，请输入数字";

                // 根据位数判断是秒级还是毫秒级
                // 通常毫秒级时间戳是13位，秒级是10位
                if (timestamp.Length >= 13)
                {
                    return TimestampToDateTime(ts, true);
                }
                else
                {
                    return TimestampToDateTime(ts, false);
                }
            }
            catch (Exception ex)
            {
                return $"转换失败: {ex.Message}";
            }
        }
    }
}