using System;
using System.Threading;
using BedrockLauncher.Core.BackGround;
using BedrockLauncher.Core.Utils;
using Windows.Management.Deployment;

namespace BedrockLauncher.Core.CoreOption;

public class LocalGamePackageOptions
{
	public required string FileFullPath;

	public required MinecraftBuildTypeVersion Type;

	public required string InstallDstFolder;

	public Progress<DecompressProgress>? ExtractionProgress;

	public Progress<DeploymentProgress>? DeployProgress;

	public IProgress<InstallStates>? InstallStates;

	public CancellationToken? CancellationToken;

	public required MinecraftGameTypeVersion GameTypeVersion;

	public BackGroundConfig? BackGroundConfig;

	public string? GameName;

	public bool UseHardwareDecode { get; set; } = true;
}
