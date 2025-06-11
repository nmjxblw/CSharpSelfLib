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
        /// <summary>
        /// AI输出对话框
        /// </summary>
        public string AIChatBoxText
        {
            get => GetProperty("Hei,there!");
            set => SetProperty(value);
        }
        /// <summary>
        /// 用户输入对话框
        /// </summary>
        public string UserChatBoxText
        {
            get => GetProperty("Say Hi!");
            set => SetProperty(value);
        }
        /// <summary>
        /// 工具栏设置按钮
        /// </summary>
        public string ToolBarSettingButtonText
        {
            get => GetProperty("ToolBar_Setting".Translate());
            set => SetProperty(value);
        }
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

        }
        /// <summary>
        /// 处理发送按钮点击事件
        /// </summary>
        public void Send()
        {
            AIChatBoxText = ConfigManager.Data.Test;
        }
        public void OnMinimizeClicked()
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }
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
    }
}
