namespace Dopamine;
/// <summary>
/// 基于HttpClient客户端
/// </summary>
public class DopamineHttpClient : HttpClient
{
	/// <summary>
	/// 构造
	/// </summary>
	public DopamineHttpClient() : base()
	{
	}

	/// <summary>
	/// 构造
	/// </summary>
	public DopamineHttpClient(string? url = default) : base()
	{
		this.Url = url ?? this.Url;
	}
	/// <summary>
	/// 构造
	/// </summary>
	/// <param name="handler"></param>
	public DopamineHttpClient(HttpMessageHandler handler) : base(handler)
	{
	}
	/// <summary>
	/// 构造
	/// </summary>
	/// <param name="handler"></param>
	/// <param name="disposeHandler"></param>
	public DopamineHttpClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
	{
	}

	/// <summary>
	/// 目标Url
	/// </summary>
	public virtual string Url { get; set; } = "https://localhost:10001/";
	/// <summary>
	/// 异步POST请求String型消息
	/// </summary>
	/// <param name="stringMessage"></param>
	/// <param name="url"></param>
	/// <returns></returns>
	public virtual async Task<string?> PostStringAsync(string stringMessage, string? url = default)
	{
		url = url ?? Url;
		try
		{
			Console.WriteLine("Message:{0}\nUrl:{1}", stringMessage, url);
			StringContent content = new StringContent(stringMessage, Encoding.UTF8, "application/json");
			HttpResponseMessage response = await this.PostAsync(url, content);
			response.EnsureSuccessStatusCode();
			$"Uri:{response.RequestMessage?.RequestUri?.ToString()}".ShowInConsole();
			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception ex)
		{
			ex.Message.ShowInTrace(true);
			return null;
		}
	}
	/// <summary>
	/// 异步GET请求API
	/// </summary>
	/// <param name="APIString"></param>
	/// <param name="url"></param>
	/// <returns></returns>
	public virtual async Task<string?> GetAPIAsync(string APIString, string? url = default)
	{
		url = url ?? Url;
		try
		{
			url = string.Format("{0}/api/{1}", url, APIString);
			HttpResponseMessage response = await this.GetAsync(url);
			response.EnsureSuccessStatusCode();
			$"Uri:{response.RequestMessage?.RequestUri?.ToString()}".ShowInConsole();
			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception ex)
		{
			ex.Message.ShowInTrace(true);
			return null;
		}
	}
}

/// <summary>
/// Message类
/// </summary>
public class HttpMessage
{
	/// <summary>
	/// HttpBody中的消息
	/// </summary>
	public string Message { get; set; } = string.Empty;
	/// <summary>
	/// 标识符
	/// </summary>
	public object Flag { get; set; } = new
	{
		Error = "None",
		Sender = new
		{
			IP = "127.0.0.1",
			Port = 10001,
		},
	};
}
