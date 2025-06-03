using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class LinkField : GenericField
{
	private readonly Func<ISubject?> Subject;

	public override bool MayHaveLinks => true;

	public LinkField(string label, string text, Func<ISubject?> subject)
		: base(label, new FormattedText(text, Color.Blue))
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		this.Subject = subject;
	}

	public override bool TryGetLinkAt(int x, int y, [NotNullWhen(true)] out ISubject? subject)
	{
		if (base.TryGetLinkAt(x, y, out subject))
		{
			return true;
		}
		subject = this.Subject();
		return subject != null;
	}
}
