using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using BedrockLauncher.Core.BackGround;
using BedrockLauncher.Core.VersionJsons;

namespace BedrockLauncher.Core.Utils;

public static class ManifestEditor
{
	private const string SCCD_BASE64 = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPEN1c3RvbUNhcGFiaWxpdHlEZXNjcmlwdG9yIHhtbG5zPSJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL2FwcHgvMjAxOC9zY2NkIiB4bWxuczpzPSJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL2FwcHgvMjAxOC9zY2NkIj4KICA8Q3VzdG9tQ2FwYWJpbGl0aWVzPgogICAgPEN1c3RvbUNhcGFiaWxpdHkgTmFtZT0iTWljcm9zb2Z0LmNvcmVBcHBBY3RpdmF0aW9uXzh3ZWt5YjNkOGJid2UiPjwvQ3VzdG9tQ2FwYWJpbGl0eT4KICA8L0N1c3RvbUNhcGFiaWxpdGllcz4KICA8QXV0aG9yaXplZEVudGl0aWVzIEFsbG93QW55PSJ0cnVlIi8+CiAgPENhdGFsb2c+RkZGRjwvQ2F0YWxvZz4KPC9DdXN0b21DYXBhYmlsaXR5RGVzY3JpcHRvcj4=";

	public static async Task<bool> EditManifest(string directory, string gameName, BackGroundConfig? editer)
	{
		if (string.IsNullOrEmpty(directory))
		{
			throw new ArgumentNullException("directory");
		}
		string manifestPath = Path.Combine(directory, "AppxManifest.xml");
		if (!File.Exists(manifestPath))
		{
			return false;
		}
		try
		{
			return await Task.Run(delegate
			{
				XDocument xDocument = XDocument.Load(manifestPath, LoadOptions.PreserveWhitespace);
				XNamespace xNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
				XNamespace rescap = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities";
				XNamespace uap = "http://schemas.microsoft.com/appx/manifest/uap/windows10/4";
				XNamespace uap2 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/10";
				XNamespace xNamespace2 = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
				XNamespace xNamespace3 = "http://schemas.microsoft.com/appx/manifest/desktop/windows10/4";
				XElement root = xDocument.Root;
				if (root == null)
				{
					return false;
				}
				UpdateIgnorableNamespaces(root, xNamespace, rescap, uap, uap2);
				UpdateApplicationTrustLevel(root, xNamespace, uap2);
				UpdateCapabilities(root, xNamespace, rescap, uap);
				// 小teto定制：移除 Package 级 customInstall 扩展（GDK 包特征；loose 注册会因缺少 customInstallActions 能力而失败 0x80080204）
				XElement? pkgExt = root.Element(xNamespace + "Extensions");
				if (pkgExt != null)
				{
					pkgExt.Elements().Where(delegate(XElement e) { return (string?)e.Attribute("Category") == "windows.customInstall"; }).Remove();
					if (!pkgExt.HasElements)
					{
						pkgExt.Remove();
					}
				}
				XElement? obj2 = root.Element(xNamespace + "Applications")?.Element(xNamespace + "Application");
				XElement? obj3 = obj2?.Element(xNamespace + "Extensions");
				XElement? obj4 = root?.Element(xNamespace + "Identity");
				obj4.SetAttributeValue(value: VersionsHelper.GetNextVersion(new Version(obj4?.Attribute("Version")?.Value)), name: "Version");
				obj3.RemoveAll();
				obj2.SetAttributeValue(xNamespace3 + "SupportsMultipleInstances", "true");
				XElement xElement = obj2.Element(xNamespace2 + "VisualElements");
				if (!string.IsNullOrEmpty(gameName))
				{
					xElement.SetAttributeValue("DisplayName", gameName);
				}
				xElement.SetAttributeValue("AppListEntry", "none");
				if (editer.HasValue)
				{
					XElement xElement2 = xElement.Element(xNamespace2 + "SplashScreen");
					if (!string.IsNullOrEmpty(editer.Value.FileFullPath))
					{
						string fileName = Path.GetFileName(editer.Value.FileFullPath);
						File.Copy(editer.Value.FileFullPath, Path.Combine(directory, fileName));
						xElement2.SetAttributeValue("Image", fileName);
					}
					if (editer.Value.BackGroundColor.HasValue)
					{
						xElement2.SetAttributeValue("BackgroundColor", editer.Value.BackGroundColor.Value.ToHex(includeHash: true));
					}
				}
				xDocument.Save(manifestPath, SaveOptions.DisableFormatting);
				File.WriteAllBytes(Path.Combine(directory, "CustomCapability.SCCD"), Convert.FromBase64String("PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPEN1c3RvbUNhcGFiaWxpdHlEZXNjcmlwdG9yIHhtbG5zPSJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL2FwcHgvMjAxOC9zY2NkIiB4bWxuczpzPSJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL2FwcHgvMjAxOC9zY2NkIj4KICA8Q3VzdG9tQ2FwYWJpbGl0aWVzPgogICAgPEN1c3RvbUNhcGFiaWxpdHkgTmFtZT0iTWljcm9zb2Z0LmNvcmVBcHBBY3RpdmF0aW9uXzh3ZWt5YjNkOGJid2UiPjwvQ3VzdG9tQ2FwYWJpbGl0eT4KICA8L0N1c3RvbUNhcGFiaWxpdGllcz4KICA8QXV0aG9yaXplZEVudGl0aWVzIEFsbG93QW55PSJ0cnVlIi8+CiAgPENhdGFsb2c+RkZGRjwvQ2F0YWxvZz4KPC9DdXN0b21DYXBhYmlsaXR5RGVzY3JpcHRvcj4="));
				string text = File.ReadAllText(manifestPath);
				// 小teto定制：修复正则——RemoveAll()后Extensions变成<Extensions></Extensions>而非自闭合<Extensions/>，原正则匹配不到导致替换失败
				string value = Regex.Match(text, "<\\s*Extensions\\s*(?:/>|>\\s*</Extensions\\s*>)").Value;
				if (string.IsNullOrEmpty(value)) value = "<Extensions></Extensions>";
				string contents = text.Replace(value, " <Extensions>\r\n        <uap4:Extension Category=\"windows.loopbackAccessRules\">\r\n          <uap4:LoopbackAccessRules>\r\n            <uap4:Rule Direction=\"out\" PackageFamilyName=\"Microsoft.MEECC_8wekyb3d8bbwe\" />\r\n          </uap4:LoopbackAccessRules>\r\n        </uap4:Extension>\r\n        <uap:Extension Category=\"windows.fileTypeAssociation\" EntryPoint=\"App2\">\r\n          <uap:FileTypeAssociation Name=\"mcperf\">\r\n            <uap:DisplayName>MCPERF</uap:DisplayName>\r\n            <uap:InfoTip>Launch Minecraft and import world</uap:InfoTip>\r\n            <uap:SupportedFileTypes>\r\n              <uap:FileType>.MCPERF</uap:FileType>\r\n            </uap:SupportedFileTypes>\r\n          </uap:FileTypeAssociation>\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.fileTypeAssociation\" EntryPoint=\"App2\">\r\n          <uap:FileTypeAssociation Name=\"mcshortcut\">\r\n            <uap:DisplayName>MCSHORTCUT</uap:DisplayName>\r\n            <uap:InfoTip>Launch Minecraft and load world</uap:InfoTip>\r\n            <uap:SupportedFileTypes>\r\n              <uap:FileType>.MCSHORTCUT</uap:FileType>\r\n            </uap:SupportedFileTypes>\r\n          </uap:FileTypeAssociation>\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.fileTypeAssociation\" EntryPoint=\"App2\">\r\n          <uap:FileTypeAssociation Name=\"mcpack\">\r\n            <uap:DisplayName>MCPACK</uap:DisplayName>\r\n            <uap:InfoTip>Launch Minecraft and import resource pack</uap:InfoTip>\r\n            <uap:SupportedFileTypes>\r\n              <uap:FileType>.MCPACK</uap:FileType>\r\n            </uap:SupportedFileTypes>\r\n          </uap:FileTypeAssociation>\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.fileTypeAssociation\" EntryPoint=\"App2\">\r\n          <uap:FileTypeAssociation Name=\"mcworld\">\r\n            <uap:DisplayName>MCWORLD</uap:DisplayName>\r\n            <uap:InfoTip>Launch Minecraft and import world</uap:InfoTip>\r\n            <uap:SupportedFileTypes>\r\n              <uap:FileType>.MCWORLD</uap:FileType>\r\n            </uap:SupportedFileTypes>\r\n          </uap:FileTypeAssociation>\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.fileTypeAssociation\" EntryPoint=\"App2\">\r\n          <uap:FileTypeAssociation Name=\"mcproject\">\r\n            <uap:DisplayName>MCPROJECT</uap:DisplayName>\r\n            <uap:InfoTip>Launch Minecraft and import project</uap:InfoTip>\r\n            <uap:SupportedFileTypes>\r\n              <uap:FileType>.MCPROJECT</uap:FileType>\r\n            </uap:SupportedFileTypes>\r\n          </uap:FileTypeAssociation>\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.fileTypeAssociation\" EntryPoint=\"App2\">\r\n          <uap:FileTypeAssociation Name=\"mceditoraddon\">\r\n            <uap:DisplayName>MCEDITORADDON</uap:DisplayName>\r\n            <uap:InfoTip>Launch Minecraft and import editor addon</uap:InfoTip>\r\n            <uap:SupportedFileTypes>\r\n              <uap:FileType>.MCEDITORADDON</uap:FileType>\r\n            </uap:SupportedFileTypes>\r\n          </uap:FileTypeAssociation>\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.protocol\">\r\n          <uap:Protocol Name=\"ms-xbl-multiplayer\" />\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.protocol\">\r\n          <uap:Protocol Name=\"minecraft\" />\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.fileTypeAssociation\" EntryPoint=\"App2\">\r\n          <uap:FileTypeAssociation Name=\"mcaddon\">\r\n            <uap:DisplayName>MCADDON</uap:DisplayName>\r\n            <uap:InfoTip>Launch Minecraft and import addon</uap:InfoTip>\r\n            <uap:SupportedFileTypes>\r\n              <uap:FileType>.MCADDON</uap:FileType>\r\n            </uap:SupportedFileTypes>\r\n          </uap:FileTypeAssociation>\r\n        </uap:Extension>\r\n        <uap:Extension Category=\"windows.fileTypeAssociation\" EntryPoint=\"App2\">\r\n          <uap:FileTypeAssociation Name=\"mctemplate\">\r\n            <uap:DisplayName>MCTEMPLATE</uap:DisplayName>\r\n            <uap:InfoTip>Launch Minecraft and import world template</uap:InfoTip>\r\n            <uap:SupportedFileTypes>\r\n              <uap:FileType>.MCTEMPLATE</uap:FileType>\r\n            </uap:SupportedFileTypes>\r\n          </uap:FileTypeAssociation>\r\n        </uap:Extension>\r\n      </Extensions>");
				File.WriteAllText(manifestPath, contents);
				return true;
			});
		}
		catch
		{
			throw;
		}
	}

	private static void UpdateIgnorableNamespaces(XElement package, XNamespace ns, XNamespace rescap, XNamespace uap4, XNamespace uap10)
	{
		XNamespace xNamespace = "http://schemas.microsoft.com/appx/manifest/desktop/windows10/4";
		XAttribute xAttribute = package.Attribute("IgnorableNamespaces");
		string[] array = new string[5] { "uap", "uap4", "uap10", "rescap", "desktop4" };
		if (xAttribute == null)
		{
			package.SetAttributeValue("IgnorableNamespaces", string.Join(" ", array));
		}
		else
		{
			string[] array2 = xAttribute.Value.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			IEnumerable<string> enumerable = array.Except(array2);
			if (enumerable.Any())
			{
				xAttribute.Value = string.Join(" ", array2.Concat(enumerable));
			}
		}
		package.SetAttributeValue(XNamespace.Xmlns + "desktop4", xNamespace.NamespaceName);
		package.SetAttributeValue(XNamespace.Xmlns + "rescap", rescap.NamespaceName);
		package.SetAttributeValue(XNamespace.Xmlns + "uap4", uap4.NamespaceName);
		package.SetAttributeValue(XNamespace.Xmlns + "uap10", uap10.NamespaceName);
	}

	private static void UpdateApplicationTrustLevel(XElement package, XNamespace ns, XNamespace uap10)
	{
		(package.Element(ns + "Applications")?.Element(ns + "Application"))?.SetAttributeValue(uap10 + "TrustLevel", "mediumIL");
	}

	private static void UpdateCapabilities(XElement package, XNamespace ns, XNamespace rescap, XNamespace uap4)
	{
		XElement xElement = package.Element(ns + "Capabilities");
		if (xElement != null)
		{
			// 小teto定制：移除会重复/不再需要的能力（保留 appLicensing 游戏许可 + internetClient）
			xElement.Elements(rescap + "Capability").Where(delegate(XElement e)
			{
				string n = (string)e.Attribute("Name");
				return n == "customInstallActions" || n == "runFullTrust" || n == "unvirtualizedResources";
			}).Remove();
			xElement.Elements(uap4 + "CustomCapability").Remove();
			List<XElement> list = xElement.Elements(ns + "DeviceCapability").ToList();
			list.ForEach(delegate(XElement c)
			{
				c.Remove();
			});
			xElement.Add(new XElement(rescap + "Capability", new XAttribute("Name", "runFullTrust")), new XElement(rescap + "Capability", new XAttribute("Name", "unvirtualizedResources")), new XElement(uap4 + "CustomCapability", new XAttribute("Name", "Microsoft.coreAppActivation_8wekyb3d8bbwe")));
			if (list.Count > 0)
			{
				list.ForEach(xElement.Add);
			}
			else
			{
				xElement.Add(new XElement(ns + "DeviceCapability", new XAttribute("Name", "internetClient")));
			}
		}
	}
}
