using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BedrockLauncher.Core.CoreOption;
using BedrockLauncher.Core.DependsComplete;
using BedrockLauncher.Core.GdkDecode;
using BedrockLauncher.Core.Utils;
using BedrockLauncher.Core.UwpRegister;
using Microsoft.Win32;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.System;

namespace BedrockLauncher.Core;

public class BedrockCore
{
	public CoreOptions Options { get; set; } = new CoreOptions();

	public BedrockCore()
	{
		if (Environment.OSVersion.Version.Build < 19041)
		{
			throw new BedrockCoreException("Not Support Windows Version (<19041)");
		}
		Options = new CoreOptions();
	}

	public BedrockCore(CoreOptions options)
	{
		if (Environment.OSVersion.Version.Build < 19041)
		{
			throw new BedrockCoreException("Not Support Windows Version (<19041)");
		}
		Options = options;
	}

	public async Task InitAsync()
	{
		if (Options.IsAutoOpenDevelopment && !GetWindowsDevelopmentState())
		{
			throw new BedrockCoreException("Windows Developer Mode is required for non-admin UWP loose package registration. Please enable Developer Mode in Windows settings.");
		}
		if (Options.IsAutoCompleteVC)
		{
			var (flag, flag2) = IsHasVCRuntime(RuntimeInformation.OSArchitecture);
			if (!flag || !flag2)
			{
				VCRuntimeHelper.CompleteVCRuntimeAsync(RuntimeInformation.OSArchitecture).Wait();
			}
		}
		if (Options.IsAutoCompleteGameInput)
		{
			await AutoCompleteGameInput();
		}
	}

	public bool GetWindowsDevelopmentState()
	{
		try
		{
			object obj = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock", writable: true)?.GetValue("AllowDevelopmentWithoutDevLicense", 1);
			if (obj == null)
			{
				return false;
			}
			if ((int)obj == 0)
			{
				return false;
			}
			return true;
		}
		catch
		{
			throw new BedrockCoreException("Can't Get Development state");
		}
	}

	public bool OpenWindowsDevelopment()
	{
		try
		{
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock", writable: true)?.SetValue("AllowDevelopmentWithoutDevLicense", 1);
			return true;
		}
		catch
		{
			throw new BedrockCoreException("Can't Open Deveopment Successfully");
		}
	}

	public (bool, bool) IsHasVCRuntime(Architecture arch)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			bool flag = false;
			bool item = false;
			flag = CheckVersion(arch switch
			{
				Architecture.X64 => new string[2] { "SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x64", "SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x64" }, 
				Architecture.X86 => new string[2] { "SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x86", "SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x86" }, 
				Architecture.Arm64 => new string[2] { "SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\arm64", "SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\arm64" }, 
				_ => new string[2] { "SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x64", "SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x64" }, 
			});
			if ((from p in new PackageManager().FindPackagesForUser(string.Empty)
				where p.Id.Name.Contains("Microsoft.VCLibs.140")
				select p).Count() != 0)
			{
				item = true;
			}
			return (item, flag);
		}
		catch
		{
			return (false, false);
		}
		static bool CheckVersion(string[] archli)
		{
			foreach (string name in archli)
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
				if (registryKey != null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public async Task<InstallResult?> InstallPackageAsync(LocalGamePackageOptions options)
	{
		InstallResult installResult = new InstallResult();
		Directory.CreateDirectory(options.InstallDstFolder);
		if (options.Type == MinecraftBuildTypeVersion.GDK)
		{
			await Task.Run(async delegate
			{
				MsiXVDDecoder decoder = new MsiXVDDecoder(new CikKey(options.GameTypeVersion switch
				{
					MinecraftGameTypeVersion.Release => _DEFINE_REF2.rel, 
					MinecraftGameTypeVersion.Preview => _DEFINE_REF2.pre, 
					MinecraftGameTypeVersion.Beta => _DEFINE_REF2.pre, 
					_ => null, 
				}), options.UseHardwareDecode);
				MsiXVDStream msiXVDStream = new MsiXVDStream(options.FileFullPath);
				msiXVDStream.Parse();
				options.InstallStates?.Report(InstallStates.Extracting);
				await msiXVDStream.ExtractTaskAsync(Path.GetFullPath(options.InstallDstFolder), decoder, options.ExtractionProgress, options.CancellationToken.GetValueOrDefault());
				options.InstallStates?.Report(InstallStates.Extracted);
			});
			return installResult;
		}
		if (options.Type == MinecraftBuildTypeVersion.UWP)
		{
			options.InstallStates?.Report(InstallStates.Extracting);
			await ZipExtractor.ExtractWithProgressAsync(options.FileFullPath, options.InstallDstFolder, options.ExtractionProgress, options.CancellationToken.GetValueOrDefault());
			File.Delete(Path.Combine(options.InstallDstFolder, "AppxSignature.p7x"));
			options.InstallStates?.Report(InstallStates.Extracted);
			MinecraftGameTypeVersion gameTypeVersion = options.GameTypeVersion;
			bool is_installed = BedrockLauncher.Core.UwpRegister.UwpRegister.IsPackageInstalled(gameTypeVersion switch
			{
				MinecraftGameTypeVersion.Beta => "Microsoft.MinecraftWindowsBeta", 
				MinecraftGameTypeVersion.Release => "Microsoft.MinecraftUWP", 
				MinecraftGameTypeVersion.Preview => "Microsoft.MinecraftWindowsBeta", 
			});
			await ManifestEditor.EditManifest(options.InstallDstFolder, options.GameName ?? TimeBasedVersion.GetVersion(), options.BackGroundConfig);
			DeploymentOptionsConfig config = new DeploymentOptionsConfig
			{
				PackagePath = Path.Combine(options.InstallDstFolder, "AppxManifest.xml"),
				CancellationToken = options.CancellationToken.GetValueOrDefault(),
				Timeout = new TimeSpan(0, 3, 0),
				DeploymentOptions = (DeploymentOptions)(is_installed ? 262146 : 2),
				ProgressCallback = options.DeployProgress
			};
			options.InstallStates?.Report(InstallStates.Registering);
			DeploymentResult deploymentResult = await BedrockLauncher.Core.UwpRegister.UwpRegister.RegisterAppxAsync(config);
			options.InstallStates?.Report(InstallStates.Registered);
			installResult.DeploymentResult = deploymentResult;
			return installResult;
		}
		return null;
	}

	private static async Task<Process> WaitForProcessAsync(string processName, DateTime startTime, TimeSpan timeout)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (stopwatch.Elapsed < timeout)
		{
			Process[] processesByName = Process.GetProcessesByName(processName);
			if (processesByName.Length != 0)
			{
				return (from p in processesByName
					where p.StartTime > startTime
					orderby (p.StartTime - startTime).TotalMilliseconds
					select p).FirstOrDefault();
			}
			await Task.Delay(200);
		}
		return null;
	}

	private static DateTime GetStartTimeSafe(Process proc)
	{
		try
		{
			return proc.StartTime;
		}
		catch
		{
			return DateTime.MinValue;
		}
	}

	public async Task<Process> LaunchGameAsync(LaunchOptions options)
	{
		Process result = new Process();
		if (options.MinecraftBuildType == MinecraftBuildTypeVersion.GDK)
		{
			options.Progress?.Report(LaunchState.Launching);
			string path = "Minecraft.Windows.exe";
			string fileName = Path.Combine(options.GameFolder, path);
			result = Process.Start(new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = options.LaunchArgs,
				WorkingDirectory = options.GameFolder,
				UseShellExecute = true,
				CreateNoWindow = false,
				Verb = (options.RunAsAdministrator ? "runas" : string.Empty)
			});
			options.Progress?.Report(LaunchState.Launched);
		}
		if (options.MinecraftBuildType == MinecraftBuildTypeVersion.UWP)
		{
			string text = Path.Combine(options.GameFolder, "AppxManifest.xml");
			MinecraftGameTypeVersion gameType = options.GameType;
			string packageFamily = gameType switch
			{
				MinecraftGameTypeVersion.Release => "Microsoft.MinecraftUWP_8wekyb3d8bbwe", 
				MinecraftGameTypeVersion.Preview => "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe", 
				MinecraftGameTypeVersion.Beta => "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe", 
			};
			gameType = options.GameType;
			string text2 = gameType switch
			{
				MinecraftGameTypeVersion.Beta => "Microsoft.MinecraftWindowsBeta", 
				MinecraftGameTypeVersion.Release => "Microsoft.MinecraftUWP", 
				MinecraftGameTypeVersion.Preview => "Microsoft.MinecraftWindowsBeta", 
			};
			if (!File.Exists(text))
			{
				throw new IOException("File doesn't exist");
			}
			PackageManager val = new PackageManager();
			bool flag = false;
			foreach (Package item in val.FindPackagesForUser(string.Empty))
			{
				if (item.Id.Name.Equals(text2, StringComparison.OrdinalIgnoreCase) && Path.GetFullPath(item.InstalledPath) == Path.GetFullPath(options.GameFolder))
				{
					flag = true;
				}
			}
			bool flag2 = BedrockLauncher.Core.UwpRegister.UwpRegister.IsPackageInstalled(text2);
			DeploymentOptionsConfig deploymentOptionsConfig = new DeploymentOptionsConfig();
			options.Progress?.Report(LaunchState.Registering);
			deploymentOptionsConfig.CancellationToken = options.CancellationToken.GetValueOrDefault();
			deploymentOptionsConfig.PackagePath = text;
			deploymentOptionsConfig.DeploymentOptions = (DeploymentOptions)(flag2 ? 262146 : 2);
			deploymentOptionsConfig.ProgressCallback = options.RegisterProgress;
			if ((!flag & flag2) || !flag2)
			{
				DeploymentResult val2 = await BedrockLauncher.Core.UwpRegister.UwpRegister.RegisterAppxAsync(deploymentOptionsConfig);
				options.Progress?.Report(LaunchState.Registered);
				if (!val2.IsRegistered)
				{
					throw new Exception(val2.ErrorText);
				}
			}
			if (options.Old_VersionLaunching)
			{
				IList<AppDiagnosticInfo> result2 = WindowsRuntimeSystemExtensions.AsTask<IList<AppDiagnosticInfo>>(AppDiagnosticInfo.RequestInfoForPackageAsync(packageFamily)).Result;
				if (result2.Count != 0)
				{
					await result2[0].LaunchAsync();
				}
			}
			else
			{
				LauncherOptions val3 = new LauncherOptions
				{
					TargetApplicationPackageFamilyName = packageFamily
				};
				string uriString = "minecraft://launch";
				if (!string.IsNullOrEmpty(options?.LaunchArgs))
				{
					string text3 = string.Empty;
					string launchArgs = options.LaunchArgs;
					if (launchArgs.StartsWith("minecraft://", StringComparison.OrdinalIgnoreCase))
					{
						int num = launchArgs.IndexOf('?');
						if (num >= 0 && num < launchArgs.Length - 1)
						{
							text3 = launchArgs.Substring(num + 1);
						}
						else
						{
							string text4 = launchArgs.Substring("minecraft://".Length).TrimStart('/');
							if (!string.IsNullOrEmpty(text4))
							{
								text3 = text4 + "=true";
							}
						}
					}
					else if (launchArgs.Contains('=') && !launchArgs.Contains(' '))
					{
						text3 = launchArgs;
					}
					else if (launchArgs.Contains('=') && launchArgs.Contains(' '))
					{
						string[] array = (from arg in launchArgs.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
							where arg.Contains('=')
							select arg.Trim()).ToArray();
						text3 = ((array.Length == 0) ? ("args=" + Uri.EscapeDataString(launchArgs)) : string.Join("&", array));
					}
					else
					{
						text3 = "args=" + Uri.EscapeDataString(launchArgs);
					}
					if (!string.IsNullOrEmpty(text3))
					{
						uriString = "minecraft://launch?" + text3;
					}
				}
				await Launcher.LaunchUriAsync(new Uri(uriString), val3);
			}
			Process[] processesByName = Process.GetProcessesByName("Minecraft.Windows");
			Process[] processesByName2 = Process.GetProcessesByName("Minecraft.Win10.DX11");
			result = (from p in processesByName.Concat(processesByName2).ToArray()
				orderby p.StartTime
				select p).Last();
		}
		return result;
	}

	public async Task<DeploymentResult?> RemoveUWPGameAsync(MinecraftGameTypeVersion type)
	{
		PackageManager val = new PackageManager();
		IEnumerable<Package> enumerable = val.FindPackagesForUser("");
		foreach (Package item in enumerable)
		{
			string familyName = item.Id.FamilyName;
			if (familyName == type switch
			{
				MinecraftGameTypeVersion.Release => "Microsoft.MinecraftUWP_8wekyb3d8bbwe", 
				MinecraftGameTypeVersion.Preview => "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe", 
				MinecraftGameTypeVersion.Beta => "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe", 
			})
			{
				return await WindowsRuntimeSystemExtensions.AsTask<DeploymentResult, DeploymentProgress>(val.RemovePackageAsync(item.Id.FullName, (RemovalOptions)4096));
			}
		}
		return null;
	}

	private async Task<string> GetPackageUriInside([NotNull] string metadata)
	{
		if (metadata.StartsWith("http"))
		{
			return metadata;
		}
		try
		{
			string obj = await UpdateIDHelper.GetUriAsync(metadata);
			if (string.IsNullOrEmpty(obj))
			{
				throw new BedrockCoreNoAvailbaleVersionUri("There is no available uri for this");
			}
			return obj;
		}
		catch
		{
			throw;
		}
	}

	public async Task<string> GetPackageUri(BuildInfo buildInfo, Architecture devicesArch)
	{
		Variation variation = buildInfo.Variations.Find((Variation variation2) => variation2.Arch == devicesArch);
		if (variation == null)
		{
			throw new BedrockCoreException($"Unable to find {devicesArch} Version");
		}
		if (variation.MetaData.Count == 0)
		{
			throw new BedrockCoreNoAvailbaleVersionUri("There is no available Uri to download");
		}
		return await GetPackageUriInside(variation.MetaData.Last());
	}

	public async Task AutoCompleteGameInput()
	{
		if (!MsiHelper.IsMsiProductInstalledByGuid("64d0ccb1-329e-d507-0886-47e53d59ae21"))
		{
			await VCRuntimeHelper.InstallGameInput();
		}
	}
}
