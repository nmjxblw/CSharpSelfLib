using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Framework
{
    public static class DynamicClassSerializer
    {
        /// <summary>
        /// 默认存储路径
        /// </summary>
        private static string DefaultSavePath =
            Path.Combine(Environment.CurrentDirectory, "DynamicClassData.json");

        /// <summary>
        /// 序列化并保存DynamicClass对象
        /// </summary>
        public static void Save(DynamicClass dynamicObj, string filePath = null)
        {
            var path = string.IsNullOrEmpty(filePath) ? DefaultSavePath : filePath;

            // 使用DynamicClassConverter进行序列化
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new DynamicClassConverter() }
            };

            string json = JsonConvert.SerializeObject(dynamicObj, Formatting.Indented, settings);

            // 原子写入操作（避免写入过程中断导致数据损坏）
            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tempPath, path);
        }

        /// <summary>
        /// 从文件加载并反序列化为DynamicClass对象
        /// </summary>
        public static DynamicClass Load(string filePath = null)
        {
            var path = string.IsNullOrEmpty(filePath) ? DefaultSavePath : filePath;

            if (!File.Exists(path))
                throw new FileNotFoundException("DynamicClass data file not found", path);

            string json = File.ReadAllText(path);

            // 使用DynamicClassConverter进行反序列化
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new DynamicClassConverter() }
            };

            return JsonConvert.DeserializeObject<DynamicClass>(json, settings);
        }

        /// <summary>
        /// 检查数据文件是否存在
        /// </summary>
        public static bool DataFileExists(string filePath = null)
        {
            var path = string.IsNullOrEmpty(filePath) ? DefaultSavePath : filePath;
            return File.Exists(path);
        }

        /// <summary>
        /// 删除数据文件
        /// </summary>
        public static void DeleteDataFile(string filePath = null)
        {
            var path = string.IsNullOrEmpty(filePath) ? DefaultSavePath : filePath;
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>
        /// 批量序列化多个DynamicClass对象到同一文件
        /// </summary>
        public static void SaveAll(IEnumerable<DynamicClass> objects, string filePath = null)
        {
            var path = string.IsNullOrEmpty(filePath) ? DefaultSavePath : filePath;
            var list = new List<object>();

            foreach (var obj in objects)
            {
                // 使用反射获取所有属性值[6](@ref)
                var dict = new Dictionary<string, object>();
                foreach (var prop in obj.GetDynamicMemberNames())
                {
                    dict[prop] = obj[prop];
                }
                list.Add(dict);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(list, Formatting.Indented));
        }
    }
}
