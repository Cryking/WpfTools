using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfTools
{
    /// <summary>
    /// FindDialog.xaml 的交互逻辑
    /// </summary>
    public partial class FindDialog : Window
    {
        public System.Windows.Controls.RichTextBox? TargetRichTextBox { get; set; }
        private readonly List<int> _matchPositions = new List<int>();
        private int _currentMatchIndex = -1;

        public FindDialog()
        {
            InitializeComponent();
            
            // 设置焦点到查找文本框
            Loaded += (s, e) => txtSearchText.Focus();
            
            // 绑定事件
            txtSearchText.TextChanged += TxtSearchText_TextChanged;
            txtSearchText.KeyDown += TxtSearchText_KeyDown;
            chkCaseSensitive.Click += ChkCaseSensitive_Click;
        }

        /// <summary>
        /// 处理回车键按下事件
        /// </summary>
        private void TxtSearchText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnFindNext_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 处理搜索文本变化事件
        /// </summary>
        private void TxtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSearchResults();
        }

        /// <summary>
        /// 处理区分大小写复选框点击事件
        /// </summary>
        private void ChkCaseSensitive_Click(object sender, RoutedEventArgs e)
        {
            UpdateSearchResults();
        }

        /// <summary>
        /// 更新搜索结果
        /// </summary>
        private void UpdateSearchResults()
        {
            if (TargetRichTextBox == null)
            {
                txtStatus.Text = "目标文本框未设置";
                return;
            }

            string searchText = txtSearchText.Text;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _matchPositions.Clear();
                _currentMatchIndex = -1;
                txtStatus.Text = "0 个匹配项";
                return;
            }

            FindAllMatches(searchText);
            
            if (_matchPositions.Count > 0)
            {
                txtStatus.Text = $"{_matchPositions.Count} 个匹配项";
                // 自动选中第一个匹配项
                if (_currentMatchIndex == -1)
                {
                    _currentMatchIndex = 0;
                    NavigateToMatch(0);
                }
            }
            else
            {
                txtStatus.Text = "0 个匹配项";
            }
        }

        /// <summary>
        /// 查找所有匹配项
        /// </summary>
        /// <param name="searchText">搜索文本</param>
        private void FindAllMatches(string searchText)
        {
            _matchPositions.Clear();
            _currentMatchIndex = -1;

            // 获取RichTextBox的文本内容
            var textRange = new TextRange(TargetRichTextBox.Document.ContentStart, TargetRichTextBox.Document.ContentEnd);
            string text = textRange.Text;
            
            if (string.IsNullOrEmpty(text))
                return;

            StringComparison comparison = chkCaseSensitive.IsChecked == true 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;

            int index = 0;
            while (index < text.Length)
            {
                int foundIndex = text.IndexOf(searchText, index, comparison);
                if (foundIndex == -1)
                    break;
                
                _matchPositions.Add(foundIndex);
                index = foundIndex + 1;
            }
        }

        /// <summary>
        /// 查找下一个
        /// </summary>
        private void BtnFindNext_Click(object sender, RoutedEventArgs e)
        {
            NavigateToNextMatch();
        }

        /// <summary>
        /// 查找上一个
        /// </summary>
        private void BtnFindPrevious_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPreviousMatch();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 导航到下一个匹配项
        /// </summary>
        private void NavigateToNextMatch()
        {
            if (_matchPositions.Count == 0)
                return;

            _currentMatchIndex = (_currentMatchIndex + 1) % _matchPositions.Count;
            NavigateToMatch(_currentMatchIndex);
        }

        /// <summary>
        /// 导航到上一个匹配项
        /// </summary>
        private void NavigateToPreviousMatch()
        {
            if (_matchPositions.Count == 0)
                return;

            _currentMatchIndex = _currentMatchIndex <= 0 
                ? _matchPositions.Count - 1 
                : _currentMatchIndex - 1;
            NavigateToMatch(_currentMatchIndex);
        }

        /// <summary>
        /// 导航到指定匹配项
        /// </summary>
        /// <param name="matchIndex">匹配项索引</param>
        private void NavigateToMatch(int matchIndex)
        {
            if (matchIndex < 0 || matchIndex >= _matchPositions.Count)
                return;

            if (TargetRichTextBox == null)
                return;

            string searchText = txtSearchText.Text;
            int position = _matchPositions[matchIndex];

            try
            {
                // 获取文本起始位置
                var textRange = new TextRange(TargetRichTextBox.Document.ContentStart, TargetRichTextBox.Document.ContentEnd);
                var text = textRange.Text;

                // 计算实际的TextPointer位置
                var startPointer = TargetRichTextBox.Document.ContentStart;
                var currentPointer = startPointer;

                // 逐个字符移动到目标位置
                for (int i = 0; i < position && currentPointer != null; i++)
                {
                    currentPointer = currentPointer.GetNextContextPosition(LogicalDirection.Forward);
                }

                if (currentPointer != null)
                {
                    // 设置选中的结束位置
                    var endPointer = currentPointer;
                    for (int i = 0; i < searchText.Length && endPointer != null; i++)
                    {
                        endPointer = endPointer.GetNextContextPosition(LogicalDirection.Forward);
                    }

                    if (endPointer != null)
                    {
                        // 选中文本
                        TargetRichTextBox.Selection.Select(currentPointer, endPointer);
                        TargetRichTextBox.Focus();
                        
                        // 滚动到选中位置
                        TargetRichTextBox.CaretPosition = currentPointer;
                        TargetRichTextBox.BringIntoView();
                        
                        // 更新状态
                        txtStatus.Text = $"{_matchPositions.Count} 个匹配项 - 第 {matchIndex + 1}/{_matchPositions.Count} 个";
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"错误: {ex.Message}";
            }
        }
    }
}