using System;

namespace BedrockLauncher.Core;

public class BedrockCoreNoAvailbaleVersionUri : Exception
{
	public BedrockCoreNoAvailbaleVersionUri(string message)
		: base(message)
	{
	}
}
