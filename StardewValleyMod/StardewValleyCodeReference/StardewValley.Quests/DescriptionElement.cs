using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Monsters;
using StardewValley.SaveSerialization;

namespace StardewValley.Quests;

[XmlInclude(typeof(Item))]
[XmlInclude(typeof(Character))]
public class DescriptionElement : INetObject<NetFields>
{
	public static XmlSerializer serializer = SaveSerializer.GetSerializer(typeof(DescriptionElement));

	/// <summary>The translation key for the text to render.</summary>
	[XmlElement("xmlKey")]
	public string translationKey;

	/// <summary>The values to substitute for placeholders like <c>{0}</c> in the translation text.</summary>
	[XmlElement("param")]
	public List<object> substitutions;

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("DescriptionElement");

	/// <summary>Construct an instance for an empty text.</summary>
	public DescriptionElement()
		: this(string.Empty)
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="key">The translation key for the text to render.</param>
	/// <param name="substitutions">The values to substitute for placeholders like <c>{0}</c> in the translation text.</param>
	public DescriptionElement(string key, params object[] substitutions)
	{
		this.NetFields.SetOwner(this);
		this.translationKey = key;
		this.substitutions = new List<object>();
		this.substitutions.AddRange(substitutions);
	}

	public string loadDescriptionElement()
	{
		if (string.IsNullOrWhiteSpace(this.translationKey))
		{
			return string.Empty;
		}
		object[] substitutions = this.substitutions.ToArray();
		for (int i = 0; i < substitutions.Length; i++)
		{
			object obj = substitutions[i];
			if (!(obj is DescriptionElement element))
			{
				if (!(obj is Object obj2))
				{
					if (!(obj is Monster monster))
					{
						if (obj is NPC npc)
						{
							substitutions[i] = NPC.GetDisplayName(npc.name.Value);
						}
						continue;
					}
					DescriptionElement d;
					if (monster.name.Value == "Frost Jelly")
					{
						d = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13772");
						substitutions[i] = d.loadDescriptionElement();
					}
					else
					{
						d = new DescriptionElement("Data\\Monsters:" + monster.name.Value);
						substitutions[i] = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? (d.loadDescriptionElement().Split('/').Last() + "s") : d.loadDescriptionElement().Split('/').Last());
					}
					substitutions[i] = d.loadDescriptionElement().Split('/').Last();
				}
				else
				{
					substitutions[i] = ItemRegistry.GetDataOrErrorItem(obj2.QualifiedItemId).DisplayName;
				}
			}
			else
			{
				substitutions[i] = element.loadDescriptionElement();
			}
		}
		switch (substitutions.Length)
		{
		case 0:
			if (!this.translationKey.Contains("Dialogue.cs.7") && !this.translationKey.Contains("Dialogue.cs.8"))
			{
				return Game1.content.LoadString(this.translationKey);
			}
			return Game1.content.LoadString(this.translationKey).Replace("/", " ").TrimStart(' ');
		case 1:
			return Game1.content.LoadString(this.translationKey, substitutions[0]);
		case 2:
			return Game1.content.LoadString(this.translationKey, substitutions[0], substitutions[1]);
		case 3:
			return Game1.content.LoadString(this.translationKey, substitutions[0], substitutions[1], substitutions[2]);
		default:
			return Game1.content.LoadString(this.translationKey, substitutions);
		}
	}
}
