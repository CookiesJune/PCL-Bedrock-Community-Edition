namespace BedrockLauncher.Core.Utils;

public struct DecompressProgress
{
	public string FileName;

	public long CurrentCount;

	public long TotalCount;

	public double Percentage
	{
		get
		{
			if (TotalCount <= 0)
			{
				return 0.0;
			}
			return (double)CurrentCount / (double)TotalCount * 100.0;
		}
	}
}
