using System;

namespace BedrockLauncher.Core;

public class BedrockCoreNetWorkError : Exception
{
	public BedrockCoreNetWorkError(string message)
		: base(message)
	{
	}
}
