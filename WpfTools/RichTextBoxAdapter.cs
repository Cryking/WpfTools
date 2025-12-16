using System;
using System.Linq;
using System.Windows.Documents;

namespace WpfTools
{
    /// <summary>
    /// RichTextBox适配器，使其与TextBox接口兼容
    /// </summary>
    public class RichTextBoxAdapter
    {
        private readonly System.Windows.Controls.RichTextBox _richTextBox;

        public RichTextBoxAdapter(System.Windows.Controls.RichTextBox richTextBox)
        {
            _richTextBox = richTextBox ?? throw new System.ArgumentNullException(nameof(richTextBox));
        }

        /// <summary>
        /// 获取或设置文本内容
        /// </summary>
        public string Text
        {
            get
            {
                var textRange = new TextRange(_richTextBox.Document.ContentStart, _richTextBox.Document.ContentEnd);
                return textRange.Text;
            }
            set
            {
                _richTextBox.Document.Blocks.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run(value));
                    _richTextBox.Document.Blocks.Add(paragraph);
                }
            }
        }

        /// <summary>
        /// 选中文本
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="length">选中长度</param>
        public void Select(int start, int length)
        {
            var textPointerStart = _richTextBox.Document.ContentStart.GetPositionAtOffset(start);
            var textPointerEnd = _richTextBox.Document.ContentStart.GetPositionAtOffset(start + length);
            
            if (textPointerStart != null && textPointerEnd != null)
            {
                _richTextBox.Selection.Select(textPointerStart, textPointerEnd);
            }
        }

        /// <summary>
        /// 获取焦点
        /// </summary>
        public void Focus()
        {
            _richTextBox.Focus();
        }

        /// <summary>
        /// 滚动到指定位置
        /// </summary>
        /// <param name="characterIndex">字符索引</param>
        public void ScrollToPosition(int characterIndex)
        {
            try
            {
                // 获取指定字符位置的TextPointer
                var textPointer = _richTextBox.Document.ContentStart.GetPositionAtOffset(characterIndex);
                if (textPointer != null)
                {
                    // 设置光标位置并滚动到视图
                    _richTextBox.CaretPosition = textPointer;
                    _richTextBox.ScrollToVerticalOffset(_richTextBox.VerticalOffset);
                    _richTextBox.BringIntoView();
                }
            }
            catch
            {
                // 如果失败，使用通用的BringIntoView
                _richTextBox.BringIntoView();
            }
        }

        /// <summary>
        /// 滚动到指定行（兼容性方法）
        /// </summary>
        /// <param name="lineIndex">行索引</param>
        public void ScrollToLine(int lineIndex)
        {
            try
            {
                // 根据行索引计算大概的字符位置
                int approximateCharIndex = lineIndex * 50; // 假设平均每行50个字符
                ScrollToPosition(approximateCharIndex);
            }
            catch
            {
                _richTextBox.BringIntoView();
            }
        }

        /// <summary>
        /// 根据字符索引获取行号
        /// </summary>
        /// <param name="charIndex">字符索引</param>
        /// <returns>行号</returns>
        public int GetLineIndexFromCharacterIndex(int charIndex)
        {
            // 简化的行号计算，对于复杂文档可能不够精确
            var textRange = new TextRange(_richTextBox.Document.ContentStart, _richTextBox.Document.ContentEnd);
            var text = textRange.Text.Substring(0, Math.Min(charIndex, textRange.Text.Length));
            return text.Count(c => c == '\n');
        }

        /// <summary>
        /// 清除所有高亮（空实现，不再使用高亮功能）
        /// </summary>
        public void ClearHighlight()
        {
            // 不再使用背景高亮功能
        }

        /// <summary>
        /// 高亮指定范围的文本（空实现，不再使用高亮功能）
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="length">长度</param>
        /// <param name="color">高亮颜色</param>
        public void HighlightText(int start, int length, System.Windows.Media.Color color)
        {
            // 不再使用背景高亮功能
        }

        /// <summary>
        /// 重新加载文档内容（用于更新后刷新显示）
        /// </summary>
        public void ReloadDocument()
        {
            var currentText = Text;
            Text = currentText;
        }
    }
}