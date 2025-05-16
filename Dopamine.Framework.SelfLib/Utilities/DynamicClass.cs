using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System;
using Newtonsoft.Json.Linq;
namespace Dopamine.Framework
{
	/// <summary>
	/// 动态解析类
	/// </summary>
	[JsonConverter(typeof(DynamicClassConverter))]
	public class DynamicClass : DynamicObject
	{
		private readonly ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>();

		/// <summary>
		/// 动态属性访问
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return _properties.TryGetValue(binder.Name, out result);
		}
		/// <summary>
		/// 动态属性设置
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool TrySetMember(SetMemberBinder binder, object value = default)
		{
			if (value == default) return false;
			_properties[binder.Name] = value;
			return true;
		}

		/// <summary>
		/// 索引器访问
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object this[string key]
		{
			get
			{
				if (string.IsNullOrEmpty(key)) return default;
				return _properties.TryGetValue(key, out var value) ? value : default;
			}
			set
			{
				if (!string.IsNullOrEmpty(key))
					_properties[key] = value ?? "null";
			}
		}

		/// <summary>
		/// 获取所有属性
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> Properties => _properties;
		/// <summary>
		/// 返回所有键名
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _properties.Keys;
		}
		/// <summary>
		/// 重写 TryDeleteMember：删除单个成员（支持动态语法）
		/// </summary>
		/// <param name="binder"></param>
		/// <returns></returns>
		public override bool TryDeleteMember(DeleteMemberBinder binder)
		{
			string key = binder.Name;
			return TryRemoveMemberInternal(key);
		}
		/// <summary>
		/// 内部删除逻辑
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private bool TryRemoveMemberInternal(string key)
		{
			return _properties.TryRemove(key, out _);
		}
		/// <summary>
		/// 清空
		/// </summary>
		public void Clear()
		{
			_properties.Clear();
		}
		/// <summary>
		/// 批量删除成员
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public int Remove(params string[] keys)
		{
			if (keys == default) return 0;
			int count = 0;
			foreach (string key in keys)
			{
				if (string.IsNullOrEmpty(key)) continue;
				if (TryRemoveMemberInternal(key)) count++;
			}
			return count;
		}
	}
	/// <summary>
	/// 专属Json解析类
	/// </summary>
	public class DynamicClassConverter : JsonConverter
	{
		// 添加类型判断
		public override bool CanConvert(Type objectType) =>
			objectType == typeof(DynamicClass);

		// 反序列化逻辑
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var dynamicClass = new DynamicClass();
			JObject jo = JObject.Load(reader);

			foreach (var prop in jo.Properties())
			{
				dynamicClass[prop.Name] = ConvertToken(prop.Value);
			}

			return dynamicClass;
		}

		// 处理嵌套类型
		private object ConvertToken(JToken token)
		{
			switch (token.Type)
			{
				case JTokenType.Object:
					var nested = new DynamicClass();
					foreach (var prop in ((JObject)token).Properties())
					{
						nested[prop.Name] = ConvertToken(prop.Value);
					}
					return nested;

				case JTokenType.Array:
					var list = new List<object>();
					foreach (var item in (JArray)token)
					{
						list.Add(ConvertToken(item));
					}
					return list;

				default:
					return ((JValue)token).Value;
			}
		}

		// 序列化逻辑
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var dynamicClass = (DynamicClass)value;
			writer.WriteStartObject();

			foreach (var prop in dynamicClass.Properties)
			{
				writer.WritePropertyName(prop.Key);
				serializer.Serialize(writer, prop.Value);
			}

			writer.WriteEndObject();
		}
	}
}
