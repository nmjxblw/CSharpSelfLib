using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dopamine.Framework;
namespace Dopamine.ChatApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑,在这里仅处理 ​UI 渲染、动画、焦点控制​ 等与界面强相关的操作
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 主对话窗口的视图模式
        /// </summary>
        private readonly MainWindowViewModel ViewModel = new MainWindowViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

		protected override void OnActivated(EventArgs e)
		{
            WindowState = WindowState.Normal;
            base.OnActivated(e);
        }

		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				this.DragMove();
			}
		}
	}
}
