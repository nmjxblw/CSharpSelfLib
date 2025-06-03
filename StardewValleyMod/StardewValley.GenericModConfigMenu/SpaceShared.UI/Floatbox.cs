namespace SpaceShared.UI;

internal class Floatbox : Textbox
{
	public float Value
	{
		get
		{
			if (!float.TryParse(this.String, out var value))
			{
				return 0f;
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
			float result;
			return float.TryParse(this.String, out result);
		}
	}

	protected override void ReceiveInput(string str)
	{
		bool hasDot = this.String.Contains('.');
		bool valid = true;
		for (int i = 0; i < str.Length; i++)
		{
			char c = str[i];
			if (!char.IsDigit(c) && (c != '.' || hasDot) && (c != '-' || !(this.String == "") || i != 0))
			{
				valid = false;
				break;
			}
			if (c == '.')
			{
				hasDot = true;
			}
		}
		if (valid)
		{
			this.String += str;
			base.Callback?.Invoke(this);
		}
	}
}
