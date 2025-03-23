using System.Text.Json;
using System.Net.Http;
using System.Text;

namespace DeepSeekApi
{
	/// <summary>
	/// Deep Seek控制静态类
	/// </summary>
	public static class DeepSeekControl
	{
		/// <summary>
		/// 构造DeepSeek控制类
		/// </summary>
		static DeepSeekControl()
		{

		}
		static string think = string.Empty;
		/// <summary>
		/// 返回上次思考的内容
		/// </summary>
		public static string Think => think;
		static string answer = string.Empty;
		/// <summary>
		/// 返回上次思考结果
		/// </summary>
		public static string Answer => answer;
		static bool error = false;
		/// <summary>
		/// 返回上次访问是否发生错误
		/// </summary>
		public static bool Error => error;
		/// <summary>
		/// 将文本发送给DeepSeek并获得思考过程和结果
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static async Task AskDeepSeek(this string input)
		{
			using HttpClient httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ConfigManager.Data.ApiKey}");

			object requestData = new
			{
				model = "deepseek-reasoner",
				messages = new[]
				{
					new
					{
						role = "user",
						content = input
					}
				},
			};

			try
			{
				JsonSerializerOptions jsonOptions = new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase // 默认已是camelCase，此处仅为示例
				};
				HttpResponseMessage response = await httpClient.PostAsync(ConfigManager.Data.ApiUrl,
					new StringContent(JsonSerializer.Serialize(requestData, jsonOptions),
					Encoding.UTF8,
					"application/json"));

				response.EnsureSuccessStatusCode();

				string responseContent = await response.Content.ReadAsStringAsync();
				Recorder.Write(responseContent);
				using JsonDocument doc = JsonDocument.Parse(responseContent);
				JsonElement root = doc.RootElement;

				// 假设响应结构包含思考过程
				//if (root.TryGetProperty("reasoning_content", out JsonElement reasoningSteps))
				//{
				//	StringBuilder stringBuilder = new StringBuilder();
				//	stringBuilder.AppendLine();
				//	foreach (JsonElement step in reasoningSteps.EnumerateArray())
				//	{
				//		stringBuilder.AppendLine($"- {step.GetString()}");
				//	}
				//	think = stringBuilder.ToString();
				//}

				if (root.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
				{
					answer = "\n" + choices[0].GetProperty("message").GetProperty("content").GetString() ?? "无回复";
					think = "\n" + choices[0].GetProperty("message").GetProperty("reasoning_content").GetString() ?? "无思考";
				}
			}
			catch (HttpRequestException e)
			{
				error = true;
				answer = $"请求错误: {e.Message}";
			}
			catch (Exception e)
			{
				error = true;
				answer = $"错误: {e.Message}";
			}
		}
	}
}
