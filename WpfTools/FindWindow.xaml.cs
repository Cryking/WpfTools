using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfTools
{
    /// <summary>
    /// FindWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FindWindow : Window
    {
        private readonly RichTextBoxAdapter _targetTextBox;
        private readonly List<int> _matchPositions = new List<int>();
        private int _currentMatchIndex = -1;

        public FindWindow(RichTextBoxAdapter targetTextBox)
        {
            InitializeComponent();
            _targetTextBox = targetTextBox ?? throw new ArgumentNullException(nameof(targetTextBox));
            
            // 设置焦点到查找文本框
            Loaded += (s, e) => txtSearchText.Focus();
            
            // 绑定回车键事件和文本变化事件
            txtSearchText.KeyDown += TxtSearchText_KeyDown;
            txtSearchText.TextChanged += TxtSearchText_TextChanged;
            chkCaseSensitive.Click += ChkCaseSensitive_Click;
            

        }

        /// <summary>
        /// 处理回车键按下事件
        /// </summary>
        private void TxtSearchText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnFindNext_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
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
            string searchText = txtSearchText.Text;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _matchPositions.Clear();
                _currentMatchIndex = -1;
                ShowStatus("0 个匹配项");
                return;
            }

            FindAllMatches(searchText);
            
            if (_matchPositions.Count > 0)
            {
                ShowStatus($"{_matchPositions.Count} 个匹配项");
                // 自动选中第一个匹配项
                if (_currentMatchIndex == -1)
                {
                    _currentMatchIndex = 0;
                    NavigateToMatch(0);
                }
            }
            else
            {
                ShowStatus("0 个匹配项");
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
        /// 查找所有匹配项
        /// </summary>
        /// <param name="searchText">搜索文本</param>
        private void FindAllMatches(string searchText)
        {
            _matchPositions.Clear();
            _currentMatchIndex = -1;

            string text = _targetTextBox.Text;
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

            string searchText = txtSearchText.Text;
            int position = _matchPositions[matchIndex];

            // 选中文本
            _targetTextBox.Select(position, searchText.Length);
            _targetTextBox.Focus();
            
            // 直接滚动到匹配的字符位置
            _targetTextBox.ScrollToPosition(position);
            
            ShowStatus($"{_matchPositions.Count} 个匹配项 - 第 {matchIndex + 1}/{_matchPositions.Count} 个");
        }



        /// <summary>
        /// 显示状态信息
        /// </summary>
        /// <param name="message">状态消息</param>
        /// <param name="autoClear">是否自动清除（默认为false，保持匹配个数的显示）</param>
        private void ShowStatus(string message, bool autoClear = false)
        {
            txtStatus.Text = message;
            
            // 只有在明确要求时才自动清除
            if (autoClear)
            {
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += (s, e) =>
                {
                    txtStatus.Text = "";
                    timer.Stop();
                };
                timer.Start();
            }
        }


    }
}