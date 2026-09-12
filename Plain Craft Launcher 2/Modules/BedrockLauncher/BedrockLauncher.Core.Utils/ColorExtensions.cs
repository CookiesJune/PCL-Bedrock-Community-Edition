using System.Drawing;

namespace BedrockLauncher.Core.Utils;

public static class ColorExtensions
{
	public static string ToHex(this Color color)
	{
		return $"{color.R:X2}{color.G:X2}{color.B:X2}";
	}

	public static string ToHex(this Color color, bool includeHash = false)
	{
		string text = $"{color.R:X2}{color.G:X2}{color.B:X2}";
		if (!includeHash)
		{
			return text;
		}
		return "#" + text;
	}
}
