using System.Collections;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Dopamine
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
		public override bool TryGetMember(GetMemberBinder binder, out object? result)
		{
			return _properties.TryGetValue(binder.Name, out result);
		}
		/// <summary>
		/// 动态属性设置
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool TrySetMember(SetMemberBinder binder, object? value = default)
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
		public object? this[string key]
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
		// 内部删除逻辑
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
	public class DynamicClassConverter : JsonConverter<DynamicClass>
	{
		/// <summary>
		/// 读取接口
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="typeToConvert"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public override DynamicClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var data = new DynamicClass();
			ReadObject(ref reader, data, options);
			return data;
		}
		/// <summary>
		/// 写入接口
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="data"></param>
		/// <param name="options"></param>
		/// <exception cref="JsonException"></exception>
		private void ReadObject(ref Utf8JsonReader reader, DynamicClass data, JsonSerializerOptions options)
		{
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					return;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString() ?? throw new JsonException("Property name is null");
					reader.Read();
					data[propertyName] = ReadValue(ref reader, options);
				}
			}
		}
		/// <summary>
		/// 读取值
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <exception cref="JsonException"></exception>
		private object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartObject:
					var nestedData = new DynamicClass();
					ReadObject(ref reader, nestedData, options);
					return nestedData;

				case JsonTokenType.StartArray:
					var list = new List<object>();
					while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
					{
						list.Add(ReadValue(ref reader, options) ?? "null");
					}
					return list;

				case JsonTokenType.String:
					return reader.GetString();

				case JsonTokenType.Number:
					if (reader.TryGetInt32(out int intValue)) return intValue;
					if (reader.TryGetInt64(out long longValue)) return longValue;
					return reader.GetDecimal();

				case JsonTokenType.True:
					return true;

				case JsonTokenType.False:
					return false;

				case JsonTokenType.Null:
					return null;

				default:
					throw new JsonException($"Unsupported token type: {reader.TokenType}");
			}
		}
		/// <summary>
		/// 写入动态类
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="options"></param>
		public override void Write(Utf8JsonWriter writer, DynamicClass value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			foreach (var kvp in value.Properties)
			{
				writer.WritePropertyName(kvp.Key);
				WriteValue(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
		/// <summary>
		/// 写入值
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="options"></param>
		private static void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case null:
					writer.WriteNullValue();
					break;

				case DynamicClass data:
					JsonSerializer.Serialize(writer, data, options);
					break;

				case IList list:
					writer.WriteStartArray();
					foreach (var item in list)
					{
						WriteValue(writer, item, options);
					}
					writer.WriteEndArray();
					break;

				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}

	}
}
