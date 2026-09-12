using System;

namespace BedrockLauncher.Core.GdkDecode;

[Flags]
public enum XvcRegionFlags : uint
{
	Resident = 1u,
	InitialPlay = 2u,
	Preview = 4u,
	FileSystemMetadata = 8u,
	Present = 0x10u,
	OnDemand = 0x20u,
	Available = 0x40u
}
