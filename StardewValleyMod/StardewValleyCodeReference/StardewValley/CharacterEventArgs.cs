using System;

namespace StardewValley;

public class CharacterEventArgs : EventArgs
{
	private readonly char character;

	private readonly int lParam;

	public char Character => this.character;

	public int Param => this.lParam;

	public int RepeatCount => this.lParam & 0xFFFF;

	public bool PreviousState => (this.lParam & 0x40000000) > 0;

	public bool TransitionState => (this.lParam & int.MinValue) > 0;

	public CharacterEventArgs(char character, int lParam)
	{
		this.character = character;
		this.lParam = lParam;
	}
}
