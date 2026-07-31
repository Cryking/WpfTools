using System.Text;
using System.Windows;
using Application = System.Windows.Application;

namespace WpfTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 注册代码页编码提供程序，使GBK/GB2312等编码在.NET 8下可用
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 全局未捕获异常兜底，避免程序直接崩溃退出
            DispatcherUnhandledException += (s, e) =>
            {
                System.Windows.MessageBox.Show($"发生未处理的异常:\n{e.Exception.Message}", "程序错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 初始化Windows Forms（系统托盘NotifyIcon依赖）
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            base.OnStartup(e);
        }
    }
}
