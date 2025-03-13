using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO.Ports;
using ZH;
using System.Collections.Concurrent;
using System.Threading;
using System.Text.RegularExpressions;
// 耐压测试
namespace VoltageInsulationTest
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		//public MainViewModel MianViewModel = new MainViewModel();
		public MainWindow()
		{
			InitializeComponent();
			//DataContext = MianViewModel;
			InitializeData();
		}

		#region 数据、属性以及字段

		#region 硬件数据
		/// <summary>
		/// 耐压仪COM端口
		/// </summary>
		public int InstrumentCOMPort { get; set; } = 1;
		/// <summary>
		/// 耐压仪的输出电压(V)
		/// </summary>
		public float InstrumentVoltage { get; set; } = 0;
		/// <summary>
		/// 耐压仪的输出时长(s)
		/// </summary>
		public int InstrumentOutputVoltage { get; set; } = 60;
		/// <summary>
		/// 耐压仪的漏电电流上限(mA)
		/// </summary>
		public float InstrumentLeakCurrent { get; set; } = 30f;
		/// <summary>
		/// 耐压板COM端口
		/// </summary>
		public int PlateCOMPort { get; set; } = 2;
		/// <summary>
		/// 耐压板的漏电电流上限(mA)
		/// </summary>
		public float PlateLeakCurrent { get; set; } = 5f;
		/// <summary>
		/// 耐压仪实例化
		/// </summary>
		private Ainuo_WithstandVoltageInstrument Instrument { get; set; } = new Ainuo_WithstandVoltageInstrument();
		/// <summary>
		/// 耐压板实例化
		/// </summary>
		private Ainuo_WithstandVoltagePlate Plate { get; set; } = new Ainuo_WithstandVoltagePlate();
		#endregion

		/// <summary>
		/// 帧数据缓存
		/// </summary>
		private List<byte[]> FrameBuffer { get; } = new List<byte[]>();
		/// <summary>
		/// 接收区数据帧字符串缓存
		/// </summary>
		private StringBuilder ReceiveFrameStringBuilder { get; } = new StringBuilder();
		/// <summary>
		/// 发送区数据帧字符串缓存
		/// </summary>
		private StringBuilder SendFrameStringBuilder { get; } = new StringBuilder();
		/// <summary>
		/// 日志字符串缓存
		/// </summary>
		private StringBuilder LogStringBuilder { get; } = new StringBuilder();
		/// <summary>
		/// 日志快捷方法
		/// </summary>
		private string LogMessage
		{
			get
			{
				return LogStringBuilder.ToString();
			}
			set
			{
				LogStringBuilder.Insert(0, string.Format("[{0}]{1,40}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), value));
				LoggerTextBlock.Text = LogStringBuilder.ToString();
				LoggerTextBlockScrollViewer.ScrollToTop();
			}
		}

		#endregion
		#region UI逻辑
		/// <summary>
		/// 更新UI
		/// </summary>
		private void RefreshUI()
		{
			// TODO:刷新
		}
		/// <summary>
		/// 将信息打印到日志框中
		/// </summary>
		/// <param name="message"></param>
		/// <param name="isAppend"></param>
		private void ShowInLogger(string message, bool isAppend = true)
		{
			if (!isAppend) LogStringBuilder.Clear();
			LogMessage = message;
		}
		#endregion

		#region 文本框交互逻辑
		/// <summary>
		/// 耐压仪器COM出入文本框获得键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentCOMTextBoxGotFocus(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(InstrumentCOMTextBox.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			InstrumentCOMTextBox.Foreground = Brushes.Black;
			InstrumentCOMTextBox.Text = string.Empty;
		}
		/// <summary>
		/// 耐压仪器COM出入文本框失去键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentCOMTextBoxLostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(InstrumentCOMTextBox.Text))
			{
				InstrumentCOMTextBox.Foreground = Brushes.LightGray;
				InstrumentCOMTextBox.Text = "端口号";
			}
		}
		/// <summary>
		/// 耐压仪泄露电流文本框失去键盘焦点 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentLeakCurrentTextLostFocus(object sender, RoutedEventArgs e)
		{

		}
		#endregion

		#region 按钮交互逻辑

		/// <summary>
		/// 耐压仪联机按钮按下
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentConnectButtonClick(object sender, RoutedEventArgs e)
		{
			bool flag = false;
			// 先检测是否是int型
			if (int.TryParse(InstrumentCOMTextBox.Text, out var temp))
			{
				// 转化成PortName
				Instrument.InitSettingCom(temp, 1000, 100);
				// 发送关源指令，测试是否通讯成功
				flag = Instrument.Start((byte)0x02) == 1;
			}
			// UI显示
			if (flag)
			{
				InstrumentCOMPort = temp;
				LogMessage = $"耐压仪联机成功！";
				InstrumentConnetionFlag.Source = new BitmapImage(new Uri("pack://application:,,,/Images/合格.png"));
			}
			else
			{
				LogMessage = $"耐压仪联机失败！";
				InstrumentConnetionFlag.Source = new BitmapImage(new Uri("pack://application:,,,/Images/不合格.png"));
			}
		}
		/// <summary>
		/// 耐压板COM出入文本框获得键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateCOMTextBoxGotFocus(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(PlateCOMTextBox.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			PlateCOMTextBox.Foreground = Brushes.Black;
			PlateCOMTextBox.Text = string.Empty;
		}
		/// <summary>
		/// 耐压板COM出入文本框失去键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateCOMTextBoxLostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(PlateCOMTextBox.Text))
			{
				PlateCOMTextBox.Foreground = Brushes.LightGray;
				PlateCOMTextBox.Text = "端口号";
			}
		}
		/// <summary>
		/// 耐压板联机按钮按下
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateConnectButtonClick(object sender, RoutedEventArgs e)
		{
			bool flag = false;
			if (int.TryParse(PlateCOMTextBox.Text, out var temp))
			{
				Plate.InitSettingCom(temp, 1000, 100);

			}
			if (flag)
			{
				PlateCOMPort = temp;
				LogMessage = $"耐压板联机成功！";
				PlateConnetionFlag.Source = new BitmapImage(new Uri("pack://application:,,,/Images/合格.png"));
			}
			else
			{
				LogMessage = $"耐压板联机失败！";
				PlateConnetionFlag.Source = new BitmapImage(new Uri("pack://application:,,,/Images/不合格.png"));
			}
		}
		/// <summary>
		/// 清空接收区数据帧
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReceiveFrameTextboxClearButtonClick(object sender, RoutedEventArgs e)
		{
			ReceiveFrameTextbox.Text = string.Empty;
			ReceiveFrameStringBuilder.Clear();
			LogMessage = "接收区数据帧清空！";
		}
		/// <summary>
		/// 解析接受区内的数据帧
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReceiveFrameDeserializeTextboxRefreshButtonClick(object sender, RoutedEventArgs e)
		{
			ReceiveFrameDeserializeTextbox.Text = string.Empty;
			List<string> ReceivedFrames = ReceiveFrameTextbox.Text.Split('\n').ToList();
			foreach (var frame in ReceivedFrames)
			{
				ReceiveFrameStringBuilder.AppendLine();
			}
			LogMessage = "接受区数据帧解析成功！";
		}
		/// <summary>
		/// 清空发送窗口中的数据帧
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSendFrameTextboxClearButtonClick(object sender, RoutedEventArgs e)
		{
			SendFrameTextbox.Text = string.Empty;
			SendFrameStringBuilder.Clear();
			LogMessage = "发送区数据帧清空！";
		}
		/// <summary>
		/// 耐压仪设置
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentSettingButtonClick(object sender, RoutedEventArgs e)
		{
			LogMessage = "耐压仪参数设置成功！";
		}
		/// <summary>
		/// 升源按钮按下
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPowerOnButtonClick(object sender, RoutedEventArgs e)
		{
			LogMessage = "耐压仪开始实验！";
		}
		/// <summary>
		/// 设置耐压板漏电电流
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSetPlateLeakCurrentButtonClick(object sender, RoutedEventArgs e)
		{
			LogMessage = "设置耐压板最大漏电电流成功！";
		}
		/// <summary>
		/// 读取耐压板数据
		/// </summary>
		/// <remarks>从指定表位开始到指定表位结束</remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReadPlateDataButtonClick(object sender, RoutedEventArgs e)
		{
			LogMessage = "读取表位数据成功！";
		}
		/// <summary>
		/// 结束实现按钮按下
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPowerOffButtonClick(object sender, RoutedEventArgs e)
		{
			LogMessage = "耐压仪结束实验！";
		}
		#endregion

		#region 逻辑
		/// <summary>
		/// 初始化数据
		/// </summary>
		private void InitializeData()
		{
			FrameBuffer.Clear();
			FrameManager.Instance.AddFrame(BitConverter.ToString(new byte[] { 73, 110, 105, 116, 105, 97, 108, 105, 122, 101, 100 }).Replace("-", " "), true);
			RefreshUI();
		}
		#endregion
	}
}
