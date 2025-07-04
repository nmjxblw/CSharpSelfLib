﻿using Dopamine.ChatApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dopamine.Framework;
using System.Threading;
namespace Dopamine.ChatApp
{
    /// <summary>
    /// 记录器
    /// </summary>
    public static class Recorder
    {
        private static bool enable = false;
        private static readonly object LockObj = new object();
        private static readonly string MainRecordsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recorder", "MainRecords");
        private static readonly string ErrorRecordsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recorder", "ErrorRecords");
        private static readonly List<string> RecordPaths = new List<string>() { MainRecordsPath, ErrorRecordsPath };
        /// <summary>
        /// 功能启用标识符
        /// </summary>
        public static bool Enable { get => enable; set => enable = value; }

        /// <summary>
        /// 私有构造，在构造时自动注册记录报错
        /// </summary>
        static Recorder()
        {
            Enable = true;
            foreach (string filePath in RecordPaths)
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
            }
            // 记录未设置处理的错误
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                if (ex != null)
                {
                    RecordError(ex);
                    Trace.WriteLine(message: ex.StackTrace);
                }
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Exception ex = e.Exception;
                if (ex != null)
                {
                    RecordError(ex);
                    Trace.WriteLine(message: ex.StackTrace);
                }
            };
        }
        /// <summary>
        /// 写入
        /// </summary>
        public static void Write(string msg = default, string filePath = default, bool enable = true, ContextLevel contextLevel = ContextLevel.Public)
        {

            if (!enable || msg == default || contextLevel == ContextLevel.Private) return;
            if (filePath == default) filePath = RecordPaths[0];
            Trace.WriteLine(msg);


            lock (LockObj)
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                    Thread.Sleep(100); // 确保目录创建完成
                }
                string file = Path.Combine(filePath, $"{DateTime.Now:yyyy-MM-dd}.txt");
                string text = $"[{DateTime.Now:T}] {msg}{Environment.NewLine}";

                File.AppendAllText(file, text, Encoding.UTF8);
            }
        }
        /// <summary>
        /// 标准记录
        /// </summary>
        public static void Record(string msg = default)
        {
            try
            {
                if (msg == default) return;
                StackTrace stackTrace = new StackTrace(true);
                StackFrame stackFrame = stackTrace.GetFrame(1);
                string callingName = stackFrame?.GetMethod()?.DeclaringType?.FullName;
                Write(string.Format("[{0:50}]\t{1}", callingName, msg), RecordPaths[0], Enable, ContextLevel.Public);
            }
            catch (Exception ex)
            {
                ex.Message.ShowInTrace(true);
            }
        }
        /// <summary>
        /// 报错记录
        /// </summary>
        /// <param name="exception"></param>
        public static void RecordError(Exception exception = default,
            [CallerMemberName] string method = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (exception == default) return;
            Write($"{file}:{line}\t[{method}]\t{exception.Message}{Environment.NewLine}{exception.StackTrace}", RecordPaths[1], Enable, ContextLevel.Error);
        }
    }
    /// <summary>
    /// 记录器内容的等级
    /// </summary>
    public enum ContextLevel
    {
        /// <summary>
        /// 无设置
        /// </summary>
        None = 0,
        /// <summary>
        /// 公开级别
        /// </summary>
        Public,
        /// <summary>
        /// 非公开级别
        /// </summary>
        Private,
        /// <summary>
        /// 警告级别
        /// </summary>
        Alert,
        /// <summary>
        /// 报错级别
        /// </summary>
        Error,
        /// <summary>
        /// 调试级别
        /// </summary>
        Debug,
    }
}
