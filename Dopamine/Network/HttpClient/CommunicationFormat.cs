using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Json请求体的通信格式
/// </summary>
public partial class CommunicationFormat<T>
{
	/// <summary>
	/// 主体信息
	/// </summary>
	public T? Data { get; set; } = default;
	/// <summary>
	/// 备注内容
	/// </summary>
	/// <remarks></remarks>
	public string Remark { get; set; } = "null";
	/// <summary>
	/// 错误信号标识符
	/// </summary>
	public bool ErrorFlag { get; set; } = false;
}
