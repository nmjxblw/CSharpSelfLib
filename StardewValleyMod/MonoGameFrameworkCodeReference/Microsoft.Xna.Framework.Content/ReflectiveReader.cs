using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

internal class ReflectiveReader<T> : ContentTypeReader
{
	private delegate void ReadElement(ContentReader input, object parent);

	private List<ReadElement> _readers;

	private ConstructorInfo _constructor;

	private ContentTypeReader _baseTypeReader;

	public override bool CanDeserializeIntoExistingObject => base.TargetType.IsClass();

	public ReflectiveReader()
		: base(typeof(T))
	{
	}

	protected internal override void Initialize(ContentTypeReaderManager manager)
	{
		base.Initialize(manager);
		Type baseType = ReflectionHelpers.GetBaseType(base.TargetType);
		if (baseType != null && baseType != typeof(object))
		{
			this._baseTypeReader = manager.GetTypeReader(baseType);
		}
		this._constructor = base.TargetType.GetDefaultConstructor();
		PropertyInfo[] properties = base.TargetType.GetAllProperties();
		FieldInfo[] fields = base.TargetType.GetAllFields();
		this._readers = new List<ReadElement>(fields.Length + properties.Length);
		PropertyInfo[] array = properties;
		foreach (PropertyInfo property in array)
		{
			ReadElement read = ReflectiveReader<T>.GetElementReader(manager, property);
			if (read != null)
			{
				this._readers.Add(read);
			}
		}
		FieldInfo[] array2 = fields;
		foreach (FieldInfo field in array2)
		{
			ReadElement read2 = ReflectiveReader<T>.GetElementReader(manager, field);
			if (read2 != null)
			{
				this._readers.Add(read2);
			}
		}
	}

	private static ReadElement GetElementReader(ContentTypeReaderManager manager, MemberInfo member)
	{
		PropertyInfo property = member as PropertyInfo;
		FieldInfo field = member as FieldInfo;
		if (property != null)
		{
			if (!property.CanRead)
			{
				return null;
			}
			if (property.GetIndexParameters().Any())
			{
				return null;
			}
		}
		if (ReflectionHelpers.GetCustomAttribute<ContentSerializerIgnoreAttribute>(member) != null)
		{
			return null;
		}
		ContentSerializerAttribute contentSerializerAttribute = ReflectionHelpers.GetCustomAttribute<ContentSerializerAttribute>(member);
		if (contentSerializerAttribute == null)
		{
			if (property != null)
			{
				if (!ReflectionHelpers.PropertyIsPublic(property))
				{
					return null;
				}
				if (!property.CanWrite)
				{
					ContentTypeReader typeReader = manager.GetTypeReader(property.PropertyType);
					if (typeReader == null || !typeReader.CanDeserializeIntoExistingObject)
					{
						return null;
					}
				}
			}
			else
			{
				if (!field.IsPublic)
				{
					return null;
				}
				if (field.IsInitOnly)
				{
					return null;
				}
			}
		}
		Type elementType;
		Action<object, object> setter;
		if (property != null)
		{
			elementType = property.PropertyType;
			if (property.CanWrite)
			{
				setter = delegate(object o, object v)
				{
					property.SetValue(o, v, null);
				};
			}
			else
			{
				setter = delegate
				{
				};
			}
		}
		else
		{
			elementType = field.FieldType;
			setter = field.SetValue;
		}
		if (contentSerializerAttribute != null && contentSerializerAttribute.SharedResource)
		{
			return delegate(ContentReader input, object parent)
			{
				Action<object> fixup = delegate(object value)
				{
					setter(parent, value);
				};
				input.ReadSharedResource(fixup);
			};
		}
		ContentTypeReader reader = manager.GetTypeReader(elementType);
		if (reader == null)
		{
			if (!(elementType == typeof(Array)))
			{
				throw new ContentLoadException($"Content reader could not be found for {elementType.FullName} type.");
			}
			reader = new ArrayReader<Array>();
		}
		Func<object, object> construct = (object parent) => (object)null;
		if (property != null && !property.CanWrite)
		{
			construct = (object parent) => property.GetValue(parent, null);
		}
		return delegate(ContentReader input, object parent)
		{
			object existingInstance = construct(parent);
			object arg = input.ReadObject(reader, existingInstance);
			setter(parent, arg);
		};
	}

	protected internal override object Read(ContentReader input, object existingInstance)
	{
		T obj = ((existingInstance == null) ? ((this._constructor == null) ? ((T)Activator.CreateInstance(typeof(T))) : ((T)this._constructor.Invoke(null))) : ((T)existingInstance));
		if (this._baseTypeReader != null)
		{
			this._baseTypeReader.Read(input, obj);
		}
		object boxed = obj;
		foreach (ReadElement reader in this._readers)
		{
			reader(input, boxed);
		}
		obj = (T)boxed;
		return obj;
	}
}
