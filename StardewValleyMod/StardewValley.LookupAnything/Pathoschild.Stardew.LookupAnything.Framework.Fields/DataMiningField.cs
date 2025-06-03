using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class DataMiningField : GenericField
{
	public DataMiningField(string label, IEnumerable<IDebugField>? fields)
		: base(label)
	{
		IDebugField[] fieldArray = fields?.ToArray() ?? Array.Empty<IDebugField>();
		base.HasValue = fieldArray.Any();
		if (base.HasValue)
		{
			base.Value = this.GetFormattedText(fieldArray).ToArray();
		}
	}

	private IEnumerable<IFormattedText> GetFormattedText(IDebugField[] fields)
	{
		int i = 0;
		for (int last = fields.Length - 1; i <= last; i++)
		{
			IDebugField field = fields[i];
			yield return new FormattedText("*", Color.Red, bold: true);
			yield return new FormattedText(field.Label + ":");
			yield return (i != last) ? new FormattedText(field.Value + Environment.NewLine) : new FormattedText(field.Value);
		}
	}
}
