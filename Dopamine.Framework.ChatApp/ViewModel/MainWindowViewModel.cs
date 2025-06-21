using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Dopamine.ChatApp
{
    /// <summary>
    /// 主对话框的视图模型
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindowViewModel() : base()
        {
            CompositionTarget.Rendering += OnRenderingFrame;
        }
        #region 公开属性
        #region 工具栏
        public string ToolBarSettingButtonText
        {
            get => GetProperty("ToolBar_Setting".Translate());
            private set => SetProperty(value);
        }
        #endregion
        #region 对话框GroupBox元素
        /// <summary>
        /// 对话组框标头
        /// </summary>
        public string ChatBoxSettingGroupBoxHeader
        {
            get => GetProperty("HistoryChats".Translate());
            private set => SetProperty(value);
        }
        #region 时钟区域
        public string TimeDisplayGroupBoxHeader
        {
            get => GetProperty("TimeDisplay_Header".Translate());
            private set => SetProperty(value);
        }
        /// <summary>
        /// 系统时间
        /// </summary>
        public DateTime SystemTime
        {
            get => GetProperty(DateTime.Now);
            private set => SetProperty(value);
        }
        #endregion
        #endregion
        #region 对话框元素
        /// <summary>
        /// 对话文本框组框标头
        /// </summary>
        public string ChatBoxTextGroupBoxHeader
        {
            get => GetProperty("ChatBox_Header".Translate());
            private set => SetProperty(value);
        }
        /// <summary>
        /// AI输出对话框
        /// </summary>
        public string AIChatBoxText
        {
            get => GetProperty("Greatings".Translate());
            set => SetProperty(value);
        }
        /// <summary>
        /// 用户输入对话框
        /// </summary>
        public string UserChatBoxText
        {
            get => GetProperty("AskForGreatings".Translate());
            set => SetProperty(value);
        }
        #endregion
        #endregion
        #region 私有属性
        /// <summary>
        /// 上一帧的时间戳
        /// </summary>
        private DateTime _lastUpdateTime = DateTime.Now;
        #endregion
        #region 方法
        /// <summary>
        /// 响应程序每帧渲染事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnRenderingFrame(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            double deltaTime = (currentTime - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = currentTime;
            Update(deltaTime);
        }
        /// <summary>
        /// 每帧更新
        /// </summary>
        /// <param name="deltaTime"></param>
        private void Update(double deltaTime)
        {
            SystemTime = DateTime.Now;
        }
        #region 按键逻辑处理
        /// <summary>
        /// 处理发送按钮点击事件
        /// </summary>
        public void Send()
        {
            AIChatBoxText = AppConfig.GetValue(ConfigSection.SecondSection, "Test");
        }
        /// <summary>
        /// 处理窗口最小化
        /// </summary>
        public void OnMinimizeClicked()
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// 处理窗口
        /// </summary>
        public void OnResizeClicked()
        {
            if (App.Current.MainWindow.WindowState != WindowState.Normal)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                App.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }
        public void OnAppQuitClicked()
        {
            if (MessageBox.Show("确定要退出吗？", "退出程序", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                App.Current.Shutdown();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CompositionTarget.Rendering -= OnRenderingFrame;
            }
            base.Dispose(disposing);
        }
        #endregion
        #endregion
    }
}
