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
using ZH.SocketModule;
using System.Collections.Concurrent;
using System.Threading;
using System.Text.RegularExpressions;
using System.ComponentModel;
// 耐压测试
namespace VoltageInsulationTest
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			InitializeData();
		}

		#region 数据、属性以及字段

		#region 硬件数据
		/// <summary>
		/// 用于解析文本框中数据帧的正则表达式
		/// </summary>
		public Regex FrameRegex { get; } = new Regex(
	@"^(\[(?<date>[^\]]+)\]\s*)?(\[(?<port>[^\]]+)\]\s*)?((?<direction>[<>]{3})?\s*)?(?<frame>(?:[0-9A-Fa-frameInfo]{2}\s?)+)(?:\s*\[(?<remark>[^\]]*)\])?$",
	RegexOptions.IgnoreCase
);
		/// <summary>
		/// 耐压仪COM端口
		/// </summary>
		public int InstrumentCOMPort { get; set; } = 1;
		/// <summary>
		/// 耐压仪的输出电压(V)
		/// </summary>
		public float InstrumentVoltage { get; set; } = 0f;
		/// <summary>
		/// 耐压仪的输出时长(s)
		/// </summary>
		public float InstrumentOutputTime { get; set; } = 60f;
		/// <summary>
		/// 耐压仪的漏电电流上限(mA)
		/// </summary>
		public float InstrumentLeakCurrent { get; set; } = 30f;
		/// <summary>
		/// 耐压仪组合索引
		/// </summary>
		public int TestCombinationIndex { get; set; } = 1;
		/// <summary>
		/// 测试组合代码数据集
		/// </summary>
		public List<byte> TestCombinationCodeCollection { get; } = new List<byte>() { 0x30, 0x31, 0x32 };
		/// <summary>
		/// 耐压板COM端口
		/// </summary>
		public int PlateCOMPort { get; set; } = 2;
		/// <summary>
		/// 耐压板的漏电电流上限(mA)
		/// </summary>
		public float PlateLeakCurrent { get; set; } = 5f;
		/// <summary>
		/// 耐压板读取开始表位号
		/// </summary>
		public int PlateStartSocket { get; set; } = 1;
		/// <summary>
		/// 耐压板读取结束表位号
		/// </summary>
		public int PlateEndSocket { get; set; } = 1;
		/// <summary>
		/// 耐压仪实例化
		/// </summary>
		private WithstandVoltageInstrument Instrument { get; set; } = new WithstandVoltageInstrument();
		/// <summary>
		/// 耐压板实例化
		/// </summary>
		private WithstandVoltagePlate Plate { get; set; } = new WithstandVoltagePlate();
		#endregion

		/// <summary>
		/// 接收区数据帧字符串缓存
		/// </summary>
		private StringBuilder ReceiveFrameStringBuilder { get; } = new StringBuilder();
		/// <summary>
		/// 发送区数据帧字符串缓存
		/// </summary>
		private StringBuilder SendFrameStringBuilder { get; } = new StringBuilder();
		/// <summary>
		/// 存储所有的数据帧
		/// </summary>
		private StringBuilder AllFrameStringBuilder { get; } = new StringBuilder();
		/// <summary>
		/// 数据帧缓存队列
		/// </summary>
		private ConcurrentQueue<FrameInfo> FrameQueue { get; } = new ConcurrentQueue<FrameInfo>();
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
				LogStringBuilder.Insert(0, string.Format("[{0}]{1,40}\n", DateTime.Now.ToString("T"), value));
				LoggerTextBlock.Text = LogStringBuilder.ToString();
				LoggerTextBlockScrollViewer.ScrollToTop();
			}
		}

		#endregion
		#region UI逻辑
		/// <summary>
		/// 更新文本框UI
		/// </summary>
		private void RefreshTextBoxes()
		{
			// TODO:刷新
			InstrumentCOMTextBox.Text = InstrumentCOMPort.ToString();
			PlateCOMTextBox.Text = PlateCOMPort.ToString();
			InstrumentVoltageText.Text = InstrumentVoltage.ToString();
			InstrumentOutputTimeText.Text = InstrumentOutputTime.ToString();
			InstrumentLeakCurrentText.Text = InstrumentLeakCurrent.ToString();
			PlateLeakCurrentText.Text = PlateLeakCurrent.ToString();
			PlateStartSocketText.Text = PlateStartSocket.ToString();
			PlateEndSocketText.Text = PlateEndSocket.ToString();

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
		/// 耐压仪电压文本框获得键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentVoltageTextGotFocus(object sender, RoutedEventArgs e)
		{
			// 检测是否为合法输入
			if (float.TryParse(InstrumentVoltageText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			// 不是的话就清空
			InstrumentVoltageText.Foreground = Brushes.Black;
			InstrumentVoltageText.Text = string.Empty;
		}
		/// <summary>
		/// 耐压仪文本框失去键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentVoltageTextLostFocus(object sender, RoutedEventArgs e)
		{
			// 检测是否为合法输入
			if (float.TryParse(InstrumentVoltageText.Text.Replace(" ", "").Trim(), out float _))
			{
				return;
			}
			// 不是的话就使用上次输入的电压
			InstrumentVoltageText.Text = InstrumentVoltage.ToString("F2");
		}
		/// <summary>
		/// 耐压仪输出时间文本框获得键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentOutputTimeTextGotFocus(object sender, RoutedEventArgs e)
		{
			// 检测是否为合法数值
			if (int.TryParse(InstrumentOutputTimeText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			// 不合法的话就清空
			InstrumentOutputTimeText.Foreground = Brushes.Black;
			InstrumentOutputTimeText.Text = string.Empty;
		}
		/// <summary>
		/// 耐压仪输出时间文本框失去键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentOutputTimeTextLostFocus(object sender, RoutedEventArgs e)
		{
			// 检测是否为合法输入
			if (int.TryParse(InstrumentOutputTimeText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			// 不是的话就使用上次输入的数值
			InstrumentOutputTimeText.Text = InstrumentOutputTime.ToString();
		}
		/// <summary>
		/// 耐压仪泄露电流文本框获得键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentLeakCurrentTextGotFocus(object sender, RoutedEventArgs e)
		{
			if (float.TryParse(InstrumentLeakCurrentText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			InstrumentLeakCurrentText.Foreground = Brushes.Black;
			InstrumentLeakCurrentText.Text = string.Empty;
		}
		/// <summary>
		/// 耐压仪泄露电流文本框失去键盘焦点 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstrumentLeakCurrentTextLostFocus(object sender, RoutedEventArgs e)
		{
			if (float.TryParse(InstrumentLeakCurrentText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			InstrumentLeakCurrentText.Text = InstrumentLeakCurrent.ToString("F2");
		}
		/// <summary>
		/// 耐压板泄露电流文本框获得键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateLeakCurrentTextGotFocus(object sender, RoutedEventArgs e)
		{
			if (float.TryParse(PlateLeakCurrentText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			PlateLeakCurrentText.Foreground = Brushes.Black;
			PlateLeakCurrentText.Text = string.Empty;
		}
		/// <summary>
		/// 耐压板泄露电流文本框失去键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateLeakCurrentTextLostFocus(object sender, RoutedEventArgs e)
		{
			if (float.TryParse(PlateLeakCurrentText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			PlateLeakCurrentText.Text = PlateLeakCurrent.ToString("F2");
		}
		/// <summary>
		/// 耐压板读取开始表位号文本框获得键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateStartSocketTextGotFocus(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(PlateStartSocketText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			PlateStartSocketText.Foreground = Brushes.Black;
			PlateStartSocketText.Text = string.Empty;
		}
		/// <summary>
		/// 耐压板读取开始表位号文本框失去键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateStartSocketTextLostFocus(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(PlateStartSocketText.Text.Replace(" ", "").Trim(), out int temp))
			{
				PlateStartSocket = Math.Max(1, temp);
				PlateStartSocketText.Text = PlateStartSocket.ToString();
				return;
			}
			PlateStartSocket = 1;
			PlateStartSocketText.Text = PlateStartSocket.ToString();
		}
		/// <summary>
		/// 耐压板读取结束表位号文本框获得键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateEndSocketTextGotFocus(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(PlateEndSocketText.Text.Replace(" ", "").Trim(), out int temp))
			{
				PlateEndSocket = Math.Max(PlateStartSocket, temp);
				PlateEndSocketText.Text = PlateEndSocket.ToString();
				return;
			}
			PlateEndSocket = PlateStartSocket;
			PlateEndSocketText.Text = PlateEndSocket.ToString();
		}
		/// <summary>
		/// 耐压板读取表位号文本框失去键盘焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlateEndSocketTextLostFocus(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(PlateEndSocketText.Text.Replace(" ", "").Trim(), out _))
			{
				return;
			}
			PlateEndSocketText.Text = "8";
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
				flag = Instrument.PowerOn(false);
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
				flag = Plate.InitSettingCom(temp, 1000, 100) == 0;
				flag = Plate.SetStrat((byte)0x30) && flag;
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
		/// 解析接受区和发送区内的数据帧，手动刷新解析，优先级低
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void OnFrameDeserializeTextboxRefreshButtonClick(object sender, RoutedEventArgs e)
		{
			try
			{
				// 先清空解析区数据
				FrameDeserializeTextbox.Text = string.Empty;
				List<FrameInfo> receiveFrames = new List<FrameInfo>();
				List<FrameInfo> sendFrames = new List<FrameInfo>();
				string ReceiveFrameTextboxContent = ReceiveFrameTextbox.Text;
				string SendFrameTextboxContent = SendFrameTextbox.Text;
				List<Task> tasks = new List<Task>
				{
					Task.Run(async () =>
					{
						// 收录文本框的数据帧
						foreach (string rec in ReceiveFrameTextboxContent.Split('\n'))
						{
							if(string.IsNullOrWhiteSpace(rec))continue;
							Match match = FrameRegex.Match(rec);
							if (match.Success)
							{
								FrameInfo frameInfo = new FrameInfo()
								{
									Date = Convert.ToDateTime(match.Groups["date"].Value??DateTime.Now.ToString()),
									Port = int.Parse(Regex.Replace(match.Groups["port"].Value, @"\D", "")),// 移除所有非数字字符，提取整数
									IsSend = false,
									Frame = match.Groups["frame"].Value.Trim().Replace(" ", "").FromHexString(),
									Remark = match.Groups["remark"].Value,// 数据帧备注
								};
								receiveFrames.Add(frameInfo);
							}
						}
						//receiveFrames.Sort((x,y)=>x.Date.CompareTo(y.Date));
						await Task.Yield();
					}),
					Task.Run(async () =>
					{
						
						// 收录文本框中的数据帧
						foreach (string send in SendFrameTextboxContent.Split('\n'))
						{
							if(string.IsNullOrWhiteSpace(send))continue;
							Match match = FrameRegex.Match(send);
							if (match.Success)
							{
								FrameInfo frameInfo = new FrameInfo()
								{
									Date = Convert.ToDateTime(match.Groups["date"].Value??DateTime.Now.ToString()),
									Port = int.Parse(Regex.Replace(match.Groups["port"].Value, @"\D", "")),// 移除所有非数字字符，提取整数
									IsSend = true,
									Frame = match.Groups["frame"].Value.Trim().Replace(" ", "").FromHexString(),
									Remark = match.Groups["remark"].Value,// 数据帧备注
								};
								sendFrames.Add(frameInfo);
							}
						}
						//sendFrames.Sort((x,y)=>x.Date.CompareTo(y.Date));
						await Task.Yield();
					})
				};
				await Task.WhenAll(tasks);
				// 合并数据帧（临时）
				List<FrameInfo> fullFrames = receiveFrames;
				fullFrames.AddRange(sendFrames);
				fullFrames.Sort((x, y) => x.Date.CompareTo(y.Date));
				// 清空缓存
				AllFrameStringBuilder.Clear();
				// 解析
				foreach (FrameInfo frameInfo in fullFrames)
				{
					AllFrameStringBuilder.Insert(0, DeserializeFrame(frameInfo) + "\n");
				}
				await Dispatcher.InvokeAsync(() =>
				{
					// 输出
					FrameDeserializeTextbox.Text = AllFrameStringBuilder.ToString();
					LogMessage = "数据帧解析区刷新成功！";
				});
			}
			catch (Exception ex)
			{
				await Dispatcher.InvokeAsync(() => LogMessage = $"刷新解析失败，报错内容：{ex.Message}");
			}
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
			// 先将数字正则化处理
			if (!float.TryParse(Regex.Replace(InstrumentVoltageText.Text, @"[^0-9.+-]", ""), out float voltageTemp))
			{
				LogMessage = "耐压仪电压输入值异常";
				return;
			}
			if (!float.TryParse(Regex.Replace(InstrumentOutputTimeText.Text, @"[^0-9.+-]", ""), out float timeTemp))
			{
				LogMessage = "耐压仪测试时间输入值异常";
				return;
			}
			if (!float.TryParse(Regex.Replace(InstrumentLeakCurrentText.Text, @"[^0-9.+-]", ""), out float InstrumentLeakCurrentTemp))
			{
				LogMessage = "耐压仪漏电电流输入值异常";
				return;
			}
			if (!float.TryParse(Regex.Replace(PlateLeakCurrentText.Text, @"[^0-9.+-]", ""), out float PlateLeakCurrentTemp))
			{
				LogMessage = "耐压板漏电电流输入值异常";
				return;
			}
			if (TestCombinationComboBox.SelectedIndex == -1)
			{
				LogMessage = "未选择耐压组合方式";
				return;
			}
			// 处理数字
			InstrumentVoltage = Math.Max(0f, voltageTemp);
			InstrumentVoltageText.Text = InstrumentVoltage.ToString("F1");
			InstrumentOutputTime = Math.Max(0f, timeTemp);
			InstrumentOutputTimeText.Text = InstrumentOutputTime.ToString("F1");
			InstrumentLeakCurrent = Math.Max(0f, Math.Min(30f, InstrumentLeakCurrentTemp));
			InstrumentLeakCurrentText.Text = InstrumentLeakCurrent.ToString("F2");
			PlateLeakCurrent = Math.Max(0f, Math.Min(5f, PlateLeakCurrentTemp));
			PlateLeakCurrentText.Text = PlateLeakCurrent.ToString("F2");
			// 设置耐压仪
			if (!Instrument.SetModelValue(InstrumentVoltage, InstrumentLeakCurrent, 0f, InstrumentOutputTime, 50, 10, 10))
			{
				LogMessage = "耐压参数设置失败";
			}
			// 设置耐压板
			if (!Plate.SetLeakageI((int)Math.Floor(PlateLeakCurrent * 100)))
			{
				LogMessage = "耐压板漏电电流设置失败";
			}
			if (!Plate.SetTestType(TestCombinationCodeCollection[TestCombinationIndex]))
			{
				LogMessage = "耐压测试组合方式设置失败";
			}
		}
		/// <summary>
		/// 升源指令发送中
		/// </summary>
		private bool OnPowerOn { get; set; } = false;
		/// <summary>
		/// 升源取消发信令牌源
		/// </summary>
		private CancellationTokenSource Cst { get; set; }
		/// <summary>
		/// 升源按钮按下
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void OnPowerOnButtonClick(object sender, RoutedEventArgs e)
		{
			if (OnPowerOn)
			{
				LogMessage = "耐压设备正在预热升源中，请稍等";
				return;
			}
			OnPowerOn = !OnPowerOn;
			bool flag = false;
			Plate.SetStrat((byte)0x32);
			Cst?.Cancel();
			Cst = new CancellationTokenSource();
			try
			{
				await Task.Delay(2000, Cst.Token);
				flag = Plate.SetStrat((byte)0x31);
				flag = Instrument.PowerOn(true) && flag;
			}
			catch (OperationCanceledException)
			{
				LogMessage = "升源被取消";
				return;
			}
			finally
			{
				OnPowerOn = !OnPowerOn;
				Cst.Dispose();
				Cst = null;
			}
			if (flag)
			{
				LogMessage = "耐压实验开始！";
			}
			else
			{
				// 防止器械损坏，升源失败以后要关源
				LogMessage = "耐压仪试验开始失败！";
				Instrument.PowerOn(false);
				Plate.SetStrat((byte)0x30);
			}

		}
		/// <summary>
		/// 设置耐压板漏电电流
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSetPlateLeakCurrentButtonClick(object sender, RoutedEventArgs e)
		{
			// 广播通知所有表位设置新的耐压漏电电流
			if (Plate.SetLeakageI((int)PlateLeakCurrent * 100, 0x80))
			{
				LogMessage = "设置耐压板最大漏电电流成功！";
			}
			else { LogMessage = "设置耐压板最大漏电电流失败！"; }
		}
		/// <summary>
		/// 读取耐压板数据
		/// </summary>
		/// <remarks>从指定表位开始到指定表位结束</remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReadPlateDataButtonClick(object sender, RoutedEventArgs e)
		{
			for(int index = PlateStartSocket; index <= PlateEndSocket; index++)
			{
				if(Plate.GetSamplingValue(0x31,out float sample, (byte)index))
				{
					LogMessage = $"表位号【{index:D2}】最大采样值为：{sample:F2}mA";
				}
				else
				{
					LogMessage = $"表位号【{index:D2}】最大采样失败";
				}
			}
		}
		/// <summary>
		/// 结束实现按钮按下
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPowerOffButtonClick(object sender, RoutedEventArgs e)
		{
			// 先发送关源信号令牌
			Cst?.Cancel();
			if (Instrument.PowerOn(false) && Plate.SetStrat((byte)0x32))
			{
				LogMessage = "耐压实验停止！";
			}
			else
			{
				LogMessage = "耐压试验停止失败！";
			}
		}
		#endregion

		#region 下拉复选框逻辑
		/// <summary>
		/// 耐压板
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTestCombinationComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			TestCombinationIndex = TestCombinationComboBox.SelectedIndex;
		}
		#endregion

		#region 逻辑
		/// <summary>
		/// 初始化数据
		/// </summary>
		private void InitializeData()
		{
			while (FrameQueue.Count > 0)
			{
				FrameQueue.TryDequeue(out _);
			}
			LogMessage = "初始化数据";
			RefreshTextBoxes();
		}
		/// <summary>
		/// 处理log写入事件
		/// </summary>
		/// <param name="msg"></param>
		public void HandleLogWriteEvent(string msg)
		{
			try
			{
				Match match = FrameRegex.Match(msg);
				if (match.Success)
				{
					FrameInfo frameInfo = new FrameInfo()
					{
						Date = Convert.ToDateTime(match.Groups["date"].Value),
						Port = int.Parse(Regex.Replace(match.Groups["port"].Value, @"\D", "")),// 移除所有非数字字符，提取整数
						IsSend = match.Groups["direction"].Value.Trim() == ">>>",
						Frame = match.Groups["frame"].Value.Trim().Replace(" ", "").FromHexString(),
						Remark = match.Groups["remark"].Value,// 数据帧备注
					};
					// 将写入log插入缓存
					if (frameInfo.IsSend)
					{
						SendFrameStringBuilder.Insert(0, msg + "\n");
						SendFrameTextbox.Text = SendFrameStringBuilder.ToString();
						SendFrameTextboxScrollViewer.ScrollToTop();
					}
					else
					{
						ReceiveFrameStringBuilder.Insert(0, msg + "\n");
						ReceiveFrameTextbox.Text = ReceiveFrameStringBuilder.ToString();
						ReceiveFrameTextboxScrollViewer.ScrollToTop();
					}
					// 处理enqueue
					// 控制数据帧缓存大小为200个
					while (FrameQueue.Count >= 200)
					{
						FrameQueue.TryDequeue(out _);
					}
					FrameQueue.Enqueue(frameInfo);
					// 刷新解析区
					HandleNewFrameEnqueue();
				}
				else
				{
					LogMessage = $"正则表达式匹配失败，无效格式信息：{msg}";
				}
			}
			catch (Exception ex)
			{
				LogMessage = $"数据帧处理异常，错误信息：{ex.Message}";
			}
		}
		/// <summary>
		/// 处理新数据帧入队，刷新优先级高于手动刷新
		/// </summary>
		public void HandleNewFrameEnqueue()
		{
			AllFrameStringBuilder.Clear();
			foreach (FrameInfo frameInfo in FrameQueue)
			{
				AllFrameStringBuilder.Insert(0, DeserializeFrame(frameInfo) + "\n");
			}
			FrameDeserializeTextbox.Text = AllFrameStringBuilder.ToString();
			// 更新显示
			RefreshTextBoxes();
			// 滚动至顶部
			FrameDeserializeScrollViewer.ScrollToTop();
		}
		/// <summary>
		/// 解析数据帧信息
		/// </summary>
		/// <param name="frameInfo"></param>
		/// <returns></returns>
		public string DeserializeFrame(FrameInfo frameInfo)
		{
			string result = string.Empty;
			// 判断是否为发送方
			if (frameInfo.IsSend)
			{
				// 判断是否为耐压仪端口
				if (frameInfo.Port == InstrumentCOMPort)
				{
					result = frameInfo.Frame.ParseSendInstrumentFrame();
				}
				// 处理耐压板发送
				else
				{
					result = frameInfo.Frame.ParseSendPlateFrame();
				}
			}
			// 处理接收方数据帧
			else
			{
				// 判断是否为耐压仪端口
				if (frameInfo.Port == InstrumentCOMPort)
				{
					result = frameInfo.Frame.ParseReceiveInstrumentFrame();
				}
				// 处理耐压板接收
				else
				{
					result = frameInfo.Frame.ParseReceivePlateFrame();
				}
			}
			return string.Format("[{0}] {1} {2}",
				frameInfo.Date.ToString("HH:mm:ss"),
				frameInfo.IsSend ? ">>>" : "<<<",
				result);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (Instrument.Connected)
			{
				Instrument.PowerOn(false);
			}
			base.OnClosing(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
		}


		#endregion
	}
}
