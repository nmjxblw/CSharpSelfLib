using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 数据帧结构体
/// </summary>
/// <remarks>结构体中存入了数据帧的时间戳，端口号，发送方向，数据帧本体以及备注</remarks>
public struct FrameInfo
{
	/// <summary>
	/// 数据帧内容
	/// </summary>
	public byte[] Frame { get; set; }
	/// <summary>
	/// 是否是发送方
	/// </summary>
	public bool IsSend { get; set; }
	/// <summary>
	/// 备注
	/// </summary>
	public string Remark { get; set; }
	/// <summary>
	/// 日期
	/// </summary>
	public DateTime Date { get; set; }
	/// <summary>
	/// 端口号
	/// </summary>
	public int Port { get; set; }
}
