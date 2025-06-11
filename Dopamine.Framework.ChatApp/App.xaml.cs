using Dopamine.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Dopamine.ChatApp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //public static MainWindow mainWindow = new MainWindow();
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SystemSleepManager.RestoreSleep(); // 恢复系统休眠和屏幕关闭策略
            base.OnExit(e);
        }

        protected override void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            base.OnFragmentNavigation(e);
        }

        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            base.OnLoadCompleted(e);
        }

        protected override void OnNavigated(NavigationEventArgs e)
        {
            base.OnNavigated(e);
        }

        protected override void OnNavigating(NavigatingCancelEventArgs e)
        {
            base.OnNavigating(e);
        }

        protected override void OnNavigationFailed(NavigationFailedEventArgs e)
        {
            base.OnNavigationFailed(e);
        }

        protected override void OnNavigationProgress(NavigationProgressEventArgs e)
        {
            base.OnNavigationProgress(e);
        }

        protected override void OnNavigationStopped(NavigationEventArgs e)
        {
            base.OnNavigationStopped(e);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
        }
        /// <summary>
        /// 程序启动时触发事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // 创建进程锁，确保有且仅有一个进程在进行任务
            new Mutex(true, "Dopamine.ChatApp", out bool ret);
            if (!ret)
            {
                MessageBox.Show("程序已启动", "", MessageBoxButton.OK, MessageBoxImage.Stop);
                // 强制关闭，不触发 Closing 事件，不释放 WPF 资源，不等待前台线程结束
                Environment.Exit(0);
                //Application.Current.Shutdown() // 安全关闭
            }
            SystemSleepManager.PreventSleep(); // 阻止系统休眠和屏幕关闭
            SetUnhandledException();
            this.MainWindow = new MainWindow();
            MainWindow.Show();
            
            base.OnStartup(e);
        }

        private void SetUnhandledException()
        {
            this.DispatcherUnhandledException += (s, e) =>
            {
                string error_str;
                try
                {
                    e.Handled = true;
                    error_str = string.Format("{0}\t{1}", s.ToString(), e.Exception.StackTrace);
                    Recorder.RecordError(error_str);
                }
                catch (Exception ex)
                {
                    error_str = string.Format("致命错误：{0}\n额外错误：{1}", e.Exception.StackTrace, ex.StackTrace);
                    Recorder.RecordError(error_str);
                }
            };
        }
    }
}
