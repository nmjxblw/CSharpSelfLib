namespace SpaceShared.UI;

internal class Intbox : Textbox
{
	public int Value
	{
		get
		{
			if (!int.TryParse(this.String, out var value))
			{
				return 0;
			}
			return value;
		}
		set
		{
			this.String = value.ToString();
		}
	}

	public bool IsValid
	{
		get
		{
			int result;
			return int.TryParse(this.String, out result);
		}
	}

	protected override void ReceiveInput(string str)
	{
		bool valid = true;
		for (int i = 0; i < str.Length; i++)
		{
			char c = str[i];
			if (!char.IsDigit(c) && (c != '-' || !(this.String == "") || i != 0))
			{
				valid = false;
				break;
			}
		}
		if (valid)
		{
			this.String += str;
			base.Callback?.Invoke(this);
		}
	}
}
