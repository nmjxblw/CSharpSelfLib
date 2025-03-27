using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

		private DateTime _lastUpdateTime = DateTime.Now;
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
		public void Send()
		{
			AIChatBoxText = "发送";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CompositionTarget.Rendering -= OnRenderingFrame;
			}
			base.Dispose(disposing);
		}
	}
}
