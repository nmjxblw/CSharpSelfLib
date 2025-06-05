using LLama.Common;
using LLama;
using System.Text;
using System.Windows.Forms;

namespace DeepSeekApi
{
    public partial class LLamaApplication
    {
        /// <summary>
        /// 模型路径
        /// </summary>
        public static string ModelPath
        {
            get
            {
                string path = string.Empty;
                foreach (string modelLocalName in ConfigManager.Data.Model)
                {
                    if (string.IsNullOrWhiteSpace(modelLocalName))
                        continue;
                    path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", modelLocalName);
                    if (File.Exists(path))
                        break; // 找到第一个有效的模型文件后退出循环
                }
                if (!File.Exists(path))
                {
                    throw new Exception("未找到有效的模型文件，请检查配置文件中的模型路径设置。");
                }
                else
                {
                    Console.WriteLine($"模型文件路径: {path}");
                }
                return path;
            }
        }
        public static string SystemRole => ConfigManager.Data.SystemRole;
        /// <summary>
        /// 编码转化
        /// </summary>
        /// <param name="input"></param>
        /// <param name="original"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string ConvertEncoding(string input, Encoding original, Encoding target)
        {
            byte[] bytes = original.GetBytes(input);
            byte[] convertedBytes = Encoding.Convert(original, target, bytes);
            return target.GetString(convertedBytes);
        }
        /// <summary>
        /// 主运行程序
        /// </summary>
        /// <returns></returns>
        public static async Task Run()
        {
            // Register provider for GB2312 encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.ForegroundColor = ConsoleColor.White;

            ModelParams parameters = new ModelParams(ModelPath)
            {
                ContextSize = 1024,
                GpuLayerCount = 0,
                Encoding = Encoding.UTF8
            };
            using LLamaWeights model = LLamaWeights.LoadFromFile(parameters);
            using LLamaContext context = model.CreateContext(parameters);
            InteractiveExecutor executor = new InteractiveExecutor(context);

            ChatSession session;
            ChatHistory chatHistory = new ChatHistory();
            chatHistory.AddMessage(AuthorRole.System, ConvertEncoding(ConfigManager.Data.SystemRole, Encoding.GetEncoding("gb2312"), Encoding.UTF8));
            chatHistory.AddMessage(AuthorRole.User, ConvertEncoding(ConfigManager.Data.UserRole, Encoding.GetEncoding("gb2312"), Encoding.UTF8));
            chatHistory.AddMessage(AuthorRole.Assistant, ConvertEncoding(ConfigManager.Data.AssistantRole, Encoding.GetEncoding("gb2312"), Encoding.UTF8));
            session = new ChatSession(executor, chatHistory);

            session
                .WithHistoryTransform(new LLamaTransforms.DefaultHistoryTransform());

            InferenceParams inferenceParams = new InferenceParams()
            {
                MaxTokens = -1,
                //AntiPrompts = new List<string> { "用户：" }
            };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The chat session has started.");

            // show the prompt
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("用户：");
            Console.ForegroundColor = ConsoleColor.Green;
            string userInput = Console.ReadLine() ?? "";

            while (true)
            {
                if (userInput.ToLower().Equals("exit")
                    || userInput.ToLower().Equals("quit")
                    || userInput.ToLower().Equals("q")) break;
                // Convert the encoding from gb2312 to utf8 for the language model
                // and later saving to the history json file.
                userInput = ConvertEncoding(userInput, Encoding.GetEncoding("gb2312"), Encoding.UTF8);

                if (userInput.ToLower().Equals("save"))
                {
                    session.SaveSession("chat-with-DeepSeek-chinese");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Session saved.");
                }
                else if (userInput.ToLower().Equals("regenerate"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Regenerating last response ...");

                    await foreach (
                        string text
                        in session.RegenerateAssistantMessageAsync(
                            inferenceParams))
                    {
                        Console.ForegroundColor = (ConsoleColor)11;

                        // Convert the encoding from utf8 to gb2312 for the console output.
                        Console.Write(ConvertEncoding(text, Encoding.UTF8, Encoding.GetEncoding("gb2312")));
                    }
                }
                else
                {
                    await foreach (
                        string text
                        in session.ChatAsync(
                            new ChatHistory.Message(AuthorRole.User, userInput),
                            inferenceParams))
                    {
                        Console.ForegroundColor = (ConsoleColor)11;
                        Console.Write(text);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                userInput = Console.ReadLine() ?? "";

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
