using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards;

public class MailReward : OrderReward
{
	public NetBool noLetter = new NetBool(value: true);

	public NetStringList grantedMails = new NetStringList();

	public NetBool host = new NetBool(value: false);

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(this.noLetter, "noLetter").AddField(this.grantedMails, "grantedMails").AddField(this.host, "host");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		string raw = order.Parse(data["MailReceived"]);
		this.grantedMails.AddRange(ArgUtility.SplitBySpace(raw));
		if (data.TryGetValue("NoLetter", out var rawValue))
		{
			this.noLetter.Value = Convert.ToBoolean(order.Parse(rawValue));
		}
		if (data.TryGetValue("Host", out rawValue))
		{
			this.host.Value = Convert.ToBoolean(order.Parse(rawValue));
		}
	}

	public override void Grant()
	{
		foreach (string mail in this.grantedMails)
		{
			if (this.host.Value)
			{
				if (!Game1.IsMasterGame)
				{
					continue;
				}
				if (Game1.newDaySync.hasInstance())
				{
					Game1.addMail(mail, this.noLetter.Value, sendToEveryone: true);
					continue;
				}
				string actualMail = mail;
				if (actualMail == "ClintReward" && Game1.player.mailReceived.Contains("ClintReward"))
				{
					Game1.player.mailReceived.Remove("ClintReward2");
					actualMail = "ClintReward2";
				}
				Game1.addMailForTomorrow(actualMail, this.noLetter.Value, sendToEveryone: true);
			}
			else if (Game1.newDaySync.hasInstance())
			{
				Game1.addMail(mail, this.noLetter.Value, sendToEveryone: true);
			}
			else
			{
				string actualMail2 = mail;
				if (actualMail2 == "ClintReward" && Game1.player.mailReceived.Contains("ClintReward"))
				{
					Game1.player.mailReceived.Remove("ClintReward2");
					actualMail2 = "ClintReward2";
				}
				Game1.addMailForTomorrow(actualMail2, this.noLetter.Value, sendToEveryone: true);
			}
		}
	}
}
