using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using ADOX;
using Microsoft.Win32;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Dopamine
{
	public partial class DopamineMainWindow : Window
	{
		private List<DatabaseInfo> databases { get; set; } = new List<DatabaseInfo>();

		public DopamineMainWindow()
		{
			InitializeComponent();
		}

		private void SelectDatabases_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			{
				Filter = "Access 2007数据库|*.mdb",
				Multiselect = true
			};

			if (dialog.ShowDialog() == true)
			{
				foreach (var path in dialog.FileNames)
				{
					if (!databases.Any(d => d.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
					{
						databases.Add(new DatabaseInfo { FilePath = path });
					}
				}
				RefreshDatabaseList();
			}
		}
		private async void MergeDatabases_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (databases.Count < 1)
				{
					MessageBox.Show("请至少选择一个数据库");
					return;
				}
				string pathString = FolderDialog.ShowDialog();
				if (string.IsNullOrEmpty(pathString)) return;
				string MergedDbPath = Path.Combine(pathString, "MergedData.mdb");
				// 启用不确定进度
				Dispatcher.Invoke(() =>
				{
					progressBar.IsIndeterminate = true;
					progressBar.Visibility = Visibility.Visible;
				});

				await MergeDatabasesAsync(databases.Select(d => d.FilePath).ToList(), MergedDbPath);
				if (MessageBox.Show("数据库合并完成！") == System.Windows.Forms.DialogResult.OK)
					System.Diagnostics.Process.Start("explorer.exe", $"{Path.GetDirectoryName(MergedDbPath)}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"合并失败: {ex.Message}");
			}
			finally
			{
				Dispatcher.Invoke(() =>
				{
					progressBar.IsIndeterminate = false;
					progressBar.Visibility = Visibility.Collapsed;
				});
			}
		}

		private async Task MergeDatabasesAsync(List<string> sourcePaths, string targetPath)
		{
			await Task.Run(() =>
			{
				// 创建目标数据库
				if (File.Exists(targetPath)) File.Delete(targetPath);
				CreateEmptyDatabase(targetPath);

				var firstDb = sourcePaths.First();
				var otherDbs = sourcePaths.Skip(1).ToList();
				// 复制首个数据库结构
				CopyDatabaseStructure(firstDb, targetPath);

				// 并行处理其他数据库
				Parallel.ForEach(otherDbs, dbPath =>
				{
					using (var targetConn = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={targetPath};"))
					using (var sourceConn = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};"))
					{
						targetConn.Open();
						sourceConn.Open();

						List<string> sourceTables = GetTableNames(sourceConn);
						List<string> targetTables = GetTableNames(targetConn);
						IEnumerable<string> tables = sourceTables.Intersect(targetTables);
						int tableCount = tables.Count();
						int currentCount = 0;
						foreach (var table in tables)
						{
							CopyTableData(sourceConn, targetConn, table);
							Dispatcher.Invoke(() =>
							{
								progressBar.Value = (++currentCount / (double)tableCount) * 100;
								txtStatus.Text = $"正在处理 {Path.GetFileName(dbPath)}...";
							});
						}
					}
				});
			});
		}

		private void CopyDatabaseStructure(string sourcePath, string targetPath)
		{
			using (var source = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={sourcePath};"))
			using (var target = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={targetPath};"))
			{
				source.Open();
				target.Open();

				var schema = source.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
					new object[] { null, null, null, "TABLE" });

				foreach (DataRow row in schema.Rows)
				{
					string tableName = row["TABLE_NAME"].ToString();
					if (!tableName.StartsWith("MSys"))
					{
						CloneTableStructure(source, target, tableName);
					}
				}
			}
		}

		private void CloneTableStructure(OleDbConnection source, OleDbConnection target, string tableName)
		{
			using (var cmd = new OleDbCommand($"SELECT * INTO [{tableName}] FROM [{tableName}] IN '' [;DATABASE={source.DataSource}]", target))
			{
				cmd.ExecuteNonQuery();
			}
		}

		private void CopyTableData(OleDbConnection source, OleDbConnection target, string tableName)
		{
			using (var selectCmd = new OleDbCommand($"SELECT * FROM [{tableName}]", source))
			using (var reader = selectCmd.ExecuteReader())
			{
				var targetColumns = GetTableColumns(target, tableName);
				var columnNames = targetColumns.Keys.ToArray();

				using (var insertCmd = CreateInsertCommand(target, tableName, targetColumns))
				{
					while (reader.Read())
					{
						try
						{
							for (int i = 0; i < columnNames.Length; i++)
							{
								var value = reader[columnNames[i]];
								insertCmd.Parameters[i].Value = value ?? DBNull.Value;
							}
							insertCmd.ExecuteNonQuery();
						}
						catch {/* 跳过字段不匹配的记录 */}
					}
				}
			}
		}

		// 创建空数据库文件
		private void CreateEmptyDatabase(string filePath)
		{
			if (File.Exists(filePath)) File.Delete(filePath);
			Catalog catalog = null;
			string provider = Path.GetExtension(filePath).ToLower() == ".accdb"
			   ? "Microsoft.ACE.OLEDB.12.0"
			   : "Microsoft.Jet.OLEDB.4.0";
			try
			{
				catalog = new Catalog();
				string connectionString = $"Provider={provider};Data Source={filePath};";
				catalog.Create(connectionString);
			}
			finally
			{
				// 显式释放 COM 对象
				if (catalog != null)
				{
					Marshal.ReleaseComObject(catalog);
					catalog = null;
				}
			}
			// 使用特殊连接字符串创建新数据库
			var connStr = $"Provider={provider};Data Source={filePath};";
			using (var conn = new OleDbConnection(connStr))
			{
				try
				{
					conn.Open();  // 通过打开连接创建新数据库
								  // 创建系统表（必需的基础结构）
					using (var cmd = new OleDbCommand("CREATE TABLE __CreationLog (ID INT, CreatedDate DATETIME)", conn))
					{
						cmd.ExecuteNonQuery();
						cmd.CommandText = "DROP TABLE __CreationLog";
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					throw new Exception($"数据库创建失败，请确认：\n1. 已安装Access引擎\n2. 有写文件权限\n3. 文件路径有效\n原始错误：{ex.Message}");
				}
			}
		}
		// 获取数据库中的用户表列表
		private List<string> GetTableNames(OleDbConnection connection)
		{
			var tables = new List<string>();
			try
			{
				var schema = connection.GetOleDbSchemaTable(
					OleDbSchemaGuid.Tables,
					new object[] { null, null, null, "TABLE" });

				foreach (DataRow row in schema.Rows)
				{
					var tableName = row["TABLE_NAME"].ToString();
					if (!tableName.StartsWith("MSys") && !tableName.StartsWith("~"))
					{
						tables.Add(tableName);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("获取表结构失败：" + ex.Message);
			}
			return tables;
		}

		// 获取表字段结构字典（字段名 -> 数据类型）
		private Dictionary<string, OleDbType> GetTableColumns(OleDbConnection connection, string tableName)
		{
			var columns = new Dictionary<string, OleDbType>();
			try
			{
				var schema = connection.GetOleDbSchemaTable(
					OleDbSchemaGuid.Columns,
					new object[] { null, null, tableName, null });

				foreach (DataRow row in schema.Rows)
				{
					var columnName = row["COLUMN_NAME"].ToString();
					var dataType = (int)row["DATA_TYPE"];

					// 处理特殊类型映射
					OleDbType oleType;

					switch ((OleDbType)dataType)
					{
						case OleDbType.BigInt:  // Access无BigInt
							oleType = OleDbType.Integer;
							break;
						case OleDbType.UnsignedTinyInt:
							oleType = OleDbType.TinyInt;
							break;
						default:
							oleType = (OleDbType)dataType;
							break;
					}

					columns[columnName] = oleType;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"获取[{tableName}]表结构失败：" + ex.Message);
			}
			return columns;
		}

		// 创建参数化插入命令
		private OleDbCommand CreateInsertCommand(OleDbConnection connection, string tableName, Dictionary<string, OleDbType> columns)
		{
			var fieldList = string.Join(", ", columns.Keys.Select(k => $"[{k}]"));
			var paramList = string.Join(", ", columns.Keys.Select(k => $"@{k}"));

			var cmd = new OleDbCommand
			{
				Connection = connection,
				CommandText = $"INSERT INTO [{tableName}] ({fieldList}) VALUES ({paramList})"
			};

			// 添加类型化参数
			foreach (var col in columns)
			{
				var param = new OleDbParameter($"@{col.Key}", col.Value)
				{
					SourceVersion = DataRowVersion.Current
				};
				cmd.Parameters.Add(param);
			}

			return cmd;
		}

		// 刷新界面数据库列表
		private void RefreshDatabaseList()
		{
			lstDatabases.ItemsSource = null;  // 强制刷新
			lstDatabases.ItemsSource = databases;
			lstDatabases.Items.Refresh();
		}

		// 移除数据库按钮事件
		private void RemoveDatabase_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.Tag is DatabaseInfo dbInfo)
			{
				databases.Remove(dbInfo);
				RefreshDatabaseList();
			}
		}
	}

	public class DatabaseInfo
	{
		public string FilePath { get; set; }
	}
	public class FolderDialog
	{
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

		private struct BROWSEINFO
		{
			public IntPtr hwndOwner;
			public IntPtr pidlRoot;
			public string pszDisplayName;
			public string lpszTitle;
			public uint ulFlags;
			public IntPtr lpfn;
			public int lParam;
			public int iImage;
		}

		public static string ShowDialog()
		{
			var bi = new BROWSEINFO
			{
				lpszTitle = "选择文件夹:",
				ulFlags = 0x00000041 // BIF_NEWDIALOGSTYLE | BIF_EDITBOX
			};

			IntPtr pidl = SHBrowseForFolder(ref bi);
			if (pidl != IntPtr.Zero)
			{
				IntPtr pathPtr = Marshal.AllocHGlobal(260);
				SHGetPathFromIDList(pidl, pathPtr);
				string path = Marshal.PtrToStringAuto(pathPtr);
				Marshal.FreeHGlobal(pathPtr);
				return path;
			}
			return null;
		}
	}
}