using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BedrockLauncher.Core;

public class ArchitectureJsonConverter : JsonConverter<Architecture>
{
	public override Architecture Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return Architecture.X86;
		}
		if (reader.TokenType == JsonTokenType.String)
		{
			string text = reader.GetString();
			if (string.IsNullOrEmpty(text))
			{
				return Architecture.X86;
			}
			string text2 = text.ToLowerInvariant();
			switch (text2)
			{
			case "x64":
			case "amd64":
			case "x86_64":
				return Architecture.X64;
			case "x86":
			case "ia32":
			case "i386":
				return Architecture.X86;
			case "arm":
			case "arm32":
				return Architecture.Arm;
			case "arm64":
			case "aarch64":
				return Architecture.Arm64;
			case "wasm":
			case "webassembly":
				return Architecture.Wasm;
			case "s390x":
				return Architecture.S390x;
			case "loongarch64":
				return Architecture.LoongArch64;
			case "armv6":
				return Architecture.Armv6;
			case "ppc64le":
				return Architecture.Ppc64le;
			default:
				throw new SwitchExpressionException((object?)text2);
			}
		}
		if (reader.TokenType == JsonTokenType.Number)
		{
			return (Architecture)reader.GetInt32();
		}
		throw new JsonException($"Cant covert this token. TokenType: {reader.TokenType}");
	}

	public override void Write(Utf8JsonWriter writer, Architecture value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString().ToLowerInvariant());
	}
}
