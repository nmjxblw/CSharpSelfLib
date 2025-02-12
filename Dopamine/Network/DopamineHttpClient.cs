namespace Dopamine;
/// <summary>
/// 基于HttpClient客户端
/// </summary>
public class DopamineHttpClient
{
	/// <summary>
	/// 构造
	/// </summary>
	public DopamineHttpClient(string? url = default)
	{
		this.Url = url ?? this.Url;
	}
#if false //暂时移除
	private ClientState _current_state = ClientState.Default;
	/// <summary>
	/// 客户端状态改变事件
	/// </summary>
	public event EventHandler<ClientState>? OnStateChanged;
	/// <summary>
	/// 当前客户端状态
	/// </summary>
	public ClientState CurrentState
	{
		get
		{
			return _current_state;
		}
		set
		{
			if (value != _current_state)
			{
				_current_state = value;
				this.OnStateChanged?.Invoke(this, value);
			}
		}
	}
#endif
	/// <summary>
	/// 目标Url
	/// </summary>
	public virtual string Url { get; set; } = "https://localhost:10001/";
	/// <summary>
	/// 异步获取
	/// </summary>
	/// <param name="url"></param>
	/// <returns></returns>
	public virtual async Task<string?> GetAsync(string? url = default)
	{
		url = url ?? Url;
		try
		{
			using (HttpClient client = new HttpClient())
			{
				HttpResponseMessage response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode();
				string content = await response.Content.ReadAsStringAsync();
				return content;
			}
		}
		catch (Exception ex)
		{
			ex.Message.ShowInTrace(true);
			return null;
		}
	}
	/// <summary>
	/// 异步请求
	/// </summary>
	/// <param name="data"></param>
	/// <param name="url"></param>
	/// <returns></returns>
	public virtual async Task<string?> PostAsync(string data, string? url = default)
	{
		url = url ?? Url;
		try
		{
			using (HttpClient client = new HttpClient())
			{
				StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
				HttpResponseMessage response = await client.PostAsync(url, content);
				response.EnsureSuccessStatusCode();
				string result = await response.Content.ReadAsStringAsync();
				return result;
			}
		}
		catch (Exception ex)
		{
			ex.Message.ShowInTrace(true);
			return null;
		}
	}
}
#if false // 暂时移除
/// <summary>
/// 客户端状态
/// </summary>
public enum ClientState
{
	/// <summary>
	/// 默认
	/// </summary>
	Default,
	/// <summary>
	/// 初始化
	/// </summary>
	Initialized,
	/// <summary>
	/// 闲置状态
	/// </summary>
	IDLE,
	/// <summary>
	/// 联机中
	/// </summary>
	Connected,
	/// <summary>
	/// 联机断开
	/// </summary>
	Disconnected,
	/// <summary>
	/// 匹对中
	/// </summary>
	Pairing,
}
#endif