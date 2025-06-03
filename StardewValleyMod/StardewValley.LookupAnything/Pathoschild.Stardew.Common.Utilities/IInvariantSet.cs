using System.Collections;
using System.Collections.Generic;

namespace Pathoschild.Stardew.Common.Utilities;

internal interface IInvariantSet : IReadOnlySet<string>, IEnumerable<string>, IEnumerable, IReadOnlyCollection<string>
{
	IInvariantSet GetWith(string other);

	IInvariantSet GetWith(ICollection<string> other);

	IInvariantSet GetWithout(string other);

	IInvariantSet GetWithout(IEnumerable<string> other);
}
