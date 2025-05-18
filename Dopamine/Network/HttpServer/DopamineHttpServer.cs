using System.Web.Http;

namespace Dopamine;
/// <summary>
/// 基于HttpServer
/// </summary>
public class DopamineHttpServer : HttpServer
{
	/// <summary>
	/// 
	/// </summary>
	public DopamineHttpServer() : base()
	{
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="configuration"></param>
	public DopamineHttpServer(HttpConfiguration configuration) : base(configuration)
	{
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="dispatcher"></param>
	public DopamineHttpServer(HttpMessageHandler dispatcher) : base(dispatcher)
	{
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="dispatcher"></param>
	public DopamineHttpServer(HttpConfiguration configuration, HttpMessageHandler dispatcher) : base(configuration, dispatcher)
	{
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public override bool Equals(object? obj)
	{
		return base.Equals(obj);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public override string? ToString()
	{
		return base.ToString();
	}
	/// <summary>
	/// 手动释放资源
	/// </summary>
	/// <param name="disposing"></param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
	/// <summary>
	/// 初始化
	/// </summary>
	protected override void Initialize()
	{
		base.Initialize();
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return base.Send(request, cancellationToken);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return base.SendAsync(request, cancellationToken);
	}
}
