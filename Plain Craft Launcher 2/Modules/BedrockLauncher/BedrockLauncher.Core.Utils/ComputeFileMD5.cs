using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BedrockLauncher.Core.Utils;

public class ComputeFileMD5
{
	public static async Task<string> ComputeFileMD5Async(string filePath)
	{
		using MD5 md5 = MD5.Create();
		using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 131072, FileOptions.Asynchronous | FileOptions.SequentialScan);
		byte[] buffer = new byte[131072];
		long totalBytesRead = 0L;
		int num;
		while ((num = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
		{
			if (num == buffer.Length)
			{
				md5.TransformBlock(buffer, 0, num, null, 0);
			}
			else
			{
				md5.TransformFinalBlock(buffer, 0, num);
			}
			totalBytesRead += num;
		}
		if (totalBytesRead == 0L || md5.Hash == null)
		{
			md5.TransformFinalBlock(buffer, 0, 0);
		}
		return BitConverter.ToString(md5.Hash ?? new byte[1]).Replace("-", "").ToLowerInvariant();
	}
}
