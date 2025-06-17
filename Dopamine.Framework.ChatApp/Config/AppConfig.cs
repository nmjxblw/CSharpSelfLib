using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows.Input;
namespace Dopamine.ChatApp
{
    /// <summary>
    /// 基于app.config文件生成的静态类
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// 默认配置节名称
        /// </summary>
        public static ConfigSection DefaultSection { get; set; } = ConfigSection.MainSection;
        /// <summary>
        /// 获取默认配置节的键值
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string GetValue(string keyName) => GetValue(DefaultSection, keyName);
        /// <summary>
        /// 获取特定配置节的键值
        /// </summary>
        /// <param name="configSection">配置节</param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string GetValue(ConfigSection configSection, string keyName) =>
            ((NameValueCollection)ConfigurationManager.GetSection(configSection.ToString()))[keyName];
    }

    public enum ConfigSection
    {
        MainSection,
        SecondSection
    }
}
