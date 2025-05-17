using LLama.Common;
using LLama;
using System.Text;

namespace DeepSeekApi
{
	public partial class LLamaApplication
	{
		private static string ConvertEncoding(string input, Encoding original, Encoding target)
		{
			byte[] bytes = original.GetBytes(input);
			byte[] convertedBytes = Encoding.Convert(original, target, bytes);
			return target.GetString(convertedBytes);
		}

		public static async Task Run2()
		{
			// Register provider for GB2312 encoding
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("This example shows how to use Chinese with gb2312 encoding, which is common in windows. It's recommended" +
				" to use https://huggingface.co/hfl/chinese-alpaca-2-7b-gguf/blob/main/ggml-model-q5_0.gguf, which has been verified by LLamaSharp developers.");
			Console.ForegroundColor = ConsoleColor.White;

			var parameters = new ModelParams(ModelPath)
			{
				ContextSize = 1024,
				GpuLayerCount = 5,
				Encoding = Encoding.UTF8
			};
			using LLamaWeights model = LLamaWeights.LoadFromFile(parameters);
			using LLamaContext context = model.CreateContext(parameters);
			var executor = new InteractiveExecutor(context);

			ChatSession session;
			ChatHistory chatHistory = new ChatHistory();

			session = new ChatSession(executor, chatHistory);

			session
				.WithHistoryTransform(new LLamaTransforms.DefaultHistoryTransform());

			InferenceParams inferenceParams = new InferenceParams()
			{
				AntiPrompts = new List<string> { "用户：" }
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

				if (userInput == "save")
				{
					session.SaveSession("chat-with-DeepSeek-chinese");
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Session saved.");
				}
				else if (userInput == "regenerate")
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Regenerating last response ...");

					await foreach (
						string text
						in session.RegenerateAssistantMessageAsync(
							inferenceParams))
					{
						Console.ForegroundColor = ConsoleColor.White;

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
						Console.ForegroundColor = ConsoleColor.White;
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
