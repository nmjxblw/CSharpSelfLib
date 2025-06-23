using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Dopamine.StardewValley
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 获取类中属性名包含指定字符串的所有属性名
        /// </summary>
        /// <typeparam name="TClass">目标类类型</typeparam>
        /// <param name="searchString">要匹配的字符串（不区分大小写）</param>
        /// <returns>匹配的属性名集合</returns>
        public static List<string> GetPropertiesContainingString<TClass>(this string searchString) where TClass : class, new()
        {
            List<string> matchedProperties = new List<string>();
            if (string.IsNullOrEmpty(searchString))
                return matchedProperties;

            // 获取目标类型的所有公共实例属性
            PropertyInfo[] properties = typeof(TClass).GetProperties(
                BindingFlags.Public | BindingFlags.Instance
            );

            // 筛选属性名包含指定字符串的属性
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)) // 忽略大小写匹配
                {
                    matchedProperties.Add(prop.Name);
                }
            }

            return matchedProperties;
        }
        /// <summary>
        /// 递归查询类中属性名包含指定字符串的所有属性名
        /// </summary>
        /// <param name="obj">传入的类</param>
        /// <param name="searchString">要匹配的字符串（不区分大小写）</param>
        /// <returns></returns>
        public static List<string> GetPropertiesContainingString(this object obj, string searchString)
        {
            List<string> result = new List<string>();
            if (obj == null || string.IsNullOrEmpty(searchString)) return result;

            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                // 匹配当前层属性名
                if (prop.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(prop.Name);
                }

                // 递归处理嵌套对象属性（非值类型且非字符串）
                object? nestedValue = prop.GetValue(obj);
                if (nestedValue != null && !prop.PropertyType.IsValueType && prop.PropertyType != typeof(string))
                {
                    var nestedProps = GetPropertiesContainingString(nestedValue, searchString);
                    result.AddRange(nestedProps.Select(np => $"{prop.Name}.{np}"));
                }
            }
            return result;
        }
    }
}