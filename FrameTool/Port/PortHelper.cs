using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Net.NetworkInformation;
namespace FrameTool
{
	/// <summary>
	/// 串口助手类
	/// </summary>
	public class PortHelper : IDisposable
	{
		private SerialPort? serialPort;
		public int MaxWaitSeconds { get; set; } = 1000;
		/// <summary>
		/// 释放资源
		/// </summary>
		public void Dispose()
		{
			if (serialPort != null)
			{
				if (serialPort.IsOpen)
				{
					serialPort.Close();
				}
				serialPort.Dispose();
			}
			serialPort = null;
		}
		/// <summary>
		/// 设置串口参数
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		public bool SetPortParams(string settings)
		{
			try
			{
				serialPort = new SerialPort();
				Regex regex = new Regex(@"^\s+?(?<BaudRate>\d+),(?<DataBits>\d+),(?<Parity>[NnOoEeSs]),(?<StopBits>\d+)\s+?$", RegexOptions.IgnoreCase);
				if (regex.IsMatch(settings))
				{
					serialPort.BaudRate = int.Parse(regex.Match(settings).Groups["BaudRate"].Value);
					serialPort.DataBits = int.Parse(regex.Match(settings).Groups["DataBits"].Value);
					serialPort.Parity = regex.Match(settings).Groups["Parity"].Value.ToLower() switch
					{
						"o" => Parity.Odd,
						"e" => Parity.Even,
						"s" => Parity.Space,
						_ => Parity.None
					};
					serialPort.StopBits = regex.Match(settings).Groups["StopBits"].Value switch
					{
						"1" => StopBits.One,
						"2" => StopBits.Two,
						_ => StopBits.OnePointFive
					};
				}
				else
				{
					Console.WriteLine("设置串口参数失败: 参数格式错误");
					return false;
				}
				return true;
			}
			catch
			(Exception ex)
			{
				Console.WriteLine($"设置串口参数失败: {ex.Message}");
				return false;
			}
		}

		public bool TryOpenPort(int portIndex = 1)
		{
			if (serialPort == null)
			{
				Console.WriteLine("串口未初始化");
				return false;
			}
			if (serialPort.IsOpen)
			{
				Console.WriteLine("串口已打开");
				return true;
			}
			serialPort.PortName = $"COM{portIndex}";
			serialPort.DtrEnable = true;
			try
			{
				serialPort.Open();
				Console.WriteLine($"串口 {serialPort.PortName} 打开成功");
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				Console.WriteLine($"串口 {serialPort.PortName} 被占用");
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"打开串口失败: {ex.Message}");
				return false;
			}
		}
		public bool TrySendFrame(byte[] sendFrame, out byte[] replyFrame)
		{
			replyFrame = new byte[0];
			if (serialPort == null)
			{
				Console.WriteLine("串口未初始化");
				return false;
			}
			if (!serialPort.IsOpen)
			{
				Console.WriteLine("串口未打开");
				return false;
			}
			try
			{
				serialPort.Write(sendFrame, 0, sendFrame.Length);
				Console.WriteLine($"发送数据: {BitConverter.ToString(sendFrame)}");
				TimeSpan timeSpan = TimeSpan.FromMilliseconds(MaxWaitSeconds);
				DateTime timeStamp = DateTime.Now;
				while (DateTime.Now - timeStamp < timeSpan)
				{
					if (serialPort.BytesToRead > 0)
					{
						replyFrame = new byte[serialPort.BytesToRead];
						serialPort.Read(replyFrame, 0, replyFrame.Length);
						Console.WriteLine($"接收数据: {BitConverter.ToString(replyFrame)}");
						break;
					}
				}
				if (replyFrame == null)
				{
					Console.WriteLine("接收数据超时");
					return false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"发送数据失败: {ex.Message}");
				return false;
			}
			return true;
		}
	}
}
