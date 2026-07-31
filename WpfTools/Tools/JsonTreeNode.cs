using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace WpfTools.Tools
{
    /// <summary>
    /// JSON树节点类
    /// 用于TreeView显示JSON数据，支持属性变更通知和长字符串折叠
    /// </summary>
    public class JsonTreeNode : INotifyPropertyChanged
    {
        /// <summary>
        /// 长字符串折叠阈值（超过该长度的字符串默认折叠显示）
        /// </summary>
        private const int LongStringThreshold = 100;

        private string _fullText;
        private bool _isExpanded = true;

        /// <summary>
        /// 节点显示的名称/键
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 节点的值（用于叶子节点）
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 节点的类型
        /// </summary>
        public JsonNodeType NodeType { get; set; }

        /// <summary>
        /// 子节点集合
        /// </summary>
        public List<JsonTreeNode> Children { get; } = new();

        /// <summary>
        /// 父节点
        /// </summary>
        public JsonTreeNode Parent { get; set; }

        /// <summary>
        /// 是否有长字符串需要折叠
        /// </summary>
        public bool HasLongString { get; private set; }

        /// <summary>
        /// 完整的长字符串值
        /// </summary>
        public string FullStringValue { get; private set; }

        /// <summary>
        /// 长字符串当前是否处于折叠显示状态
        /// </summary>
        public bool IsTextCollapsed { get; private set; }

        /// <summary>
        /// 节点的完整显示文本（变更时自动通知UI刷新）
        /// </summary>
        public string FullText
        {
            get => _fullText;
            set { _fullText = value; OnPropertyChanged(nameof(FullText)); }
        }

        /// <summary>
        /// 是否展开（变更时自动通知UI刷新）
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 切换长字符串的折叠/展开显示状态
        /// </summary>
        public void ToggleLongText()
        {
            if (!HasLongString) return;

            IsTextCollapsed = !IsTextCollapsed;
            FullText = BuildText(IsTextCollapsed
                ? $"\"{FullStringValue.Substring(0, LongStringThreshold)}...[点击展开]\""
                : $"\"{FullStringValue}\"");
        }

        /// <summary>
        /// 构建显示文本（有名称时附加"名称: "前缀）
        /// </summary>
        private string BuildText(string display) =>
            string.IsNullOrEmpty(Name) ? display : $"{Name}: {display}";

        /// <summary>
        /// 从JsonElement递归创建树节点
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <param name="name">节点名称</param>
        /// <returns>树节点</returns>
        public static JsonTreeNode CreateFromJsonElement(JsonElement element, string name = "")
        {
            var node = new JsonTreeNode { Name = name };

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    node.NodeType = JsonNodeType.Object;
                    node.FullText = node.BuildText("{ }");
                    foreach (JsonProperty prop in element.EnumerateObject())
                        node.AddChild(CreateFromJsonElement(prop.Value, prop.Name));
                    break;

                case JsonValueKind.Array:
                    node.NodeType = JsonNodeType.Array;
                    node.FullText = node.BuildText("[ ]");
                    int index = 0;
                    foreach (JsonElement item in element.EnumerateArray())
                        node.AddChild(CreateFromJsonElement(item, $"[{index++}]"));
                    break;

                case JsonValueKind.String:
                    node.NodeType = JsonNodeType.String;
                    string strValue = element.GetString() ?? string.Empty;
                    node.Value = strValue;
                    if (strValue.Length > LongStringThreshold)
                    {
                        // 长字符串默认折叠显示，点击可展开
                        node.HasLongString = true;
                        node.FullStringValue = strValue;
                        node.IsTextCollapsed = true;
                        node.FullText = node.BuildText($"\"{strValue.Substring(0, LongStringThreshold)}...[点击展开]\"");
                    }
                    else
                    {
                        node.FullText = node.BuildText($"\"{strValue}\"");
                    }
                    break;

                case JsonValueKind.Number:
                    node.NodeType = JsonNodeType.Number;
                    node.Value = element.GetRawText();
                    node.FullText = node.BuildText(node.Value);
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    node.NodeType = JsonNodeType.Boolean;
                    node.Value = element.GetRawText();
                    node.FullText = node.BuildText(node.Value);
                    break;

                case JsonValueKind.Null:
                    node.NodeType = JsonNodeType.Null;
                    node.Value = "null";
                    node.FullText = node.BuildText("null");
                    break;
            }

            return node;
        }

        /// <summary>
        /// 添加子节点并维护父子关系
        /// </summary>
        private void AddChild(JsonTreeNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }

    /// <summary>
    /// JSON节点类型枚举
    /// </summary>
    public enum JsonNodeType
    {
        Object,
        Array,
        String,
        Number,
        Boolean,
        Null
    }
}
