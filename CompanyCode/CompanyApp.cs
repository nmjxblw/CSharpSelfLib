namespace CompanyCode
{
	/// <summary>
	/// 主程序
	/// </summary>
	public sealed class CompanyApp
	{
		public void Start()
		{
			//byte[] frame = [ 0x00, 0x10, 0x00];
			//frame.ToFitNumber().ToString().ShowInConsole(true);
			//frame.ToFitNumber1().ToString().ShowInConsole(true);
			byte[] frame = "68 01 FE 0A 13 60 01 1B 01".FromHexString();
			frame.GetXor().ToString("X2").ShowInConsole(true);
			BitConverter.ToString(frame.FillBBCAtLast()).Replace("-", " ").ShowInConsole(true);
		}
	}
}
