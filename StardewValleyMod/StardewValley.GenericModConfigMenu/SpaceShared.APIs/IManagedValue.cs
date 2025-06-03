using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SpaceShared.APIs;

public interface IManagedValue
{
	[MemberNotNullWhen(false, "ValidationError")]
	bool IsValid
	{
		[MemberNotNullWhen(false, "ValidationError")]
		get;
	}

	string? ValidationError { get; }

	bool IsReady { get; }

	string? Value { get; }

	IEnumerable<int> UpdateContext();
}
