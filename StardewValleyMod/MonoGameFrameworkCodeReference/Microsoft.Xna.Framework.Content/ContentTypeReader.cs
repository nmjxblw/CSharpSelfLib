using System;

namespace Microsoft.Xna.Framework.Content;

public abstract class ContentTypeReader
{
	private Type _targetType;

	public virtual bool CanDeserializeIntoExistingObject => false;

	public Type TargetType => this._targetType;

	public virtual int TypeVersion => 0;

	protected ContentTypeReader(Type targetType)
	{
		this._targetType = targetType;
	}

	protected internal virtual void Initialize(ContentTypeReaderManager manager)
	{
	}

	protected internal abstract object Read(ContentReader input, object existingInstance);
}
public abstract class ContentTypeReader<T> : ContentTypeReader
{
	protected ContentTypeReader()
		: base(typeof(T))
	{
	}

	protected internal override object Read(ContentReader input, object existingInstance)
	{
		if (existingInstance == null)
		{
			return this.Read(input, default(T));
		}
		return this.Read(input, (T)existingInstance);
	}

	protected internal abstract T Read(ContentReader input, T existingInstance);
}
