using System;

namespace ContentManifest.Internal;

internal class CHValue : CHParsable
{
	public CHValueUnion RawValue;

	public CHValueEnum ValueType = CHValueEnum.Unknown;

	public CHValue()
	{
		this.RawValue.ValueNull = null;
	}

	public void Parse(CHJsonParserContext context)
	{
		if (context.ReadHead >= context.JsonText.Length)
		{
			throw new InvalidOperationException();
		}
		CHParsable parsable = null;
		char prefixChar = context.JsonText[context.ReadHead];
		switch (prefixChar)
		{
		case '{':
			parsable = (this.RawValue.ValueObject = new CHObject());
			this.ValueType = CHValueEnum.Object;
			break;
		case '[':
			parsable = (this.RawValue.ValueArray = new CHArray());
			this.ValueType = CHValueEnum.Array;
			break;
		case '"':
			parsable = (this.RawValue.ValueString = new CHString());
			this.ValueType = CHValueEnum.String;
			break;
		case 'f':
		case 't':
			parsable = (this.RawValue.ValueBoolean = new CHBoolean());
			this.ValueType = CHValueEnum.Boolean;
			break;
		case 'n':
			if (context.ReadHead + 3 >= context.JsonText.Length)
			{
				throw new InvalidOperationException();
			}
			if (context.JsonText[context.ReadHead + 1] != 'u' || context.JsonText[context.ReadHead + 2] != 'l' || context.JsonText[context.ReadHead + 3] != 'l')
			{
				throw new InvalidOperationException();
			}
			parsable = null;
			this.ValueType = CHValueEnum.Null;
			break;
		default:
			if (CHNumber.IsValidPrefix(prefixChar))
			{
				parsable = (this.RawValue.ValueNumber = new CHNumber());
				this.ValueType = CHValueEnum.Number;
				break;
			}
			throw new InvalidOperationException();
		}
		parsable?.Parse(context);
	}

	public object GetManagedObject()
	{
		return this.ValueType switch
		{
			CHValueEnum.Object => this.RawValue.ValueObject.Members, 
			CHValueEnum.Array => this.RawValue.ValueArray.Elements, 
			CHValueEnum.String => this.RawValue.ValueString.RawString, 
			CHValueEnum.Number => this.RawValue.ValueNumber.RawDouble, 
			CHValueEnum.Boolean => this.RawValue.ValueBoolean.RawBoolean, 
			CHValueEnum.Null => null, 
			_ => throw new InvalidOperationException(), 
		};
	}
}
