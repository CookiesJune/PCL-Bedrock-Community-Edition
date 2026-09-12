using System.Runtime.InteropServices;

namespace BedrockLauncher.Core.GdkDecode;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct XvcEncryptionKeyId
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] KeyId;
}
