using System.Diagnostics.CodeAnalysis;

namespace Pathoschild.Stardew.LookupAnything.Framework.DebugFields;

internal interface IDebugField
{
	string Label { get; }

	string? Value { get; }

	[MemberNotNullWhen(true, "Value")]
	bool HasValue
	{
		[MemberNotNullWhen(true, "Value")]
		get;
	}

	bool IsPinned { get; }

	string? OverrideCategory { get; set; }
}
