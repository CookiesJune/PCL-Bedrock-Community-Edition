using System.Runtime.InteropServices;

namespace BedrockLauncher.Core.GdkDecode;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct XvcUpdateSegment
{
	public uint PageNum;

	public ulong Hash;
}
