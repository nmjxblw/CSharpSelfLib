using LLama;
using LLama.Abstractions;
using LLama.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace DeepSeekApi
{
	/// <summary>
	/// LLamaSharp管理类
	/// </summary>
	public static class LLamaManager
	{
		/// <summary>
		/// 模型路径
		/// </summary>
		public static string ModelPath
		{
			get
			{
				string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", ConfigManager.Data.Model[0]);
				if (!File.Exists(path))
					throw new Exception($"模型路径不存在：{path}，请检查模型文件是否存在");
				return path;
			}
		}
		/// <summary>
		/// 模型参数
		/// </summary>
		public static ModelParams ModelParams { get; set; } = new ModelParams(ModelPath)
		{
			ContextSize = 1024,
			GpuLayerCount = 80,
			Encoding = Encoding.UTF8
		};
		/// <summary>
		/// 将模型权重加载到内存中
		/// </summary>
		private static LLamaWeights? Weights { get; set; }
		/// <summary>
		/// 创建模型上下文
		/// </summary>
		private static LLamaContext? Context { get; set; }
		/// <summary>
		/// 交互执行器
		/// </summary>
		private static InteractiveExecutor Executor { get; set; }
		/// <summary>
		/// 主对话类
		/// </summary>
		private static ChatSession? Session { get; set; }
		/// <summary>
		/// 对话历史
		/// </summary>
		public static ChatHistory ChatHistory { get; } = new ChatHistory();
		/// <summary>
		/// 接口参数
		/// </summary>
		public static InferenceParams InferenceParams { get; } = new InferenceParams()
		{
			MaxTokens = 256,
			AntiPrompts = new List<string> { "User:" }  // 需设置有效的终止标记
		};
		/// <summary>
		/// 最后一次用户输入
		/// </summary>
		private static string LastUserInput { get; set; } = string.Empty;
		/// <summary>
		/// 文本输出构建器
		/// </summary>
		public static StringBuilder StringBuilder { get; set; } = new StringBuilder();
		private static string _outputText = string.Empty;
		/// <summary>
		/// 输出文本
		/// </summary>
		public static string OutputText
		{
			get => _outputText;
			set
			{
				_outputText = value;
				// 触发文本输出事件
				OutputTextChangeEvent?.Invoke(value);
			}
		}
		/// <summary>
		/// 输出文本事件
		/// </summary>
		public static event Action<string>? OutputTextChangeEvent;
		/// <summary>
		/// 内部静态构造类
		/// </summary>
		static LLamaManager()
		{
			Console.ForegroundColor = ConsoleColor.White;
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			ProcessStartInfo psi = new ProcessStartInfo("powershell", "nvidia-smi --query-gpu=name,driver_version --format=csv")
			{
				RedirectStandardOutput = true,
				UseShellExecute = false
			};

			using Process? process = Process.Start(psi);
			if (process == null) throw new Exception("无法验证nvidia-smi识别状态");
			string output = process.StandardOutput.ReadToEnd();
			Console.WriteLine($"【验证nvidia-smi识别状态】\n{output}\n");
			Weights = LLamaWeights.LoadFromFile(ModelParams);
			Context = Weights.CreateContext(ModelParams);
			Executor = new InteractiveExecutor(Context);
			ChatHistory.AddMessage(AuthorRole.System, ConfigManager.Data.SystemRole);
			ChatHistory.AddMessage(AuthorRole.User, ConfigManager.Data.UserRole);
			ChatHistory.AddMessage(AuthorRole.Assistant, ConfigManager.Data.AssistantRole);
			Session = new ChatSession(
				Executor,
				ChatHistory);
			// 设置自定义历史变换式
			Session.WithHistoryTransform(new LLamaTransforms.DefaultHistoryTransform());
		}
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
		/// 异步生成回答
		/// </summary>
		/// <param name="userInput"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task AskAsync(string userInput)
		{
			if (string.IsNullOrEmpty(userInput)) return;
			if (Session == null) throw new Exception("会话未初始化");
			LastUserInput = userInput;
			StringBuilder.Clear();
			//ChatHistory.Messages.RemoveAll(m => m.AuthorRole == AuthorRole.User);
			// TODO:解决重复回答
			await foreach (
				string text
			in Session!.ChatAsync(
							new ChatHistory.Message(AuthorRole.User, LastUserInput),
							InferenceParams))
			{
				if (!InferenceParams.AntiPrompts.Contains(text))
					OutputText = text;
			}
		}
		/// <summary>
		/// 异步重新生成回答
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task RegenerateAsync()
		{
			if (string.IsNullOrEmpty(OutputText)) return;
			StringBuilder.Clear();
			await foreach (
						string text
						in Session!.RegenerateAssistantMessageAsync(InferenceParams))
			{
				OutputText = StringBuilder.Append(text).ToString();
			}
		}
		/// <summary>
		/// 保存会话
		/// </summary>
		public static void Save()
		{
			if (Session == null)
			{
				return;
			}
			Session!.SaveSession("chat-with-DeepSeek-chinese");
		}
	}
}
