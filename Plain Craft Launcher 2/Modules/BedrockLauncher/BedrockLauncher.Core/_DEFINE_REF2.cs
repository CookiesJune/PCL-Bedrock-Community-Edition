using System.Runtime.InteropServices;

namespace BedrockLauncher.Core;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct _DEFINE_REF2
{
	public static readonly byte[] pre = new byte[48]
	{
		63, 214, 73, 31, 245, 139, 141, 31, 237, 126,
		219, 216, 148, 119, 218, 217, 128, 40, 20, 0,
		117, 113, 246, 163, 83, 199, 16, 186, 151, 46,
		241, 19, 198, 242, 80, 197, 75, 49, 90, 246,
		26, 51, 204, 165, 222, 133, 176, 138
	};

	public static readonly byte[] rel = new byte[48]
	{
		145, 231, 185, 189, 124, 201, 52, 55, 225, 168,
		188, 96, 37, 82, 223, 6, 201, 169, 105, 251,
		252, 187, 245, 244, 109, 113, 37, 10, 242, 38,
		207, 106, 199, 209, 92, 37, 249, 84, 99, 68,
		84, 147, 145, 209, 104, 87, 57, 31
	};
}
