using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Management.Deployment;

namespace BedrockLauncher.Core.UwpRegister;

public class DeploymentOptionsConfig
{
	[CompilerGenerated]
	private DeploymentOptions _003CDeploymentOptions_003Ek__BackingField;

	public string PackagePath { get; set; } = string.Empty;

	public DeploymentOptions DeploymentOptions
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CDeploymentOptions_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CDeploymentOptions_003Ek__BackingField = value;
		}
	}

	public CancellationToken CancellationToken { get; set; }

	public IProgress<DeploymentProgress>? ProgressCallback { get; set; }

	public TimeSpan? Timeout { get; set; }
}
