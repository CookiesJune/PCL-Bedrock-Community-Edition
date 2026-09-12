using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace BedrockLauncher.Core.Utils;

public static class MsiHelper
{
	public static int InstallMsiSilently(string msiFilePath, string additionalArgs = "")
	{
		try
		{
			if (!File.Exists(msiFilePath))
			{
				throw new IOException("Error: MSI file not found '" + msiFilePath + "'");
			}
			Process process = new Process();
			process.StartInfo.FileName = "msiexec.exe";
			string arguments = "/i \"" + msiFilePath + "\" /qn /norestart " + additionalArgs;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = false;
			process.Start();
			process.WaitForExit();
			return process.ExitCode;
		}
		catch (Exception)
		{
			throw;
		}
	}

	public static bool IsMsiProductInstalledByGuid(string productGuid)
	{
		string[] array = new string[2] { "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall" };
		using (RegistryKey registryKey = Registry.LocalMachine)
		{
			string[] array2 = array;
			foreach (string name in array2)
			{
				using RegistryKey registryKey2 = registryKey.OpenSubKey(name);
				if (registryKey2 == null)
				{
					continue;
				}
				string[] subKeyNames = registryKey2.GetSubKeyNames();
				foreach (string name2 in subKeyNames)
				{
					using RegistryKey registryKey3 = registryKey2.OpenSubKey(name2);
					string text = registryKey3?.GetValue("ProductCode") as string;
					if (!string.IsNullOrEmpty(text) && text.Equals(productGuid, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
					string text2 = registryKey3?.GetValue("UninstallString") as string;
					if (!string.IsNullOrEmpty(text2) && text2.Contains(productGuid, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
