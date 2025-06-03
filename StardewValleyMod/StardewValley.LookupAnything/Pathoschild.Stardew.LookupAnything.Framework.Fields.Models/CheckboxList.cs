using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

internal class CheckboxList
{
	internal record IntroData(string Text, SpriteInfo? Icon);

	public Checkbox[] Checkboxes;

	public IntroData? Intro;

	public bool IsHidden;

	public CheckboxList(Checkbox[] checkboxes, bool isHidden = false)
	{
		this.Checkboxes = checkboxes;
		this.IsHidden = isHidden;
	}

	public CheckboxList(IEnumerable<Checkbox> checkboxes, bool isHidden = false)
		: this(checkboxes.ToArray(), isHidden)
	{
	}

	public CheckboxList AddIntro(string text, SpriteInfo? icon = null)
	{
		this.Intro = new IntroData(text, icon);
		return this;
	}
}
