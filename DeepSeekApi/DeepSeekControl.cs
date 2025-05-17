using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Formats.Asn1;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
        /// <summary>
        /// 内置计时器
        /// </summary>
        private static Stopwatch Stopwatch { get; } = new Stopwatch();
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
        /// 信息队列
        /// </summary>
        private static List<object> message = new List<object>();
        /// <summary>
        /// 信息队列
        /// </summary>
        public static List<object> Message => message;
        public static dynamic? Data { get; set; } = new DynamicClass();
        private static JsonSerializerOptions sendOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 默认已是camelCase，此处仅为示例
        };
        private static JsonSerializerOptions receiveOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };
        public static bool IsWaitingForAnswer = false;
        /// <summary>
        /// 将文本发送给DeepSeek并获得思考过程和结果
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<string> AskDeepSeek(this string input)
        {
            IsWaitingForAnswer = true;
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ConfigManager.Data.ApiKey}");
            object tempObject = new
            {
                role = "user",
                content = input
            };
            message.Add(tempObject);
            object requestData = new
            {
                model = "deepseek-reasoner",
                messages = message.ToArray(),
            };

            try
            {
                Recorder.Write("提问：" + input);
                HttpResponseMessage response = await httpClient.PostAsync(ConfigManager.Data.ApiUrl,
                    new StringContent(JsonSerializer.Serialize(requestData, sendOptions),
                    Encoding.UTF8,
                    "application/json"));

                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();

                JsonDocument document = JsonDocument.Parse(responseContent);
                responseContent = JsonSerializer.Serialize(document.RootElement, receiveOptions);
                Recorder.Write("\n" + responseContent + "\n");
                Data = JsonSerializer.Deserialize<DynamicClass>(responseContent)!;
                answer = string.Empty;
                think = string.Empty;
                if (Data.choices != null)
                {
                    tempObject = new
                    {
                        role = Data.choices[0].message?.role?.ToString() ?? "assistant",
                        content = Data.choices[0].message?.content?.ToString(),
                    };
                    message.Add(tempObject);
                    answer = "\n" + Data.choices[0].message?.content?.ToString() ?? "无回复";
                    think = "\n" + Data.choices[0].message?.reasoning_content?.ToString() ?? "无思考";
                }
            }
            catch (HttpRequestException e)
            {
                message.RemoveAt(message.Count - 1);
                error = true;
                answer = $"请求错误: {e.Message}";
            }
            catch (Exception e)
            {
                message.RemoveAt(message.Count - 1);
                error = true;
                answer = $"错误信息: {e.Message}";
            }
            finally
            {
                IsWaitingForAnswer = false;
            }
            return answer;
        }
        /// <summary>
        /// 运行DeepSeek控制台程序
        /// </summary>
        public static void Run()
        {
            string? input = string.Empty;
            async Task DisplayWaiting()
            {
                int counter = 0;
                string SleepChars = "......";
                StringBuilder sb = new();
                Stopwatch.Restart();
                while (IsWaitingForAnswer)
                {
                    Console.Write($"思考中\t");
                    sb.Clear();
                    sb.Append(SleepChars[..counter].PadRight(SleepChars.Length));
                    Console.Write(sb.ToString());
                    Console.Write(string.Format("经过了{0,10}s", ((double)Stopwatch.ElapsedTicks / Stopwatch.Frequency).ToString("F2")));
                    counter++;
                    counter %= sb.Length + 1;
                    await Task.Delay(100);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
                Stopwatch.Stop();
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
                await Task.Delay(1);
            }
            int loopCount = 1;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{loopCount}.提问:");

                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.ToLower().Equals("quit") || input.ToLower().Equals("q"))
                {
                    break;
                }
                IsWaitingForAnswer = true;
                Task.Run(async () => await DisplayWaiting());
                Console.ForegroundColor = (ConsoleColor)11;
                string reply = $"\n{loopCount}.回复： {input.AskDeepSeek().GetAwaiter().GetResult()}\n";

                Console.WriteLine(reply);
                Thread.Sleep(100);
                Console.WriteLine();
                loopCount++;
            }
        }

    }
}
