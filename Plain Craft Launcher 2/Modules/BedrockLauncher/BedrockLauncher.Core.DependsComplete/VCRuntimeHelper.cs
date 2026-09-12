using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BedrockLauncher.Core.Utils;
using BedrockLauncher.Core.UwpRegister;
using Windows.Management.Deployment;

namespace BedrockLauncher.Core.DependsComplete;

public static class VCRuntimeHelper
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct VCUri
	{
		public static string Uwpx64 = "https://raw.gitcode.com/gcw_lJgzYtGB/RecycleObjects/blobs/3112f116e0cebdf5b1ead2da347f516406e2a365/Microsoft.VCLibs.140.00_14.0.33519.0_x64__8wekyb3d8bbwe.Appx";

		public static string Uwpx86 = "https://raw.gitcode.com/gcw_lJgzYtGB/RecycleObjects/blobs/f1cead0f80316261fd170c8f54f6cca99f4eaf22/Microsoft.VCLibs.140.00_14.0.33519.0_x86__8wekyb3d8bbwe.Appx";

		public static string Uwparm = "https://raw.gitcode.com/gcw_lJgzYtGB/RecycleObjects/blobs/106072935eb8232132813cec6c98b979544f69d6/Microsoft.VCLibs.140.00_14.0.33519.0_arm__8wekyb3d8bbwe.Appx";

		public static string Uwparm64 = "https://raw.gitcode.com/gcw_lJgzYtGB/RecycleObjects/blobs/90f5bd2c05a92f1ed5b60e1a5cc69be1627cff13/Microsoft.VCLibs.140.00_14.0.33519.0_arm64__8wekyb3d8bbwe.Appx";

		public static string Win32x64 = "https://gitcode.com/gcw_lJgzYtGB/RecycleObjects/releases/download/VCRuntime140GDK/VC_redist.x64.exe";

		public static string Win32x86 = "https://gitcode.com/gcw_lJgzYtGB/RecycleObjects/releases/download/VCRuntime140GDK/VC_redist.x86.exe";

		public static string Win32arm64 = "https://gitcode.com/gcw_lJgzYtGB/RecycleObjects/releases/download/VCRuntime140GDK/VC_redist.arm64.exe";

		public static string GameInputRedist = "https://raw.gitcode.com/gcw_lJgzYtGB/RecycleObjects/blobs/babbbbf96d352658f85ff0287e64bcd485b5f001/GameInputRedist.msi";
	}

	public static async Task CompleteVCRuntimeAsync([NotNull] Architecture architecture)
	{
		HttpClient client = new HttpClient();
		try
		{
			_ = 2;
			try
			{
				byte[] uwpVC = await DownloadPackageAsync(architecture switch
				{
					Architecture.X86 => VCUri.Uwpx86, 
					Architecture.X64 => VCUri.Uwpx64, 
					Architecture.Arm64 => VCUri.Uwparm64, 
					_ => VCUri.Uwpx64, 
				});
				byte[] bytes = await DownloadPackageAsync(architecture switch
				{
					Architecture.X64 => VCUri.Win32x64, 
					Architecture.X86 => VCUri.Win32x86, 
					Architecture.Arm64 => VCUri.Uwparm64, 
					_ => VCUri.Win32x64, 
				});
				string text = Path.GetTempFileName() + ".appx";
				string tempgdk = Path.GetTempFileName() + ".exe";
				File.WriteAllBytes(text, uwpVC);
				File.WriteAllBytes(tempgdk, bytes);
				await BedrockLauncher.Core.UwpRegister.UwpRegister.AddAppxAsync(new DeploymentOptionsConfig
				{
					CancellationToken = new CancellationToken(canceled: false),
					DeploymentOptions = (DeploymentOptions)1,
					PackagePath = text
				});
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = tempgdk,
					Arguments = "/install /quiet",
					UseShellExecute = false,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden
				};
				using Process process = new Process
				{
					StartInfo = startInfo
				};
				process.Start();
			}
			catch
			{
				throw new BedrockCoreException("Get VCPackage Error");
			}
		}
		finally
		{
			if (client != null)
			{
				((IDisposable)client).Dispose();
			}
		}
		async Task<byte[]> DownloadPackageAsync(string uri)
		{
			HttpResponseMessage obj2 = await client.GetAsync(uri);
			if (obj2.StatusCode != HttpStatusCode.OK)
			{
				throw new BedrockCoreNetWorkError("Get VCPackage Error");
			}
			return await obj2.Content.ReadAsByteArrayAsync();
		}
	}

	public static async Task InstallGameInput()
	{
		_ = 1;
		try
		{
			HttpClient client = new HttpClient();
			try
			{
				byte[] bytes = await DownloadPackageAsync(VCUri.GameInputRedist);
				string fileName = Path.GetTempFileName() + ".msi";
				await File.WriteAllBytesAsync(fileName, bytes);
				MsiHelper.InstallMsiSilently(fileName);
			}
			finally
			{
				if (client != null)
				{
					((IDisposable)client).Dispose();
				}
			}
			async Task<byte[]> DownloadPackageAsync(string uri)
			{
				HttpResponseMessage obj2 = await client.GetAsync(uri);
				if (obj2.StatusCode != HttpStatusCode.OK)
				{
					throw new BedrockCoreNetWorkError("Get VCPackage Error");
				}
				return await obj2.Content.ReadAsByteArrayAsync();
			}
		}
		catch
		{
			throw new BedrockCoreException("Install GameInputRedist Error");
		}
	}
}
