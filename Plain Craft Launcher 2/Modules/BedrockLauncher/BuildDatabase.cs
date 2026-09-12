using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BedrockLauncher.Core.SoureGenerate;

public class BuildDatabase
{
	[JsonPropertyName("CreationTime")]
	public DateTime CreationTime { get; set; }

	[JsonExtensionData]
	public Dictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();

	[JsonIgnore]
	public IAsyncEnumerable<KeyValuePair<string, BuildInfo>> Builds => GetBuildsFromExtensionData();

	private async IAsyncEnumerable<KeyValuePair<string, BuildInfo>> GetBuildsFromExtensionData()
	{
		foreach (KeyValuePair<string, object> extensionDatum in ExtensionData)
		{
			extensionDatum.Deconstruct(out var _, out var value);
			if (!(value is JsonElement { ValueKind: JsonValueKind.Object } jsonElement))
			{
				continue;
			}
			foreach (JsonProperty item in jsonElement.EnumerateObject())
			{
				BuildInfo buildInfo = JsonSerializer.Deserialize(item.Value.GetRawText(), BuildDatabaseContext.Default.BuildInfo);
				if (buildInfo != null)
				{
					yield return new KeyValuePair<string, BuildInfo>(item.Name, buildInfo);
				}
				await Task.Yield();
			}
		}
	}
}
