using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class GenericField : ICustomField
{
	protected readonly List<LinkTextArea> LinkTextAreas = new List<LinkTextArea>();

	protected readonly int IconMargin = 5;

	public string Label { get; protected set; }

	public virtual bool MayHaveLinks
	{
		get
		{
			if (this.ExpandLink == null)
			{
				return this.LinkTextAreas.Count != 0;
			}
			return true;
		}
	}

	public LinkField? ExpandLink { get; protected set; }

	public IFormattedText[]? Value { get; protected set; }

	public bool HasValue { get; protected set; }

	public GenericField(string label, string? value, bool? hasValue = null)
	{
		this.Label = label;
		this.Value = this.FormatValue(value);
		this.HasValue = hasValue ?? this.Value?.Any() ?? false;
	}

	public GenericField(string label, IFormattedText value, bool? hasValue = null)
		: this(label, new _003C_003Ez__ReadOnlySingleElementList<IFormattedText>(value), hasValue)
	{
	}

	public GenericField(string label, IEnumerable<IFormattedText> value, bool? hasValue = null)
	{
		this.Label = label;
		this.Value = value.ToArray();
		this.HasValue = hasValue ?? this.Value?.Any() ?? false;
	}

	public virtual Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		return null;
	}

	public virtual bool TryGetLinkAt(int x, int y, [NotNullWhen(true)] out ISubject? subject)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (this.ExpandLink != null)
		{
			this.ExpandLink = null;
			subject = null;
			return false;
		}
		foreach (LinkTextArea linkTextArea in this.LinkTextAreas)
		{
			Rectangle pixelArea = linkTextArea.PixelArea;
			if (((Rectangle)(ref pixelArea)).Contains(x, y))
			{
				subject = linkTextArea.Subject;
				return true;
			}
		}
		subject = null;
		return false;
	}

	public virtual void CollapseIfLengthExceeds(int minResultsForCollapse, int countForLabel)
	{
		IFormattedText[]? value = this.Value;
		if (value != null && value.Length >= minResultsForCollapse)
		{
			this.CollapseByDefault(I18n.Generic_ShowXResults(countForLabel));
		}
	}

	public void CollapseByDefault(string linkText)
	{
		this.ExpandLink = new LinkField(this.Label, linkText, () => (ISubject?)null);
	}

	protected GenericField(string label, bool hasValue = false)
		: this(label, (string?)null, (bool?)hasValue)
	{
	}

	protected IFormattedText[] FormatValue(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return Array.Empty<IFormattedText>();
		}
		return new IFormattedText[1]
		{
			new FormattedText(value)
		};
	}

	public static string? GetSaleValueString(int saleValue, int stackSize)
	{
		return GenericField.GetSaleValueString(new Dictionary<ItemQuality, int> { [ItemQuality.Normal] = saleValue }, stackSize);
	}

	public static string? GetSaleValueString(IDictionary<ItemQuality, int>? saleValues, int stackSize)
	{
		if (saleValues == null || !saleValues.Any() || saleValues.Values.All((int p) => p == 0))
		{
			return null;
		}
		if (saleValues.Count == 1)
		{
			string result = I18n.Generic_Price(saleValues.First().Value);
			if (stackSize > 1 && stackSize <= Constant.MaxStackSizeForPricing)
			{
				result = result + " (" + I18n.Generic_PriceForStack(saleValues.First().Value * stackSize, stackSize) + ")";
			}
			return result;
		}
		List<string> priceStrings = new List<string>();
		ItemQuality quality = ItemQuality.Normal;
		while (true)
		{
			if (saleValues.ContainsKey(quality))
			{
				priceStrings.Add((quality == ItemQuality.Normal) ? I18n.Generic_Price(saleValues[quality]) : I18n.Generic_PriceForQuality(saleValues[quality], I18n.For(quality)));
			}
			if (quality.GetNext() == quality)
			{
				break;
			}
			quality = quality.GetNext();
		}
		return I18n.List(priceStrings);
	}

	protected Vector2 DrawIconText(SpriteBatch batch, SpriteFont font, Vector2 position, float absoluteWrapWidth, string text, Color textColor, SpriteInfo? icon = null, Vector2? iconSize = null, Color? iconColor = null, int? qualityIcon = null, bool probe = false)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		int textOffset = 0;
		if (icon != null && iconSize.HasValue)
		{
			if (!probe)
			{
				batch.DrawSpriteWithin(icon, position.X, position.Y, iconSize.Value, iconColor);
			}
			textOffset = this.IconMargin;
		}
		else
		{
			iconSize = Vector2.Zero;
		}
		if (qualityIcon > 0 && iconSize.HasValue)
		{
			Vector2 valueOrDefault = iconSize.GetValueOrDefault();
			if (valueOrDefault.X > 0f && valueOrDefault.Y > 0f)
			{
				Rectangle qualityRect = ((qualityIcon < 4) ? new Rectangle(338 + (qualityIcon.Value - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8));
				Texture2D qualitySprite = Game1.mouseCursors;
				Vector2 qualitySize = iconSize.Value / 2f;
				Vector2 qualityPos = default(Vector2);
				((Vector2)(ref qualityPos))._002Ector(position.X + iconSize.Value.X - qualitySize.X, position.Y + iconSize.Value.Y - qualitySize.Y);
				batch.DrawSpriteWithin(qualitySprite, qualityRect, qualityPos.X, qualityPos.Y, qualitySize, iconColor);
			}
		}
		Vector2 textSize = (probe ? font.MeasureString(text) : batch.DrawTextBlock(font, text, position + new Vector2(iconSize.Value.X + (float)textOffset, 0f), absoluteWrapWidth - (float)textOffset, textColor));
		return new Vector2(iconSize.Value.X + textSize.X, Math.Max(iconSize.Value.Y, textSize.Y));
	}
}
