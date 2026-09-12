using System.Runtime.InteropServices;

namespace BedrockLauncher.Core.GdkDecode;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
public struct UserDataHeader
{
	public uint Length;

	public uint Version;

	public UserDataType Type;

	public uint Unknown;
}
