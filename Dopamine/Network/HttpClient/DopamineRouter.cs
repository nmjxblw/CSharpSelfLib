using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
namespace Dopamine
{
	/// <summary>
	/// Dopamine路由
	/// </summary>
	public class DopamineRouter
	{
		/// <summary>
		/// 构造
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		public DopamineRouter(string ip, int port)
		{
			IP = ip;
			Port = port;
			Config = new HttpSelfHostConfiguration($"http://{IP}:{Port}");
			Config.MapHttpAttributeRoutes();

			// 传统路由（可选保留）
			Config.Routes.MapHttpRoute(
				name: "DopamineApi",
				routeTemplate: "api/Dopamine/{id}",
				defaults: new { controller = "Dopamine", id = RouteParameter.Optional }
			);

			Server = new HttpSelfHostServer(Config);
			Server.OpenAsync().GetAwaiter().GetResult();
			"服务器启动".ShowInConsole(true);
		}
		/// <summary>
		/// 关闭路由
		/// </summary>
		public void Close()
		{
			Server?.Dispose();
			Config?.Dispose();
		}
		private HttpSelfHostServer? Server { get; set; }
		private HttpSelfHostConfiguration? Config { get; set; }
		private string IP { get; set; }
		private int Port { get; set; }
	}
}
