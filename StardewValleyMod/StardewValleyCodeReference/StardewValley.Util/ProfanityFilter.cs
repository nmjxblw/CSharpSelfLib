using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StardewValley.Util;

internal class ProfanityFilter
{
	private readonly List<Regex> _words;

	private readonly StringBuilder _cleanup;

	public ProfanityFilter()
		: this("Content/profanity.regex")
	{
	}

	public ProfanityFilter(string profanityFile)
	{
		this._cleanup = new StringBuilder(2048);
		string[] profanity = File.ReadAllLines(profanityFile);
		this._words = new List<Regex>(profanity.Length);
		for (int i = 0; i < profanity.Length; i++)
		{
			Regex expr = new Regex(profanity[i], RegexOptions.IgnoreCase | RegexOptions.Compiled);
			this._words.Add(expr);
		}
	}

	public string Filter(string words)
	{
		if (string.IsNullOrWhiteSpace(words))
		{
			return words;
		}
		for (int i = 0; i < this._words.Count; i++)
		{
			MatchCollection matches = this._words[i].Matches(words);
			if (matches.Count == 0)
			{
				continue;
			}
			this._cleanup.Clear();
			this._cleanup.Append(words);
			for (int m = 0; m < matches.Count; m++)
			{
				Match match = matches[m];
				int end = match.Index + match.Length;
				for (int p = match.Index; p < end; p++)
				{
					if (!char.IsWhiteSpace(this._cleanup[p]))
					{
						this._cleanup[p] = '*';
					}
				}
			}
			words = this._cleanup.ToString();
		}
		return words;
	}
}
