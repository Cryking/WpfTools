using System.Text.Json;

namespace WpfTools.Tools
{
    /// <summary>
    /// JSON格式化工具类
    /// 提供JSON字符串的格式化、压缩和验证功能
    /// </summary>
    public static class JsonFormatter
    {
        // 缓存序列化选项，避免每次调用重复创建（官方推荐做法）
        private static readonly JsonSerializerOptions IndentedOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly JsonSerializerOptions CompactOptions = new()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// 尝试解析JSON字符串，返回是否有效及错误信息
        /// </summary>
        /// <param name="jsonString">要解析的JSON字符串</param>
        /// <param name="error">解析失败时的错误信息，成功时为null</param>
        /// <returns>是否为有效JSON</returns>
        public static bool TryParse(string jsonString, out string error)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                error = "请输入有效的JSON字符串";
                return false;
            }

            try
            {
                using var _ = JsonDocument.Parse(jsonString);
                error = null;
                return true;
            }
            catch (JsonException ex)
            {
                error = $"JSON格式错误: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 美化JSON字符串格式
        /// </summary>
        /// <param name="jsonString">原始JSON字符串</param>
        /// <returns>格式化后的JSON字符串，如果格式化失败返回错误信息</returns>
        public static string FormatJson(string jsonString) => Serialize(jsonString, IndentedOptions);

        /// <summary>
        /// 压缩JSON字符串（移除空白字符）
        /// </summary>
        /// <param name="jsonString">原始JSON字符串</param>
        /// <returns>压缩后的JSON字符串，如果压缩失败返回错误信息</returns>
        public static string CompressJson(string jsonString) => Serialize(jsonString, CompactOptions);

        /// <summary>
        /// 验证JSON字符串是否有效
        /// </summary>
        /// <param name="jsonString">要验证的JSON字符串</param>
        /// <returns>验证结果字符串</returns>
        public static string ValidateJson(string jsonString) =>
            TryParse(jsonString, out string error) ? "✓ JSON格式有效" : $"✗ {error}";

        /// <summary>
        /// 使用指定选项重新序列化JSON字符串
        /// </summary>
        private static string Serialize(string jsonString, JsonSerializerOptions options)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonString))
                    return "请输入有效的JSON字符串";

                using var jsonDoc = JsonDocument.Parse(jsonString);
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
        /// 尝试修复常见的JSON格式问题（BOM、首尾空白、未加引号的键名）
        /// </summary>
        /// <param name="jsonString">可能存在问题的JSON字符串</param>
        /// <returns>尝试修复后的JSON字符串</returns>
        public static string TryFixJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return jsonString;

            // 移除BOM标记和首尾空白
            jsonString = jsonString.TrimStart('\uFEFF').Trim();

            // 仅为紧跟在 { 或 , 后的未加引号键名补充引号，避免误伤字符串值中的冒号（如URL）
            return System.Text.RegularExpressions.Regex.Replace(jsonString, @"([{,]\s*)(\w+)\s*:", @"$1""$2"":");
        }
    }
}
