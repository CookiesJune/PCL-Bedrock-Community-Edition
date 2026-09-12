using System.Collections.Generic;
using System.Text.Json.Serialization;
using BedrockLauncher.Core;

public class BuildInfo
{
	public string Key { get; set; } = string.Empty;

	[JsonPropertyName("Type")]
	[JsonConverter(typeof(MinecraftGameTypeVersionConverter))]
	public MinecraftGameTypeVersion Type { get; set; }

	[JsonPropertyName("BuildType")]
	[JsonConverter(typeof(MinecraftBuildTypeVersionConverter))]
	public MinecraftBuildTypeVersion BuildType { get; set; }

	[JsonPropertyName("ID")]
	public string ID { get; set; } = string.Empty;

	[JsonPropertyName("Date")]
	public string Date { get; set; } = string.Empty;

	[JsonPropertyName("Variations")]
	public List<Variation> Variations { get; set; } = new List<Variation>();
}
