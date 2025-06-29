// ZipTask.cs
using Microsoft.Build.Framework;
using System.IO.Compression;
using System.Reflection;

namespace Dopamine.MSBuild
{
    /// <summary>
    /// 打包Task
    /// </summary>
    public class ZipTask : Microsoft.Build.Utilities.Task
    {
        /// <summary>
        /// 从MSBuild接收的输出目录
        /// </summary>
        [Required]
        public string OutputPath { get; set; } = string.Empty;
        /// <summary>
        /// 目标文件
        /// </summary>
        [Required]
        public string Target { get; set; } = string.Empty;
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            if (!File.Exists(Path.Combine(OutputPath, Target)))
            {
                Log.LogError($"目标文件不存在");
                return false;
            }
            // 获取主程序集名称（不含扩展名）
            string? assemblyName = AssemblyName.GetAssemblyName(
                Path.Combine(OutputPath, Target) // 替换为实际程序集名
            ).Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                Log.LogError("无法获取程序集名称");
                return false;
            }

            string zipPath = Path.Combine(OutputPath, $"{assemblyName}.zip");
            string sourceDir = OutputPath; // 要压缩的目录

            try
            {
                // 删除旧压缩包（若存在）
                if (File.Exists(zipPath)) File.Delete(zipPath);

                // 创建新压缩包
                ZipFile.CreateFromDirectory(sourceDir, zipPath);
                Log.LogMessage(MessageImportance.High,
                    $"✅ 成功创建压缩包: {zipPath}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ 压缩失败: {ex.Message}");
                return false;
            }
        }
    }
}

#region --- .csproj代码实例 ---
//< Project Sdk = "Microsoft.NET.Sdk" >
//  ...
//  < !--注册自定义任务-- >
//  < UsingTask TaskName = "ZipTask"
//             AssemblyFile = "$(MSBuildThisFileDirectory)path/to/ZipTask.dll" />


//  < !--生成后执行打包-- >
//  <Target Name="ZipAfterBuild" AfterTargets="Build">
//  < ItemGroup >
//    < FrameworkDirs Include = "$(OutputPath)\**\*.dll" />
//  </ ItemGroup >
//  < ZipTask OutputPath = "%(FrameworkDirs.RootDir)%(FrameworkDirs.Directory)" />
//</ Target >
//</ Project >
#endregion

#region --- 在任务中添加压缩级别属性 ---
//public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;

//// 创建压缩包时使用：
//using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
//{
//    foreach (var file in files)
//    {
//        var entry = archive.CreateEntry(Path.GetFileName(file), CompressionLevel);
//        using (var stream = entry.Open())
//        using (var fileStream = File.OpenRead(file))
//        {
//            fileStream.CopyTo(stream);
//        }
//    }
//}
#endregion