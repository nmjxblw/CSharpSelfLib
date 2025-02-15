
namespace Dopamine;
/// <summary>
/// 心跳客户端
/// </summary>
public class DopamineBeatClient : DopamineHttpClient, IDisposable
{
	/// <summary>
	/// 构造
	/// </summary>
	public DopamineBeatClient(string? url) : base(url)
	{
		Initialize();
		Task.Run(Update);
	}
	/// <summary>
	/// 状态机
	/// </summary>
	protected readonly Dictionary<ClientState, ClientStateMachine> State = new Dictionary<ClientState, ClientStateMachine>();
	/// <summary>
	/// 当前客户端状态
	/// </summary>
	public ClientState CurrentState { get; private set; } = ClientState.IDLE;
	/// <summary>
	/// 取消信号源
	/// </summary>
	public virtual CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
	/// <summary>
	/// 运行标识符
	/// </summary>
	public virtual bool IsRunning { get; set; } = true;
	/// <summary>
	/// 是否被释放
	/// </summary>
	public virtual bool Disposed { get; set; } = false;
	/// <summary>
	/// 初始化
	/// </summary>
	protected virtual void Initialize()
	{
		State.Clear();
		State.Add(ClientState.IDLE, new IDLEState(this));
		State.Add(ClientState.Beating, new BeatingState(this));
		SwitchState(ClientState.IDLE);
	}
	/// <summary>
	/// 处理每秒运行逻辑
	/// </summary>
	/// <returns></returns>
	public virtual async Task Update()
	{
		while (IsRunning)
		{
			// TODO: 处理客户端逻辑
			State[CurrentState].OnUpdate();
			await Task.Delay(1000);
		}
		Dispose(true);
	}
	/// <summary>
	/// 状态切换
	/// </summary>
	/// <param name="newState"></param>
	public virtual void SwitchState(ClientState newState)
	{
		State[CurrentState].OnExit();
		CurrentState = newState;
		State[CurrentState].OnEnter();
	}
	/// <summary>
	/// 释放
	/// </summary>
	public virtual void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if (Disposed) return;
		if (disposing)
		{
			IsRunning = false;
			CancellationTokenSource.Cancel();
			CancellationTokenSource.Dispose();
		}
		Disposed = true;
	}
	/// <summary>
	/// 析构
	/// </summary>
	~DopamineBeatClient()
	{
		Dispose(false);
	}
}
/// <summary>
/// 简单客户端状态机，用于处理客户端逻辑
/// </summary>
public abstract class ClientStateMachine(DopamineBeatClient client)
{
	/// <summary>
	/// 父类心跳客户端
	/// </summary>
	protected DopamineBeatClient Client { get; set; } = client;
	/// <summary>
	/// 进入逻辑
	/// </summary>
	public abstract void OnEnter();
	/// <summary>
	/// 实时运行逻辑
	/// </summary>
	public abstract void OnUpdate();
	/// <summary>
	/// 退出逻辑
	/// </summary>
	public abstract void OnExit();
}
/// <summary>
/// 待机逻辑
/// </summary>
/// <param name="client"></param>
public class IDLEState(DopamineBeatClient client) : ClientStateMachine(client)
{
	/// <summary>
	/// 监听
	/// </summary>
	protected HttpListener Listener { get; set; } = new HttpListener();
	/// <summary>
	/// 进入待机时逻辑
	/// </summary>
	public override void OnEnter()
	{
		//HttpResponseMessage? response = null;
	}
	/// <summary>
	/// 退出待机时逻辑
	/// </summary>
	public override void OnExit()
	{

	}
	/// <summary>
	/// 实时运行逻辑
	/// </summary>
	public override void OnUpdate()
	{
		return;
	}
}
/// <summary>
/// 心跳逻辑
/// </summary>
/// <param name="client"></param>
public class BeatingState(DopamineBeatClient client) : ClientStateMachine(client)
{
	/// <summary>
	/// 进入心跳时逻辑
	/// </summary>
	public override void OnEnter()
	{

	}
	/// <summary>
	/// 退出心跳时逻辑
	/// </summary>
	public override void OnExit()
	{

	}
	/// <summary>
	/// 实时运行逻辑
	/// </summary>
	public override void OnUpdate()
	{

	}
}
/// <summary>
/// 
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BeatCommandController : ControllerBase
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="msg"></param>
	[HttpPost]
	public IActionResult Control([FromBody]string msg)
	{
		//try
		//{
		//	msg.Message.ShowInConsole(true);
		//}
		//catch (Exception ex)
		//{
		//	ex.Message.ShowInConsole(true);
		//}
		msg.ShowInConsole(true);
		return Ok();
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
		Error = string.Empty,
		Sender = new
		{
			IP = string.Empty,
			Port = 80,
		},
	};

}
/// <summary>
/// 客户端状态
/// </summary>
public enum ClientState
{
	/// <summary>
	/// 待机
	/// </summary>
	IDLE,
	/// <summary>
	/// 心跳
	/// </summary>
	Beating
}