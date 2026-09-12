using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BedrockLauncher.Core;

public class MinecraftBuildTypeVersionConverter : JsonConverter<MinecraftBuildTypeVersion>
{
	public override MinecraftBuildTypeVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			string text = reader.GetString()?.ToUpper();
			return text switch
			{
				"UWP" => MinecraftBuildTypeVersion.UWP, 
				"GDK" => MinecraftBuildTypeVersion.GDK, 
			};
		}
		return MinecraftBuildTypeVersion.UNKNOWN;
	}

	public override void Write(Utf8JsonWriter writer, MinecraftBuildTypeVersion value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString().ToUpper());
	}
}
