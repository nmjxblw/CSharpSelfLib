using System;
using System.Reflection;

namespace MonoGame.Framework.Utilities;

internal static class AssemblyHelper
{
	public static string GetDefaultWindowTitle()
	{
		string windowTitle = string.Empty;
		Assembly assembly = Assembly.GetEntryAssembly();
		if (assembly != null)
		{
			try
			{
				AssemblyTitleAttribute assemblyTitleAtt = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
				if (assemblyTitleAtt != null)
				{
					windowTitle = assemblyTitleAtt.Title;
				}
			}
			catch
			{
			}
			if (string.IsNullOrEmpty(windowTitle))
			{
				windowTitle = assembly.GetName().Name;
			}
		}
		return windowTitle;
	}
}
