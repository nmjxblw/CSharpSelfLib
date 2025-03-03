using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine
{
	/// <summary>
	/// 
	/// </summary>
	[RoutePrefix("api/dopamine")]
	public class DopamineController : ApiController
	{
		/// <summary>
		/// 泛型匹配
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <param name="input"></param>
		/// <returns></returns>
		[System.Web.Http.HttpPost]
		[System.Web.Http.Route("test")]
		public IHttpActionResult Test<TParam>([System.Web.Http.FromBody] TParam input)
		{
			return Ok();
		}
		/// <summary>
		/// string型交流
		/// </summary>
		/// <param name="communicate"></param>
		/// <returns></returns>
		[System.Web.Http.HttpPost]
		[System.Web.Http.Route("test")]
		public IHttpActionResult Test([System.Web.Http.FromBody] CommunicationFormat<string> communicate)
		{
			return Ok(new CommunicationFormat<string>() { Data = "Post 请求接收成功。", ErrorFlag = false, Remark = "null" });
		}
	}


}
