using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using UnityEditor.VersionControl;
namespace Dopamine
{
	/// <summary>
	/// Access管理器
	/// </summary>
	[Obsolete("功能不对",true)]
	public class AccessManager
	{
		static AccessManager()
		{
			Instance = new AccessManager();
		}
		private AccessManager() { }
		/// <summary>
		/// 单例实例化
		/// </summary>
		public static AccessManager Instance { get; }
		/// <summary>
		/// 数据库完整路径
		/// </summary>
		public string DataBaseFullNamePath { get; set; } = string.Empty;
		/// <summary>
		/// Provider字段
		/// </summary>
		public string Provider => "Microsoft.ACE.OLEDB.12.0";  
		/// <summary>
		/// 访问数据库字段
		/// </summary>
		public string ConnectionString => $"Provider={Instance.Provider};Data Source={Instance.DataBaseFullNamePath};";
		/// <summary>
		/// 获取表信息
		/// </summary>
		public async Task<DataTable> GetDataTable(string TableName)
		{
			await System.Threading.Tasks.Task.Yield();
			if (string.IsNullOrEmpty(TableName)) return new DataTable();
			DataTable dataTable = new DataTable();

			try
			{
				using (OleDbConnection connection = new OleDbConnection(Instance.ConnectionString))
				using (OleDbCommand command = new OleDbCommand($"SELECT * FROM {TableName}", connection))
				{
					connection.Open();
					using (OleDbDataReader reader = command.ExecuteReader(CommandBehavior.Default))
					{
						dataTable.Load(reader);
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine($"错误信息：{ex.Message}");
			}
			return dataTable;
		}
	}
}
