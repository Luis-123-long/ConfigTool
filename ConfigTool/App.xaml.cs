using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ConfigTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs eventArgs)
        {
            // 1. 先开登录窗口
            LoginWindow login = new LoginWindow();
            bool? result = login.ShowDialog();

            // 2. 判断结果
            if (result == true)
            {
                // 登录成功，准备启动主窗口
                MainWindow main = new MainWindow();

                // B. 【重要】因为是手动模式，必须告诉程序：
                //    "当主窗口关闭时，彻底结束整个程序"
                main.Closed += (s, args) => this.Shutdown();

                main.Show();
            }
            else
            {
                // 登录失败或取消，手动退出
                this.Shutdown();
            }
        }
        // 当程序发生任何“未捕获”的错误时，会跑进这里
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // 1. 赶紧记日志！这是崩溃前的遗言
            LogHelper.Error("发生未处理的全局异常 (Global Crash)", e.Exception);

            // 2. 告诉用户发生了什么，而不是直接消失
            MessageBox.Show($"程序遇到严重错误，即将关闭。\n错误信息: {e.Exception.Message}\n\n请将 Logs 文件夹下的日志发送给技术人员。", "系统崩溃");

            // 3. 标记为“已处理”，防止 Windows 弹出那个“程序已停止工作”的丑框
            // e.Handled = true; // 如果你想让程序尝试继续运行，设为 true（不推荐，数据可能已乱）
        }
    }
}

