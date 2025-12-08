using System.Text;
using System.Text.Json;

namespace WpfTools.Tools
{
    /// <summary>
    /// JSON格式化工具类
    /// 提供JSON字符串的格式化、压缩和验证功能
    /// </summary>
    public static class JsonFormatter
    {
        /// <summary>
        /// 美化JSON字符串格式
        /// </summary>
        /// <param name="jsonString">原始JSON字符串</param>
        /// <returns>格式化后的JSON字符串，如果格式化失败返回错误信息</returns>
        public static string FormatJson(string jsonString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonString))
                    return "请输入有效的JSON字符串";

                // 使用System.Text.Json进行解析和格式化
                var jsonDoc = JsonDocument.Parse(jsonString);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                return JsonSerializer.Serialize(jsonDoc, options);
            }
            catch (JsonException ex)
            {
                return $"JSON格式错误: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"处理失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 压缩JSON字符串（移除空白字符）
        /// </summary>
        /// <param name="jsonString">原始JSON字符串</param>
        /// <returns>压缩后的JSON字符串，如果压缩失败返回错误信息</returns>
        public static string CompressJson(string jsonString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonString))
                    return "请输入有效的JSON字符串";

                var jsonDoc = JsonDocument.Parse(jsonString);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                return JsonSerializer.Serialize(jsonDoc, options);
            }
            catch (JsonException ex)
            {
                return $"JSON格式错误: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"处理失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 验证JSON字符串是否有效
        /// </summary>
        /// <param name="jsonString">要验证的JSON字符串</param>
        /// <returns>验证结果字符串</returns>
        public static string ValidateJson(string jsonString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonString))
                    return "请输入有效的JSON字符串";

                var jsonDoc = JsonDocument.Parse(jsonString);
                return "✓ JSON格式有效";
            }
            catch (JsonException ex)
            {
                return $"✗ JSON格式错误: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"✗ 验证失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 尝试修复常见的JSON格式问题
        /// </summary>
        /// <param name="jsonString">可能存在问题的JSON字符串</param>
        /// <returns>尝试修复后的JSON字符串</returns>
        public static string TryFixJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return jsonString;

            // 移除BOM标记
            if (jsonString.StartsWith("\uFEFF"))
                jsonString = jsonString.Substring(1);

            // 移除首尾空白
            jsonString = jsonString.Trim();

            // 尝试修复常见的引号问题
            jsonString = System.Text.RegularExpressions.Regex.Replace(jsonString, @"(\w+):", @"""$1"":");

            return jsonString;
        }
    }
}