using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BedrockLauncher.Core.Utils;

namespace BedrockLauncher.Core.GdkDecode;

public class MsiXVDStream : IDisposable
{
	private const ulong XVD_HEADER_INCL_SIGNATURE_SIZE = 12288uL;

	public readonly BinaryReader Reader;

	public FileStream XvdFileStream;

	private ulong HashTreePageCount;

	private ulong HashTreePageOffset;

	private ulong MutableDataOffset;

	private bool DataIntegrity;

	private bool Resiliency;

	private ulong HashTreeLevels;

	private ulong XvdUserDataOffset;

	private bool HasSegmentMetadata;

	private SegmentMetadataHeader SegmentMetadataHeaders;

	private SegmentsAbout[] Segments;

	private string[] _segmentPaths;

	public XvcInfo XvcInfo;

	private XvcRegionHeader[] XvcRegions;

	private XvcUpdateSegment[] XvcUpdateSegments;

	private XvcRegionSpecifier[] XvcRegionSpecifiers;

	private UserDataHeader UserDataHeader;

	private bool HasUserDataFile;

	private UserDataPackageFilesHeader UserDataPackageFiles;

	private Dictionary<string, UserDataPackageFileEntry> UserDataPackages = new Dictionary<string, UserDataPackageFileEntry>();

	private Dictionary<string, byte[]> UserDataPackageContents = new Dictionary<string, byte[]>();

	private int HashEntryLength;

	public string[] EncryptionKeys { get; private set; }

	public MsiXVDHeader Header { get; private set; }

	public bool IsEncrypted { get; private set; }

	public MsiXVDStream(string fileUri)
	{
		if (!File.Exists(fileUri))
		{
			throw new FileNotFoundException("Can't found the file");
		}
		XvdFileStream = File.Open(fileUri, FileMode.Open, FileAccess.ReadWrite);
		Reader = new BinaryReader(XvdFileStream);
	}

	public void Parse()
	{
		XvdFileStream.Position = 0L;
		ParseFileHeader();
		Resiliency = Header.Volumes.HasFlag(MsiXVDVolumeAttributes.ResiliencyEnabled);
		DataIntegrity = !Header.Volumes.HasFlag(MsiXVDVolumeAttributes.DataIntegrityDisabled);
		HashTreePageCount = CalculateNumberHashPages(out HashTreeLevels, Header.NumberOfHashedPages, Resiliency);
		MutableDataOffset = Extensions.PageToOffset(Header.EmbeddedXvdPageCount) + 12288;
		HashTreePageOffset = Header.MutableDataLength + MutableDataOffset;
		XvdUserDataOffset = (DataIntegrity ? Extensions.PageToOffset(HashTreePageCount) : 0) + HashTreePageOffset;
		ParaseUserData();
		if (UserDataPackageContents.ContainsKey("SegmentMetadata.bin"))
		{
			ParseSegment();
		}
		ParseArea();
		List<string> list = new List<string>();
		for (int i = 0; i < XvcInfo.EncryptionKeyIds.Length; i++)
		{
			string text = new Guid(XvcInfo.EncryptionKeyIds[i].KeyId).ToString();
			if (!(text == "00000000-0000-0000-0000-000000000000"))
			{
				list.Add(text);
			}
		}
		EncryptionKeys = list.ToArray();
	}

	private void ParseFileHeader()
	{
		int count = Marshal.SizeOf(typeof(MsiXVDHeader));
		IsEncrypted = !(Header = Extensions.GetstructFromBytes<MsiXVDHeader>(Reader.ReadBytes(count))).Volumes.HasFlag(MsiXVDVolumeAttributes.EncryptionDisabled);
		HashEntryLength = (IsEncrypted ? 20 : 24);
	}

	private void ParaseUserData()
	{
		XvdFileStream.Position = (long)XvdUserDataOffset;
		byte[] array = new byte[Header.UserDataLength];
		XvdFileStream.Read(array.AsSpan());
		using BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));
		byte[] array2 = binaryReader.ReadBytes(Marshal.SizeOf(typeof(UserDataHeader)));
		UserDataHeader = Extensions.GetstructFromBytes<UserDataHeader>(array2);
		if (UserDataHeader.Type == UserDataType.PackageFiles)
		{
			HasUserDataFile = true;
			binaryReader.BaseStream.Position = UserDataHeader.Length;
			byte[] array3 = binaryReader.ReadBytes(Marshal.SizeOf(typeof(UserDataPackageFilesHeader)));
			UserDataPackageFiles = Extensions.GetstructFromBytes<UserDataPackageFilesHeader>(array3);
			int fileCount = (int)UserDataPackageFiles.FileCount;
			UserDataPackages.EnsureCapacity(fileCount);
			UserDataPackageFileEntry[] array4 = Extensions.GetstructArraysFromBytes<UserDataPackageFileEntry>(binaryReader.ReadBytes(Marshal.SizeOf(typeof(UserDataPackageFileEntry)) * fileCount), fileCount);
			for (int i = 0; i < array4.Length; i++)
			{
				UserDataPackageFileEntry value = array4[i];
				binaryReader.BaseStream.Position = UserDataHeader.Length + value.Offset;
				byte[] array5 = new byte[value.Size];
				binaryReader.BaseStream.Read(array5.AsSpan());
				UserDataPackages[value.FilePath] = value;
				UserDataPackageContents[value.FilePath] = array5;
			}
		}
	}

	private void ParseArea()
	{
		ulong num = Extensions.PageToOffset(Header.UserDataPageCount) + XvdUserDataOffset;
		XvdFileStream.Position = (int)num;
		byte[] array = new byte[Header.XvcDataLength];
		array.AsSpan();
		XvdFileStream.Read(array.AsSpan());
		using BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));
		XvcInfo = Extensions.GetstructFromBytes<XvcInfo>(binaryReader.ReadBytes(Marshal.SizeOf(typeof(XvcInfo))));
		if (XvcInfo.Version >= 1)
		{
			XvcRegions = Extensions.GetstructArraysFromBytes<XvcRegionHeader>(binaryReader.ReadBytes((int)(Marshal.SizeOf(typeof(XvcRegionHeader)) * XvcInfo.RegionCount)), XvcInfo.RegionCount);
			XvcUpdateSegments = Extensions.GetstructArraysFromBytes<XvcUpdateSegment>(binaryReader.ReadBytes((int)(Marshal.SizeOf(typeof(XvcUpdateSegment)) * XvcInfo.UpdateSegmentCount)), XvcInfo.UpdateSegmentCount);
			if (XvcInfo.Version >= 2)
			{
				XvcRegionSpecifiers = Extensions.GetstructArraysFromBytes<XvcRegionSpecifier>(binaryReader.ReadBytes((int)(Marshal.SizeOf(typeof(XvcRegionSpecifier)) * XvcInfo.RegionSpecifierCount)), XvcInfo.RegionSpecifierCount);
			}
		}
	}

	private void ParseSegment()
	{
		using BinaryReader binaryReader = new BinaryReader(new MemoryStream(UserDataPackageContents["SegmentMetadata.bin"]));
		SegmentMetadataHeaders = Extensions.GetstructFromBytes<SegmentMetadataHeader>(binaryReader.ReadBytes(Marshal.SizeOf(typeof(SegmentMetadataHeader))));
		HasSegmentMetadata = true;
		Segments = Extensions.GetstructArraysFromBytes<SegmentsAbout>(binaryReader.ReadBytes(Marshal.SizeOf(typeof(SegmentsAbout)) * (int)SegmentMetadataHeaders.SegmentCount), (int)SegmentMetadataHeaders.SegmentCount);
		_segmentPaths = new string[SegmentMetadataHeaders.SegmentCount];
		uint num = SegmentMetadataHeaders.HeaderLength + SegmentMetadataHeaders.SegmentCount * 16;
		for (int i = 0; i < Segments.Length; i++)
		{
			SegmentsAbout segmentsAbout = Segments[i];
			binaryReader.BaseStream.Position = num + segmentsAbout.PathOffset;
			Span<byte> span = binaryReader.ReadBytes(segmentsAbout.PathLength * 2).AsSpan();
			_segmentPaths[i] = new string(MemoryMarshal.Cast<byte, char>(span));
		}
	}

	private static ulong CalculateNumberHashPages(out ulong hashTreeLevels, ulong hashedPagesCount, bool resilient)
	{
		ulong num = (hashedPagesCount + 170 - 1) / 170;
		hashTreeLevels = 1uL;
		if (num > 1)
		{
			ulong num2 = 2uL;
			while (num2 > 1)
			{
				ulong num3 = 0uL;
				ulong num4 = hashTreeLevels;
				if (num4 <= 3)
				{
					switch ((uint)num4)
					{
					case 0u:
						num3 = (hashedPagesCount + 170 - 1) / 170;
						break;
					case 1u:
						num3 = (hashedPagesCount + 28900 - 1) / 28900;
						break;
					case 2u:
						num3 = (hashedPagesCount + 4913000 - 1) / 4913000;
						break;
					case 3u:
						num3 = (hashedPagesCount + 835210000 - 1) / 835210000;
						break;
					}
				}
				num2 = num3;
				hashTreeLevels++;
				num += num2;
			}
		}
		if (resilient)
		{
			num *= 2;
		}
		return num;
	}

	public async Task ExtractTaskAsync(string output, MsiXVDDecoder decoder, Progress<DecompressProgress>? progress, CancellationToken cts = default(CancellationToken))
	{
		await Task.Run(delegate
		{
			ulong firstSegmentOffset = Extensions.PageToOffset(XvcUpdateSegments[0].PageNum);
			XvcRegionHeader[] array = XvcRegions.Where((XvcRegionHeader region) => region.FirstSegmentIndex != 0 || firstSegmentOffset == region.Offset).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				XvcRegionHeader xvcRegionHeader = array[num];
				if (cts.IsCancellationRequested)
				{
					break;
				}
				ExtractPart(progress, output, decoder, (uint)xvcRegionHeader.Id, xvcRegionHeader.Offset, xvcRegionHeader.Length, xvcRegionHeader.FirstSegmentIndex, IsEncrypted && xvcRegionHeader.KeyId != ushort.MaxValue, cts);
			}
		});
	}

	private ulong CalculateHashEntryBlockOffset(ulong blockNo, out ulong hashEntryId)
	{
		ulong soureUlong = Extensions.ComputeHashBlockIndexForDataBlock(Header.Kind, HashTreeLevels, Header.NumberOfHashedPages, blockNo, 0u, out hashEntryId, Resiliency);
		return HashTreePageOffset + Extensions.PageToOffset(soureUlong);
	}

	private void ExtractPart(IProgress<DecompressProgress>? progressTask, string outputDirectory, MsiXVDDecoder decryptor, uint headerId, ulong regionStartOffset, ulong regionLength, uint startSegmentIndex, bool shouldDecrypt, CancellationToken cts)
	{
		Span<byte> span = stackalloc byte[16];
		if (shouldDecrypt)
		{
			MemoryMarshal.Cast<byte, uint>(span)[1] = headerId;
			Header.VdUid.AsSpan(0, 8).CopyTo(span.Slice(8));
		}
		bool flag = true;
		long num = (long)regionStartOffset;
		int num2 = 0;
		Span<byte> buffer = new byte[1048576].AsSpan();
		bool flag2 = DataIntegrity;
		long num3 = (long)CalculateHashEntryBlockOffset(Extensions.GetPageOffset(regionStartOffset - XvdUserDataOffset), out var hashEntryId);
		int num4 = (int)(hashEntryId * 24);
		Span<byte> buffer2 = new byte[1048576].AsSpan();
		uint num5 = startSegmentIndex;
		int num6 = 0;
		long pageOffset = (long)Extensions.GetPageOffset(regionLength);
		while (Segments.Length > num5 && pageOffset > num6 && !cts.IsCancellationRequested)
		{
			ulong fileSize = Segments[num5].FileSize;
			string text = _segmentPaths[num5];
			string path = Path.Join(outputDirectory, text);
			string directoryName = Path.GetDirectoryName(path);
			if (directoryName != null)
			{
				Directory.CreateDirectory(directoryName);
			}
			using FileStream fileStream = File.OpenWrite(path);
			ulong num7 = fileSize;
			do
			{
				int num8 = (int)Math.Min(num7, 4096uL);
				if (flag2)
				{
					XvdFileStream.Position = num3;
					XvdFileStream.Read(buffer2);
					flag2 = false;
				}
				if (flag)
				{
					XvdFileStream.Position = num;
					XvdFileStream.Read(buffer);
					flag = false;
				}
				Span<byte> span2 = buffer.Slice(num2, 4096);
				if (DataIntegrity)
				{
					Span<byte> span3 = buffer2.Slice(num4, 24);
					if (shouldDecrypt)
					{
						MemoryMarshal.Cast<byte, uint>(span)[0] = MemoryMarshal.Cast<byte, uint>(span3.Slice(HashEntryLength, 4))[0];
					}
					num4 += 24;
					hashEntryId++;
					if (hashEntryId == 170)
					{
						hashEntryId = 0uL;
						num4 += 16;
					}
					if (num4 == buffer2.Length)
					{
						num3 += num4;
						num4 = 0;
						hashEntryId = 0uL;
						flag2 = true;
					}
				}
				if (shouldDecrypt)
				{
					decryptor.Decrypt(span2, span2, span);
				}
				fileStream.Write(span2.Slice(0, num8));
				num7 -= (uint)num8;
				num2 += 4096;
				if (num2 == buffer.Length)
				{
					num += num2;
					num2 = 0;
					flag = true;
				}
				num6++;
				progressTask?.Report(new DecompressProgress
				{
					CurrentCount = num5,
					FileName = text,
					TotalCount = Segments.Length
				});
			}
			while (num7 != 0);
			num5++;
		}
	}

	public void Dispose()
	{
		XvdFileStream.Dispose();
		GC.Collect();
	}
}
