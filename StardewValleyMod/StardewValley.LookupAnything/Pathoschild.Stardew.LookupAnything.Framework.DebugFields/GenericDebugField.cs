using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Pathoschild.Stardew.LookupAnything.Framework.DebugFields;

internal class GenericDebugField : IDebugField
{
	public string Label { get; protected set; }

	public string? Value { get; protected set; }

	[MemberNotNullWhen(true, "Value")]
	public bool HasValue
	{
		[MemberNotNullWhen(true, "Value")]
		get;
		[MemberNotNullWhen(true, "Value")]
		protected set;
	}

	public bool IsPinned { get; protected set; }

	public string? OverrideCategory { get; set; }

	public GenericDebugField(string label, string? value, bool? hasValue = null, bool pinned = false)
	{
		this.Label = label;
		this.Value = value;
		this.HasValue = hasValue ?? (!string.IsNullOrWhiteSpace(this.Value));
		this.IsPinned = pinned;
	}

	public GenericDebugField(string label, int value, bool? hasValue = null, bool pinned = false)
		: this(label, value.ToString(CultureInfo.InvariantCulture), hasValue, pinned)
	{
	}

	public GenericDebugField(string label, float value, bool? hasValue = null, bool pinned = false)
		: this(label, value.ToString(CultureInfo.InvariantCulture), hasValue, pinned)
	{
	}
}
