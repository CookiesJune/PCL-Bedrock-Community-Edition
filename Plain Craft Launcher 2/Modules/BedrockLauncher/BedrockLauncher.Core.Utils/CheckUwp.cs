using System;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace BedrockLauncher.Core.Utils;

public static class CheckUwp
{
	public static bool IsUwpPackageInstalled(string packageFamilyName)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(packageFamilyName))
		{
			throw new ArgumentException("PackageFamilyName can't be empty", "packageFamilyName");
		}
		try
		{
			foreach (Package item in new PackageManager().FindPackagesForUser(string.Empty))
			{
				if (item.Id.FamilyName.Equals(packageFamilyName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			throw;
		}
	}
}
