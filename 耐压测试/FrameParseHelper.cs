using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltageInsulationTest
{
	/// <summary>
	/// 针对耐压仪和耐压板数据帧解析类
	/// </summary>
	public static class FrameParseHelper
	{
		#region 数据帧解析逻辑
		/// <summary>
		/// 解析发给耐压仪的数据帧
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static string ParseSendInstrumentFrame(this byte[] frame)
		{
			if (frame[0] != 0x7B || frame[frame.Length - 1] != 0x7D)
			{
				return "耐压仪发送数据帧格式不正确,数据帧头应为7B，帧尾应为7D";
			}
			if (frame.Length != frame[1])
			{
				return $"耐压仪发送数据帧格式不正确,数据长度应为{frame.Length:X2}";
			}
			// 除去帧头帧尾以及和校验
			byte[] temp = frame.SubFrame(1, frame.Length - 3);
			// 检验和校验
			byte sum = temp.GetSum();
			if (sum != frame[frame.Length - 2])
			{
				return $"耐压仪发送数据帧格式不正确，和校验应为{sum:X2}";
			}
			// 耐压仪地址
			byte address = frame[2];
			// 命令码
			byte controlCode = frame[3];
			switch (controlCode)
			{
				case 0x00:// 读取测试值结论
					{
						return $"读取耐压仪[地址：{address:D3}]的测试值结论";
					}
				case 0x01:// 启动测试（相当于<START>键）
					{
						return $"耐压仪[地址：{address:D3}]启动测试";
					}
				case 0x02:// 停止（相当于<STOP>键）
					{
						return $"耐压仪[地址：{address:D3}]停止测试";
					}
				case 0x03:// 选择测试方式(测试条件)
					{
						byte parameter = frame[4];
						string testType = string.Empty;
						switch (parameter)
						{
							case 0x00:
								testType = "ACW 测试";
								break;
							case 0x01:
								testType = "IR 测试";
								break;
							case 0x02:
								testType = "ACW--IR 测试";
								break;
							case 0x03:
								testType = "IR--ACW 测试";
								break;
							default:
								testType = "未知";
								break;
						}
						return $"耐压仪[地址：{address:D3}]选择测试方式：{testType}";
					}
				case 0x04:// 读取当前测试方式下的设置参数
					{
						return $"耐压仪[地址：{address:D3}]读取当前测试方式下的参数设置";

					}
				case 0x05:// 读取系统设置
					{
						return $"耐压仪[地址：{address:D3}]读取当前的系统参数设置";

					}
				case 0x06:// 预置测试参数
					{
						if (frame[1] == 0x10)
						{
							// IR测试
							short voltage = frame.SubFrame(4, 2).GetInt16();
							int lowerResistance = (int)frame.SubFrame(6, 3).ToFitNumber();
							int upperResistance = (int)frame.SubFrame(9, 3).ToFitNumber();
							float testTime = frame.SubFrame(12, 2).GetInt16() / 10f;
							return $"耐压仪[地址：{address:D3}]预置IR测试参数：电压{voltage}V；电阻下限{lowerResistance}MΩ；电阻上限{upperResistance}MΩ；测试时间{testTime}s";
						}
						else if (frame[1] == 0x17)
						{
							// ACW测试
							short voltage = frame.SubFrame(4, 2).GetInt16();
							float upperCurrent = (int)frame.SubFrame(6, 3).ToFitNumber() / 1000f;
							float lowerCurrent = (int)frame.SubFrame(9, 3).ToFitNumber() / 1000f;
							float testTime = frame.SubFrame(12, 2).GetInt16() / 10f;
							short frequency = frame[14];
							float smoothUpTime = frame.SubFrame(15, 2).GetInt16() / 10f;
							float smoothDownTime = frame.SubFrame(17, 2).GetInt16() / 10f;
							return $"耐压仪[地址：{address:D3}]预置ACW测试参数：电压{voltage}V；电流上限{upperCurrent}mA；电流下限{lowerCurrent}mA；测试时间{testTime}s；频率{frequency}Hz；缓升{smoothUpTime}s；缓降{smoothDownTime}s";
						}
						else if (frame[1] == 0x1F)
						{
							// 复合测试
							return $"耐压仪[地址：{address:D3}]设置复合测试参数，缺少必要信息，无解析";
						}
						break;
					}
				case 0x07:// 设置接地方式（GUARD / RETURN）
					{
						return $"耐压仪[地址：{address:D3}]设置接地方式为{(frame[4] == 0x00 ? "保护接地方式" : "回路接地方式")}";
					}
				case 0x08:// 设置启动控制方式
					{
						switch (frame[4])
						{
							case 0x00:
								{
									return $"耐压仪[地址：{address:D3}]设置通讯启动有效";
								}
							case 0x01:
								{
									return $"耐压仪[地址：{address:D3}]设置 PLC 遥控口有效";

								}
							case 0x02:
								{
									return $"耐压仪[地址：{address:D3}]设置前面启动键有效";
								}
							default:
								return $"耐压仪[地址：{address:D3}]设置启动控制方式未知";

						}
					}
				case 0x09:// 设置快测功能打开/关闭
					{
						return $"耐压仪[地址：{address:D3}]设置快测功能{(frame[4] == 0x00 ? "关闭" : "打开")}";
					}
				case 0x0A:// 读当前是否打开快测功能
					{
						return $"耐压仪[地址：{address:D3}]读取仪表是否已经打开快测功能";
					}
				case 0x0B:// 读取启动控制方式
					{
						return $"耐压仪[地址：{address:D3}]读取读取当前的启动控制方式";
					}
				default:
					return "数据帧格式不正确，命令字是无效命令";
			}
			return string.Empty;
		}
		/// <summary>
		/// 解析发给耐压板的数据帧
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static string ParseSendPlateFrame(this byte[] frame)
		{
			if (frame[0] != 0x4E && frame[frame.Length - 1] != 0x45)
			{
				return "耐压板发送数据帧格式不正确,数据帧头应为4E，帧尾应为45";
			}
			byte sum = frame.SubFrame(0, frame.Length - 2).GetSum();
			if (frame[frame.Length - 2] != sum)
			{
				return $"耐压板发送数据帧格式不正确，和校验应为{sum:X2}";
			}
			int socketIndex = frame[1] - 0x80;
			switch (frame[2])
			{
				case 0x11:
					{
						if (frame[3] != 0x02) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						short time = frame.SubFrame(4, 2).GetInt16();
						return string.Format("设置耐压板[{0}]{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							time == 0 ? "连续测试" : $"测试时间{time}s");
					}
				case 0x12:
					{
						if (frame[3] != 0x02) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						float current = frame.SubFrame(4, 2).GetInt16() / 100f;
						return string.Format("设置耐压板[{0}]{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							$"漏电电流{current}mA");
					}
				case 0x13:
					{
						if (frame[3] != 0x01) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						string cmd = string.Empty;
						switch (frame[4])
						{
							case 0x31:
								cmd = "开始测试";
								break;
							case 0x30:
								cmd = "停止测试,保持最大值显示状态";
								break;
							case 0x32:
								cmd = "复位(继电器释放),显示清零";
								break;
							case 0x33:
								cmd = "加标准采样信号，执行AD校准功能";
								break;
							default:
								cmd = "未知指令";
								break;
						}
						return string.Format("耐压板[{0}]执行指令：{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							cmd);
					}
				case 0x14:
					{
						if (frame[3] != 0x02) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						float coefficient = frame.SubFrame(4, 2).GetInt16() / 100f;
						return string.Format("设置耐压板[{0}]{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							$"漏电流设置值修正系数{coefficient:F2}%");
					}
				case 0x15:
					{
						if (frame[3] != 0x02) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						float coefficient = frame.SubFrame(4, 2).GetInt16() / 100f;
						return string.Format("设置耐压板[{0}]{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							$"漏电流采样值修正系数{coefficient:F2}%");
					}
				case 0x16:
					{
						if (frame[3] != 0x01) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						string cmd = string.Empty;
						switch (frame[4])
						{
							case 0x31:
								cmd = "读取当前测试中漏电流采样值（最小值）";
								break;
							case 0x30:
								cmd = "读取当前测试中漏电流采样值（最大值）";
								break;
							case 0x32:
								cmd = "读取当前测试中漏电流采样值（实时值）";
								break;
							default:
								cmd = "未知指令";
								break;
						}
						return string.Format("耐压板[{0}]执行指令：{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							cmd);
					}
				case 0x26:
					{
						if (frame[3] != 0x02) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						return string.Format("耐压板[{0}]{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							"返回对应表位的漏电流采样值（最小值)");
					}
				case 0x27:
					{
						if (frame[3] != 0x02) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						return string.Format("耐压板[{0}]{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							"返回对应表位的漏电流采样值（最大值）");
					}
				case 0x28:
					{
						if (frame[3] != 0x02) return "耐压板发送数据帧格式不正确，命令字是无效命令";
						return string.Format("耐压板[{0}]{1}",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							"返回对应表位的漏电流采样值（实时值)");
					}
				case 0x20:
					{
						if (frame[1] != 0xEA || frame[3] != 0x01)
						{
							return "耐压板发送数据帧格式不正确，命令字是无效命令";
						}
						string cmd = string.Empty;
						switch (frame[4])
						{
							case 0x30:
								cmd = "1";
								break;
							case 0x31:
								cmd = "2";
								break;
							case 0x32:
								cmd = "3";
								break;
							case 0x33:
								cmd = "4";
								break;
							case 0x34:
								cmd = "5";
								break;
							case 0x35:
								cmd = "6";
								break;
							case 0x36:
								cmd = "7";
								break;
							default:
								cmd = "未知";
								break;
						}
						return string.Format("耐压板切换测试组合{0}", cmd);
					}
				default:
					return "耐压板发送数据帧格式不正确，命令字是无效命令";
			}
		}
		/// <summary>
		/// 解析从耐压仪发来的数据帧
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static string ParseReceiveInstrumentFrame(this byte[] frame)
		{
			if (frame[0] != 0x7B || frame[frame.Length - 1] != 0x7D)
			{
				return "耐压仪接收帧格式不正确,数据帧头应为0x7B，帧为应为0x7D";
			}
			byte sum = frame.GetSum(1, frame.Length - 3);
			if (frame[frame.Length - 2] != sum)
			{
				return $"耐压仪接收帧格式不正确，和校验应为{sum:X2}";
			}
			if (frame.Length != frame[1])
			{
				return $"耐压仪接收帧格式不正确,数据长度应为{frame.Length:X2}";
			}
			else if (frame[1] == 6)
			{
				// Ascii应答,数据位2位
				return $"耐压仪应答： {Encoding.ASCII.GetString(frame.SubFrame(2, 2))}";
			}
			else if (frame[1] == 0x05)
			{
				switch (frame[2])
				{
					case 0x00:
						return "耐压仪接地方式为保护接地；PLC 控制关闭";
					case 0x01:
						return "耐压仪接地方式为回路接地；PLC 控制关闭";
					case 0x02:
						return "耐压仪接地方式为保护接地；PLC 控制打开";
					case 0x03:
						return "耐压仪接地方式为回路接地；PLC 控制打开";
					case 0x10:
						return "耐压仪快测功能已关";
					case 0x20:
						return "耐压仪快测功能已开";
					case 0x30:
						return "耐压仪当前启动控制方式：前面板启动键启动测试";
					case 0x40:
						return "耐压仪当前启动控制方式：PLC 遥控口启动测试";
					case 0x50:
						return "耐压仪当前启动控制方式：RS232/485 启动命令启动测试";
					default:
						return "耐压仪发送了无效回复";
				}
			}
			else if (frame[1] == 0x0E)
			{
				short voltage = frame.SubFrame(2, 2).GetInt16();
				float lowerResistance = (int)frame.SubFrame(4, 3).ToFitNumber() / 100f;
				float upperResistance = (int)frame.SubFrame(7, 3).ToFitNumber() / 100f;
				float testTime = frame.SubFrame(10, 2).GetInt16() / 10f;
				return string.Format("耐压仪设置IR测试电压{0}V；电阻下限{1}MΩ；电阻上限{2}MΩ；测试时间{3}s", voltage, lowerResistance, upperResistance, testTime);
			}
			else if (frame[1] == 0x15)
			{
				short voltage = frame.SubFrame(2, 2).GetInt16();
				float upperCurrent = frame.SubFrame(4, 3).GetInt16() / 1000f;
				float lowerCurrent = frame.SubFrame(7, 3).GetInt16() / 1000f;
				float testTime = frame.SubFrame(10, 2).GetInt16() / 10f;
				short frequency = frame[12];
				float smoothUpTime = frame.SubFrame(13, 2).GetInt16() / 10f;
				float smoothDownTime = frame.SubFrame(15, 2).GetInt16() / 10f;
				return string.Format(
					"耐压仪设置ACW测试电压{0}V；电流上限{1}mA；电流下限{2}mA；测试时间{3}s；频率{4}Hz；缓升{5}s；缓降{6}s",
					voltage,
					upperCurrent,
					lowerCurrent,
					testTime,
					frequency,
					smoothUpTime,
					smoothDownTime);
			}
			else if (frame[1] == 19)
			{
				// 处理读取测试结论 00H
				// 先获取数据体，共14字节
				byte[] body = frame.SubFrame(2, 15);
				short withstandVoltage = body.SubFrame(0, 2).GetInt16();
				float withstandCurrent = (int)body.SubFrame(2, 3).ToFitNumber() / 1000f;
				float withstandRemainingTime = body.SubFrame(5, 2).GetInt16() / 10f;
				short insulationVoltage = body.SubFrame(7, 2).GetInt16();
				float insulationResistance = (int)body.SubFrame(9, 3).ToFitNumber() / 100f;
				float insulationRemainingTime = body.SubFrame(12, 2).GetInt16() / 10f;
				byte resultByte = body[14];
				string ACWTest_Qualified = (resultByte & (1 << 0)) != 0 ? "合格" : "不合格";
				string ACWTest_UpperQualified = (resultByte & (1 << 1)) != 0 ? "合格" : "不合格";
				string ACWTest_LowerQualified = (resultByte & (1 << 2)) != 0 ? "合格" : "不合格";
				string IRTest_Qualified = (resultByte & (1 << 3)) != 0 ? "合格" : "不合格";
				string IRTest_UpperQualified = (resultByte & (1 << 4)) != 0 ? "合格" : "不合格";
				string IRTest_LowerQualified = (resultByte & (1 << 5)) != 0 ? "合格" : "不合格";
				string resultString = (resultByte & (1 << 6)) != 0 ?
					(resultByte & (1 << 7)) != 0 ? "故障状态" : "ACW测试中"
					: (resultByte & (1 << 7)) != 0 ? "IR测试中" : "测试完成";
				return string.Format(
					"耐电压测试实际输出电压值{0}V;" +
					"耐电压击穿电流测试值{1}mA;" +
					"耐电压实际测试剩余时间{2}s;" +
					"绝缘测试实际输出电压值{3}V;" +
					"绝缘电阻测试值{4}MΩ;" +
					"绝缘实际测试剩余时间{5}s;" +
					"ACW测试结果：{6};" +
					"ACW测试上限：{7};" +
					"ACW测试下限：{8};" +
					"IR测试结果：{9};" +
					"IR测试上限：{10};" +
					"IR测试下限：{11};" +
					"结论：{12}",
					withstandVoltage,
					withstandCurrent,
					withstandRemainingTime,
					insulationVoltage,
					insulationResistance,
					insulationRemainingTime,
					ACWTest_Qualified,
					ACWTest_UpperQualified,
					ACWTest_LowerQualified,
					IRTest_Qualified,
					IRTest_UpperQualified,
					IRTest_LowerQualified,
					resultString);
			}
			return string.Format("耐压仪接收了无法解析的数据帧");
		}
		/// <summary>
		/// 解析从耐压板发来的数据帧
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static string ParseReceivePlateFrame(this byte[] frame)
		{
			int socketIndex = frame[1] - 0x80;
			if (frame[0] != 0x4F || frame[frame.Length - 1] != 0x45)
			{
				return string.Format("耐压板[{0}]接收帧格式不正确，帧头应为4F，帧尾应为0x45",
					socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}");
			}
			switch (frame[2])
			{
				case 0x26:
					{
						if (frame[3] != 0x02)
						{
							return string.Format("耐压板{0}接收数据帧格式不正确，命令字是无效命令",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}");
						}
						float value = frame.SubFrame(4, 2).GetInt16() / 100f;
						return string.Format("耐压板[{0}]返回对应表位的漏电流采样值（最小值){1}mA",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							value);
					}
				case 0x27:
					{
						if (frame[3] != 0x02)
						{
							return string.Format("耐压板{0}接收数据帧格式不正确，命令字是无效命令",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}");
						}
						float value = frame.SubFrame(4, 2).GetInt16() / 100f;
						return string.Format("耐压板[{0}]返回对应表位的漏电流采样值（最大值）{1}mA",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							value);
					}
				case 0x28:
					{
						if (frame[3] != 0x02)
						{
							return string.Format("耐压板{0}接收数据帧格式不正确，命令字是无效命令",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}");
						}
						float value = frame.SubFrame(4, 2).GetInt16() / 100f;
						return string.Format("耐压板[{0}]返回对应表位的漏电流采样值（实时值）{1}mA",
							socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}",
							value);
					}
				default:
					return string.Format("耐压板[{0}]接收了无解析的指令",
					socketIndex == 0 ? "所有表位" : $"表位号：{socketIndex:D3}");
			}
		}
		#endregion
	}
}
