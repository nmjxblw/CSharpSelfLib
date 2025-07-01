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
using System.Windows.Threading;

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
            #region 开启固定帧率刷新线程
            Task FixedRateTask = Task.Run(() =>
            {
                if (int.TryParse(AppConfig.GetValue("FrameRate"), out var tempFrame))
                {
                    FrameRate = tempFrame;
                }
                OnFixedUpdate += FixedUpdate; // 订阅固定更新事件
                int sleepTime = (int)Math.Round(1000f / FrameRate); // 计算每帧的睡眠时间
                while (true)
                {
                    Thread.Sleep(sleepTime);
                    this.OnFixedUpdate?.Invoke(sleepTime / 1000d);
                }
            });
            #endregion
        }
        #region 公开属性
        #region - 事件 - 
        public event Action<double> OnFixedUpdate;
        #endregion
        #region - 工具栏 - 
        public string ToolBarSettingButtonText
        {
            get => GetProperty("ToolBar_Setting".Translate());
            private set => SetProperty(value);
        }
        #endregion
        #region - 对话框GroupBox元素 - 
        /// <summary>
        /// 对话组框标头
        /// </summary>
        public string ChatBoxSettingGroupBoxHeader
        {
            get => GetProperty("HistoryChats".Translate());
            private set => SetProperty(value);
        }
        #region  -- 时钟区域 --  
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
            /*private*/
            set => SetProperty(value);
        }
        #endregion
        #region -- 数据信息区域 --
        /// <summary>
        /// 帧率，默认为60帧每秒
        /// </summary>
        public int FrameRate
        {
            get => GetProperty(60);
            set => SetProperty(value);
        }
        #endregion
        #endregion
        #region - 对话框元素 -  
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
        /// <param name="_">每帧之间间隔的时间
        /// <para>单位s</para>
        /// </param>
        private void Update(double _)
        {
            SystemTime = DateTime.Now;
        }
        /// <summary>
        /// 按照刷新率更新
        /// </summary>
        /// <param name="_">帧间隔
        /// <para>单位s</para>
        /// </param>
        private void FixedUpdate(double _)
        {

        }
        #region 按键逻辑处理
        /// <summary>
        /// 处理发送按钮点击事件
        /// </summary>
        public void Send()
        {
            AIChatBoxText = AppConfig.GetValue(ConfigSection.SecondSection, "Test");
        }
        public void Settings()
        {
            throw new NotImplementedException();
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
            if (MessageBox.Show("Quit_App_Confirm".Translate(), "Quit_App".Translate(), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
