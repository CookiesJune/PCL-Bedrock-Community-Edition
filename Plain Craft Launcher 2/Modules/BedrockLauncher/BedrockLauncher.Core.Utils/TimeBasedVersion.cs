using System;
using System.Runtime.InteropServices;

namespace BedrockLauncher.Core.Utils;

public static class TimeBasedVersion
{
	public static string GetVersion()
	{
		DateTime now = DateTime.Now;
		int year = now.Year;
		int month = now.Month;
		int day = now.Day;
		int value = now.Hour * 60 + now.Minute;
		return $"{year}.{month}.{day}.{value}";
	}

	public static Version GetVersionObject()
	{
		DateTime now = DateTime.Now;
		int year = now.Year;
		int month = now.Month;
		int day = now.Day;
		int revision = now.Hour * 60 + now.Minute;
		return new Version(year, month, day, revision);
	}

	[UnmanagedCallersOnly(EntryPoint = "Add")]
	public static void GetVersionString()
	{
	}
}
