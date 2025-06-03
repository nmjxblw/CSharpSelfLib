using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SpaceShared.APIs;

public interface IManagedConditions
{
	[MemberNotNullWhen(false, "ValidationError")]
	bool IsValid
	{
		[MemberNotNullWhen(false, "ValidationError")]
		get;
	}

	string? ValidationError { get; }

	bool IsReady { get; }

	bool IsMatch { get; }

	bool IsMutable { get; }

	IEnumerable<int> UpdateContext();

	string? GetReasonNotMatched();
}
