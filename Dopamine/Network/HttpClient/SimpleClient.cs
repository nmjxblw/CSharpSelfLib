using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Dopamine
{
	/// <summary>
	/// 简易客户端
	/// </summary>
	public class SimpleClient : HttpClient
	{
		/// <summary>
		/// 
		/// </summary>
		public SimpleClient()
		{
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="handler"></param>
		public SimpleClient(HttpMessageHandler handler) : base(handler)
		{
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="disposeHandler"></param>
		public SimpleClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
		{
		}
		/// <summary>
		/// 自定义Post
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="input"></param>
		/// <param name="uri"></param>
		/// <returns></returns>
		public TResult? PostTo<TParam, TResult>(TParam input, string uri)
		{
			try
			{
				string inputJson = JsonConvert.SerializeObject(input);
				using StringContent content = new StringContent(inputJson, Encoding.UTF8, "application/json");
				using HttpResponseMessage responseMessage = this.PostAsync(uri, content).Result;
				responseMessage.EnsureSuccessStatusCode();
				return JsonConvert.DeserializeObject<TResult>(responseMessage.Content.ReadAsStringAsync().Result);
			}
			catch (Exception ex)
			{
				ex.Message.ShowInTrace(true);
				return default;
			}
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
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return base.Send(request, cancellationToken);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return base.SendAsync(request, cancellationToken);
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
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}
