using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.ChatApp
{
    /// <summary>
    /// 短语助手类
    /// </summary>
    public static class WordHelper
    {
        /// <summary>
        /// 翻译成资源字典中对应id的短语
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Translate(this string id)
        {
            string text = id;
            try
            {
                text = Application.Current.FindResource(id) as string;
            }
            catch (Exception ex)
            {
                string errorMessage = $"翻译短语失败，id: {id}\r\n 错误信息: {ex.Message}";
                Recorder.RecordError(errorMessage);
            }
            return text;
        }
        /// <summary>
        /// 将id翻译成对应资源字典中的短语，并以指定格式插入对象
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Translate(this string id, params object[] args)
        {
            string text = id;
            try
            {
                text = Application.Current.FindResource(id) as string;
            }
            catch (Exception ex)
            {
                string errorMessage = $"翻译短语失败，id: {id}\r\n 错误信息: {ex.Message}";
                Recorder.RecordError(errorMessage);
            }
            return string.Format(text, args);
        }
    }
}
