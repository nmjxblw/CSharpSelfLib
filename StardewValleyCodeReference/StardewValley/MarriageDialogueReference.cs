using System;
using System.Linq;
using Netcode;

namespace StardewValley;

public class MarriageDialogueReference : INetObject<NetFields>, IEquatable<MarriageDialogueReference>
{
	public const string ENDEARMENT_TOKEN = "%endearment";

	public const string ENDEARMENT_TOKEN_LOWER = "%endearmentlower";

	private readonly NetString _dialogueFile = new NetString("");

	private readonly NetString _dialogueKey = new NetString("");

	private readonly NetBool _isGendered = new NetBool(value: false);

	private readonly NetStringList _substitutions = new NetStringList();

	public NetFields NetFields { get; } = new NetFields("MarriageDialogueReference");

	public string DialogueFile => this._dialogueFile.Value;

	public string DialogueKey => this._dialogueKey.Value;

	public bool IsGendered => this._isGendered.Value;

	public string[] Substitutions => this._substitutions.ToArray();

	public MarriageDialogueReference()
	{
		this.NetFields.SetOwner(this).AddField(this._dialogueFile, "_dialogueFile").AddField(this._dialogueKey, "_dialogueKey")
			.AddField(this._isGendered, "_isGendered")
			.AddField(this._substitutions, "_substitutions");
	}

	public MarriageDialogueReference(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
		: this()
	{
		this._dialogueFile.Value = dialogue_file;
		this._dialogueKey.Value = dialogue_key;
		this._isGendered.Value = gendered;
		if (substitutions.Length != 0)
		{
			this._substitutions.AddRange(substitutions);
		}
	}

	public string GetText()
	{
		return "";
	}

	public bool IsItemGrabDialogue(NPC n)
	{
		return this.GetDialogue(n).isItemGrabDialogue();
	}

	/// <summary>Replace any tokens in the dialogue text with their localized variants.</summary>
	/// <param name="dialogue">The dialogue to modify.</param>
	/// <param name="npc">The NPC for which to replace tokens.</param>
	protected void _ReplaceTokens(Dialogue dialogue, NPC npc)
	{
		for (int i = 0; i < dialogue.dialogues.Count; i++)
		{
			dialogue.dialogues[i].Text = this._ReplaceTokens(dialogue.dialogues[i].Text, npc);
		}
	}

	/// <summary>Replace any tokens in the dialogue text with their localized variants.</summary>
	/// <param name="text">The dialogue text to modify.</param>
	/// <param name="npc">The NPC for which to replace tokens.</param>
	protected string _ReplaceTokens(string text, NPC npc)
	{
		text = text.Replace("%endearmentlower", npc.getTermOfSpousalEndearment().ToLower());
		text = text.Replace("%endearment", npc.getTermOfSpousalEndearment());
		return text;
	}

	public Dialogue GetDialogue(NPC n)
	{
		if (this._dialogueFile.Value.Contains("Marriage"))
		{
			Dialogue dialogue = n.tryToGetMarriageSpecificDialogue(this._dialogueKey.Value);
			if (dialogue == null)
			{
				Game1.log.Warn($"NPC '{n.Name}' couldn't get marriage dialogue key '{this._dialogueKey.Value}': not found.");
				dialogue = Dialogue.GetFallbackForError(n);
			}
			dialogue.removeOnNextMove = true;
			this._ReplaceTokens(dialogue, n);
			return dialogue;
		}
		string key = this._dialogueFile.Value + ":" + this._dialogueKey.Value;
		string rawText = (this._isGendered.Value ? Game1.LoadStringByGender(n.Gender, key, this._substitutions) : Game1.content.LoadString(key, this._substitutions));
		return new Dialogue(n, key, this._ReplaceTokens(rawText, n))
		{
			removeOnNextMove = true
		};
	}

	public bool Equals(MarriageDialogueReference other)
	{
		if (object.Equals(this._dialogueFile.Value, other._dialogueFile.Value) && object.Equals(this._dialogueKey.Value, other._dialogueKey.Value) && object.Equals(this._isGendered.Value, other._isGendered.Value))
		{
			return this._substitutions.SequenceEqual(other._substitutions);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is MarriageDialogueReference dialogue)
		{
			return this.Equals(dialogue);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hash = 13;
		hash = hash * 7 + ((this._dialogueFile.Value != null) ? this._dialogueFile.Value.GetHashCode() : 0);
		hash = hash * 7 + ((this._dialogueKey.Value != null) ? this._dialogueFile.Value.GetHashCode() : 0);
		hash = hash * 7 + ((!this._isGendered.Value) ? 1 : 0);
		foreach (string substitution in this._substitutions)
		{
			hash = hash * 7 + substitution.GetHashCode();
		}
		return hash;
	}
}
