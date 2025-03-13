using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltageInsulationTest
{
	public partial class MainViewModel : ViewModelBase
	{
		/// <summary>
		/// 耐压仪的端口号
		/// </summary>
		public string InstrumentCOMPort
		{
			get => GetProperty("端口号");
			set
			{
				if (int.TryParse(value, out var temp))
				{
					value = $"COM{temp}";
				}
				SetProperty(value);
			}
		}
		/// <summary>
		/// 耐压板的端口号
		/// </summary>
		public string PlateCOMPort
		{
			get => GetProperty("端口号");
			set
			{
				if (int.TryParse(value, out var temp))
				{
					value = $"COM{temp}";
				}
				SetProperty(value);
			}
		}
	}
}
