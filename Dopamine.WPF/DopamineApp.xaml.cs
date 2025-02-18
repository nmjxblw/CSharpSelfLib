using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Dopamine
{
	/// <summary>
	/// DopamineApp.xaml 的交互逻辑
	/// </summary>
	public partial class DopamineApp : Application
	{
		private const string AppMutexName = "DopamineAppSingleInstanceMutex";
		private static Mutex _appMutex;

		// Win32 API 声明
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private const int SW_RESTORE = 9;

		public DopamineApp() // 修正为 public 构造函数
		{
			//this.Startup += (s, e) => { };
			//this.Exit += (s, e) => { };
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			// 创建全局互斥体
			bool createdNew;
			_appMutex = new Mutex(true, AppMutexName, out createdNew);

			if (!createdNew)
			{
				ActivateExistingWindow();
				Current.Shutdown();
				return;
			}

			base.OnStartup(e);

			// 正常启动代码
			MainWindow = new DopamineMainWindow(); // 替换为你的主窗口类型
			MainWindow.Show();
		}

		private void ActivateExistingWindow()
		{
			// 获取当前进程信息
			Process currentProcess = Process.GetCurrentProcess();

			// 查找同名的其他进程
			foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
			{
				if (process.Id != currentProcess.Id && process.MainWindowHandle != IntPtr.Zero)
				{
					// 恢复窗口状态
					ShowWindow(process.MainWindowHandle, SW_RESTORE);
					// 前置窗口
					SetForegroundWindow(process.MainWindowHandle);
					break;
				}
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			// 释放互斥体
			if (_appMutex != null)
			{
				_appMutex.ReleaseMutex();
				_appMutex.Close();
			}
			base.OnExit(e);
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return base.ToString();
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);
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
	}
}
