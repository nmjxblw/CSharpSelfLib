using LLama.Common;
using LLama;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Text;
using LLama.Native;

namespace DeepSeekApi
{
	public partial class LLamaApplication
	{
		/// <summary>
		/// Model的路径
		/// </summary>
		public static string ModelPath => Path.Combine(Directory.GetCurrentDirectory(), "Resources", ConfigManager.Data.Model[0]); // change it to your own model path.
		/// <summary>
		/// 运行LLamaSharp
		/// </summary>
		/// <returns></returns>
		public static async Task Run1()
		{
			Console.ForegroundColor = ConsoleColor.White;
			//NativeLibraryConfig.All.WithLogCallback(delegate (LLamaLogLevel level, string message) { Console.Write($"{level}：{message}"); });

			

			var parameters = new ModelParams(ModelPath)
			{
				ContextSize = 1024, // The longest length of chat as memory.
				GpuLayerCount = 5, // How many layers to offload to GPU. Please adjust it according to your GPU memory.
				Encoding = Encoding.UTF8,
			};
			using LLamaWeights model = LLamaWeights.LoadFromFile(parameters);
			using LLamaContext context = model.CreateContext(parameters);
			InteractiveExecutor executor = new InteractiveExecutor(context);

			// Add chat histories as prompt to tell AI how to act.
			ChatHistory chatHistory = new ChatHistory();
			chatHistory.AddMessage(AuthorRole.System, "Transcript of a dialog, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.");
			chatHistory.AddMessage(AuthorRole.User, "Hello, Bob.");
			chatHistory.AddMessage(AuthorRole.Assistant, "Hello. How may I help you today?");

			ChatSession session = new ChatSession(executor, chatHistory);

			InferenceParams inferenceParams = new InferenceParams()
			{
				MaxTokens = -1, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
				AntiPrompts = new List<string> { "User:" } // Stop generation once antiprompts appear.
			};

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("The chat session has started.\nUser: ");
			Console.ForegroundColor = ConsoleColor.Green;
			string userInput = Console.ReadLine() ?? "";

			while (true)
			{
				if (userInput.ToLower().Equals("exit") || userInput.ToLower().Equals("quit") || userInput.ToLower().Equals("q")) break;
				await foreach ( // Generate the response streamingly.
					string text
					in session.ChatAsync(
						new ChatHistory.Message(AuthorRole.User, userInput),
						inferenceParams))
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.Write(text);
				}
				Console.ForegroundColor = ConsoleColor.Green;
				userInput = Console.ReadLine() ?? "";
			}
		}
		public static string ExtractEmbeddedModel(string outputPath)
		{
			var assembly = Assembly.GetExecutingAssembly();
			const string resourceName = "MyApp.Assets.Models.model.gguf";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
					throw new FileNotFoundException($"资源 {resourceName} 未找到");

				// 确保目标目录存在
				var dir = Path.GetDirectoryName(outputPath);
				Directory.CreateDirectory(dir ?? throw new InvalidOperationException());

				using (var fileStream = new FileStream(outputPath, FileMode.Create))
				{
					stream.CopyTo(fileStream);
				}
			}
			return outputPath;
		}
	}
}