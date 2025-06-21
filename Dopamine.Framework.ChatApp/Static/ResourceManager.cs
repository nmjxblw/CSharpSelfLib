using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.ChatApp
{
    /// <summary>
    /// 资源管理类
    /// </summary>
    public static class ResourceManager
    {
        static ResourceManager()
        {

        }
        /// <summary>
        /// 通过名称获取对应的资源
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static object GetResource(this string resourceName) => Application.Current.Resources[resourceName];
        /// <summary>
        /// 移除特定名字的资源
        /// </summary>
        /// <param name="resourceName"></param>
        public static void RemoveSource(this string resourceName)
        {
            for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; i++)
            {
                if (Application.Current.Resources.MergedDictionaries[i].Source.ToString().Contains(resourceName))
                {
                    Application.Current.Resources.MergedDictionaries.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
