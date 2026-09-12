using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BedrockLauncher.Core;

public class MinecraftGameTypeVersionConverter : JsonConverter<MinecraftGameTypeVersion>
{
	public override MinecraftGameTypeVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			return reader.GetString()?.ToLower() switch
			{
				"preview" => MinecraftGameTypeVersion.Preview, 
				"release" => MinecraftGameTypeVersion.Release, 
				"beta" => MinecraftGameTypeVersion.Beta, 
				_ => MinecraftGameTypeVersion.Release, 
			};
		}
		return MinecraftGameTypeVersion.Release;
	}

	public override void Write(Utf8JsonWriter writer, MinecraftGameTypeVersion value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString().ToLower());
	}
}
