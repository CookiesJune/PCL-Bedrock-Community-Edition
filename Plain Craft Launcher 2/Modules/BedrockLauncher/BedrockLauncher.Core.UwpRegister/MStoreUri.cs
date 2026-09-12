using System;
using System.Runtime.InteropServices;

namespace BedrockLauncher.Core.UwpRegister;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MStoreUri
{
	public static Uri cookieUri = new Uri("https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx");

	public static Uri fileListXmlUri = new Uri("https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx");

	public static Uri updateUri = new Uri("https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured");

	public static Uri productUri = new Uri("https://storeedgefd.dsx.mp.microsoft.com/v9.0/products/9NBLGGH2JHXJ?market=US&locale=en-US&deviceFamily=Windows.Desktop");
}
