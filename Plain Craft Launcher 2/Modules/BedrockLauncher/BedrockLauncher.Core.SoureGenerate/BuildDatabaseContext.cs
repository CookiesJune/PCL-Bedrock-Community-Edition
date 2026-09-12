using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace BedrockLauncher.Core.SoureGenerate;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(BuildDatabase))]
[JsonSerializable(typeof(BuildInfo))]
[JsonSerializable(typeof(List<Variation>))]
[JsonSerializable(typeof(Variation))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(MinecraftBuildTypeVersion))]
[JsonSerializable(typeof(MinecraftGameTypeVersion))]
[JsonSerializable(typeof(Dictionary<string, BuildInfo>))]
[JsonSerializable(typeof(Architecture))]
[GeneratedCode("System.Text.Json.SourceGeneration", "10.0.14.32716")]
public class BuildDatabaseContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<MinecraftBuildTypeVersion>? _MinecraftBuildTypeVersion;

	private JsonTypeInfo<MinecraftGameTypeVersion>? _MinecraftGameTypeVersion;

	private JsonTypeInfo<BuildDatabase>? _BuildDatabase;

	private JsonTypeInfo<BuildInfo>? _BuildInfo;

	private JsonTypeInfo<Dictionary<string, BuildInfo>>? _DictionaryStringBuildInfo;

	private JsonTypeInfo<Dictionary<string, object>>? _DictionaryStringObject;

	private JsonTypeInfo<List<Variation>>? _ListVariation;

	private JsonTypeInfo<List<string>>? _ListString;

	private JsonTypeInfo<DateTime>? _DateTime;

	private JsonTypeInfo<Architecture>? _Architecture;

	private JsonTypeInfo<Variation>? _Variation;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<object>? _Object;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public JsonTypeInfo<MinecraftBuildTypeVersion> MinecraftBuildTypeVersion => _MinecraftBuildTypeVersion ?? (_MinecraftBuildTypeVersion = (JsonTypeInfo<MinecraftBuildTypeVersion>)base.Options.GetTypeInfo(typeof(MinecraftBuildTypeVersion)));

	public JsonTypeInfo<MinecraftGameTypeVersion> MinecraftGameTypeVersion => _MinecraftGameTypeVersion ?? (_MinecraftGameTypeVersion = (JsonTypeInfo<MinecraftGameTypeVersion>)base.Options.GetTypeInfo(typeof(MinecraftGameTypeVersion)));

	public JsonTypeInfo<BuildDatabase> BuildDatabase => _BuildDatabase ?? (_BuildDatabase = (JsonTypeInfo<BuildDatabase>)base.Options.GetTypeInfo(typeof(BuildDatabase)));

	public JsonTypeInfo<BuildInfo> BuildInfo => _BuildInfo ?? (_BuildInfo = (JsonTypeInfo<BuildInfo>)base.Options.GetTypeInfo(typeof(BuildInfo)));

	public JsonTypeInfo<Dictionary<string, BuildInfo>> DictionaryStringBuildInfo => _DictionaryStringBuildInfo ?? (_DictionaryStringBuildInfo = (JsonTypeInfo<Dictionary<string, BuildInfo>>)base.Options.GetTypeInfo(typeof(Dictionary<string, BuildInfo>)));

	public JsonTypeInfo<Dictionary<string, object>> DictionaryStringObject => _DictionaryStringObject ?? (_DictionaryStringObject = (JsonTypeInfo<Dictionary<string, object>>)base.Options.GetTypeInfo(typeof(Dictionary<string, object>)));

	public JsonTypeInfo<List<Variation>> ListVariation => _ListVariation ?? (_ListVariation = (JsonTypeInfo<List<Variation>>)base.Options.GetTypeInfo(typeof(List<Variation>)));

	public JsonTypeInfo<List<string>> ListString => _ListString ?? (_ListString = (JsonTypeInfo<List<string>>)base.Options.GetTypeInfo(typeof(List<string>)));

	public JsonTypeInfo<DateTime> DateTime => _DateTime ?? (_DateTime = (JsonTypeInfo<DateTime>)base.Options.GetTypeInfo(typeof(DateTime)));

	public JsonTypeInfo<Architecture> Architecture => _Architecture ?? (_Architecture = (JsonTypeInfo<Architecture>)base.Options.GetTypeInfo(typeof(Architecture)));

	public JsonTypeInfo<Variation> Variation => _Variation ?? (_Variation = (JsonTypeInfo<Variation>)base.Options.GetTypeInfo(typeof(Variation)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<object> Object => _Object ?? (_Object = (JsonTypeInfo<object>)base.Options.GetTypeInfo(typeof(object)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static BuildDatabaseContext Default { get; } = new BuildDatabaseContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<MinecraftBuildTypeVersion> Create_MinecraftBuildTypeVersion(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<MinecraftBuildTypeVersion> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<MinecraftBuildTypeVersion>(options, JsonMetadataServices.GetEnumConverter<MinecraftBuildTypeVersion>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<MinecraftGameTypeVersion> Create_MinecraftGameTypeVersion(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<MinecraftGameTypeVersion> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<MinecraftGameTypeVersion>(options, JsonMetadataServices.GetEnumConverter<MinecraftGameTypeVersion>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<BuildDatabase> Create_BuildDatabase(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<BuildDatabase> jsonTypeInfo))
		{
			JsonObjectInfoValues<BuildDatabase> objectInfo = new JsonObjectInfoValues<BuildDatabase>
			{
				ObjectCreator = () => new BuildDatabase(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => BuildDatabasePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(BuildDatabase).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] BuildDatabasePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[3];
		JsonPropertyInfoValues<DateTime> propertyInfo = new JsonPropertyInfoValues<DateTime>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildDatabase),
			Converter = null,
			Getter = (object obj) => ((BuildDatabase)obj).CreationTime,
			Setter = delegate(object obj, DateTime value)
			{
				((BuildDatabase)obj).CreationTime = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "CreationTime",
			JsonPropertyName = "CreationTime",
			AttributeProviderFactory = () => typeof(BuildDatabase).GetProperty("CreationTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(DateTime), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<Dictionary<string, object>> propertyInfo2 = new JsonPropertyInfoValues<Dictionary<string, object>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildDatabase),
			Converter = null,
			Getter = (object obj) => ((BuildDatabase)obj).ExtensionData,
			Setter = delegate(object obj, Dictionary<string, object>? value)
			{
				((BuildDatabase)obj).ExtensionData = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = true,
			NumberHandling = null,
			PropertyName = "ExtensionData",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(BuildDatabase).GetProperty("ExtensionData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Dictionary<string, object>), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		JsonPropertyInfoValues<object> propertyInfo3 = new JsonPropertyInfoValues<object>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildDatabase),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Builds",
			JsonPropertyName = null,
			AttributeProviderFactory = () => (ICustomAttributeProvider)null
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		return array;
	}

	private JsonTypeInfo<BuildInfo> Create_BuildInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<BuildInfo> jsonTypeInfo))
		{
			JsonObjectInfoValues<BuildInfo> objectInfo = new JsonObjectInfoValues<BuildInfo>
			{
				ObjectCreator = () => new BuildInfo(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => BuildInfoPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(BuildInfo).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] BuildInfoPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[6];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildInfo),
			Converter = null,
			Getter = (object obj) => ((BuildInfo)obj).Key,
			Setter = delegate(object obj, string? value)
			{
				((BuildInfo)obj).Key = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Key",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(BuildInfo).GetProperty("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<MinecraftGameTypeVersion> propertyInfo2 = new JsonPropertyInfoValues<MinecraftGameTypeVersion>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildInfo),
			Converter = (JsonConverter<MinecraftGameTypeVersion>)ExpandConverter(typeof(MinecraftGameTypeVersion), new MinecraftGameTypeVersionConverter(), options),
			Getter = (object obj) => ((BuildInfo)obj).Type,
			Setter = delegate(object obj, MinecraftGameTypeVersion value)
			{
				((BuildInfo)obj).Type = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Type",
			JsonPropertyName = "Type",
			AttributeProviderFactory = () => typeof(BuildInfo).GetProperty("Type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(MinecraftGameTypeVersion), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<MinecraftBuildTypeVersion> propertyInfo3 = new JsonPropertyInfoValues<MinecraftBuildTypeVersion>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildInfo),
			Converter = (JsonConverter<MinecraftBuildTypeVersion>)ExpandConverter(typeof(MinecraftBuildTypeVersion), new MinecraftBuildTypeVersionConverter(), options),
			Getter = (object obj) => ((BuildInfo)obj).BuildType,
			Setter = delegate(object obj, MinecraftBuildTypeVersion value)
			{
				((BuildInfo)obj).BuildType = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "BuildType",
			JsonPropertyName = "BuildType",
			AttributeProviderFactory = () => typeof(BuildInfo).GetProperty("BuildType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(MinecraftBuildTypeVersion), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<string> propertyInfo4 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildInfo),
			Converter = null,
			Getter = (object obj) => ((BuildInfo)obj).ID,
			Setter = delegate(object obj, string? value)
			{
				((BuildInfo)obj).ID = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ID",
			JsonPropertyName = "ID",
			AttributeProviderFactory = () => typeof(BuildInfo).GetProperty("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildInfo),
			Converter = null,
			Getter = (object obj) => ((BuildInfo)obj).Date,
			Setter = delegate(object obj, string? value)
			{
				((BuildInfo)obj).Date = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Date",
			JsonPropertyName = "Date",
			AttributeProviderFactory = () => typeof(BuildInfo).GetProperty("Date", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		JsonPropertyInfoValues<List<Variation>> propertyInfo6 = new JsonPropertyInfoValues<List<Variation>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(BuildInfo),
			Converter = null,
			Getter = (object obj) => ((BuildInfo)obj).Variations,
			Setter = delegate(object obj, List<Variation>? value)
			{
				((BuildInfo)obj).Variations = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Variations",
			JsonPropertyName = "Variations",
			AttributeProviderFactory = () => typeof(BuildInfo).GetProperty("Variations", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<Variation>), Array.Empty<Type>(), null)
		};
		array[5] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo6);
		array[5].IsGetNullable = false;
		array[5].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<Dictionary<string, BuildInfo>> Create_DictionaryStringBuildInfo(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, BuildInfo>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, BuildInfo>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, BuildInfo>>
			{
				ObjectCreator = () => new Dictionary<string, BuildInfo>(),
				SerializeHandler = DictionaryStringBuildInfoSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, BuildInfo>, string, BuildInfo>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void DictionaryStringBuildInfoSerializeHandler(Utf8JsonWriter writer, Dictionary<string, BuildInfo>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		foreach (KeyValuePair<string, BuildInfo> item in value)
		{
			writer.WritePropertyName(item.Key);
			JsonSerializer.Serialize(writer, item.Value, BuildInfo);
		}
		writer.WriteEndObject();
	}

	private JsonTypeInfo<Dictionary<string, object>> Create_DictionaryStringObject(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, object>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, object>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, object>>
			{
				ObjectCreator = () => new Dictionary<string, object>(),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, object>, string, object>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<List<Variation>> Create_ListVariation(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<Variation>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<Variation>> collectionInfo = new JsonCollectionInfoValues<List<Variation>>
			{
				ObjectCreator = () => new List<Variation>(),
				SerializeHandler = ListVariationSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<Variation>, Variation>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListVariationSerializeHandler(Utf8JsonWriter writer, List<Variation>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			JsonSerializer.Serialize(writer, value[i], Variation);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<List<string>> Create_ListString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<List<string>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<List<string>> collectionInfo = new JsonCollectionInfoValues<List<string>>
			{
				ObjectCreator = () => new List<string>(),
				SerializeHandler = ListStringSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<string>, string>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void ListStringSerializeHandler(Utf8JsonWriter writer, List<string>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Count; i++)
		{
			writer.WriteStringValue(value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<DateTime> Create_DateTime(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<DateTime> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<DateTime>(options, JsonMetadataServices.DateTimeConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<Architecture> Create_Architecture(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Architecture> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<Architecture>(options, JsonMetadataServices.GetEnumConverter<Architecture>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<Variation> Create_Variation(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Variation> jsonTypeInfo))
		{
			JsonObjectInfoValues<Variation> objectInfo = new JsonObjectInfoValues<Variation>
			{
				ObjectCreator = () => new Variation(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => VariationPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(Variation).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] VariationPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[5];
		JsonPropertyInfoValues<Architecture> propertyInfo = new JsonPropertyInfoValues<Architecture>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(Variation),
			Converter = (JsonConverter<Architecture>)ExpandConverter(typeof(Architecture), new ArchitectureJsonConverter(), options),
			Getter = (object obj) => ((Variation)obj).Arch,
			Setter = delegate(object obj, Architecture value)
			{
				((Variation)obj).Arch = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Arch",
			JsonPropertyName = "Arch",
			AttributeProviderFactory = () => typeof(Variation).GetProperty("Arch", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Architecture), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<int> propertyInfo2 = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(Variation),
			Converter = null,
			Getter = (object obj) => ((Variation)obj).ArchivalStatus,
			Setter = delegate(object obj, int value)
			{
				((Variation)obj).ArchivalStatus = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ArchivalStatus",
			JsonPropertyName = "ArchivalStatus",
			AttributeProviderFactory = () => typeof(Variation).GetProperty("ArchivalStatus", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		JsonPropertyInfoValues<string> propertyInfo3 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(Variation),
			Converter = null,
			Getter = (object obj) => ((Variation)obj).OSBuild,
			Setter = delegate(object obj, string? value)
			{
				((Variation)obj).OSBuild = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "OSBuild",
			JsonPropertyName = "OSbuild",
			AttributeProviderFactory = () => typeof(Variation).GetProperty("OSBuild", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		array[2].IsGetNullable = false;
		array[2].IsSetNullable = false;
		JsonPropertyInfoValues<List<string>> propertyInfo4 = new JsonPropertyInfoValues<List<string>>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(Variation),
			Converter = null,
			Getter = (object obj) => ((Variation)obj).MetaData,
			Setter = delegate(object obj, List<string>? value)
			{
				((Variation)obj).MetaData = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MetaData",
			JsonPropertyName = "MetaData",
			AttributeProviderFactory = () => typeof(Variation).GetProperty("MetaData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(List<string>), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		array[3].IsSetNullable = false;
		JsonPropertyInfoValues<string> propertyInfo5 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(Variation),
			Converter = null,
			Getter = (object obj) => ((Variation)obj).MD5,
			Setter = delegate(object obj, string? value)
			{
				((Variation)obj).MD5 = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "MD5",
			JsonPropertyName = "MD5",
			AttributeProviderFactory = () => typeof(Variation).GetProperty("MD5", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[4] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo5);
		array[4].IsGetNullable = false;
		array[4].IsSetNullable = false;
		return array;
	}

	private JsonTypeInfo<int> Create_Int32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<int> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<int>(options, JsonMetadataServices.Int32Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<object> Create_Object(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<object> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<object>(options, JsonMetadataServices.ObjectConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<string> Create_String(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<string> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<string>(options, JsonMetadataServices.StringConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	public BuildDatabaseContext()
		: base(null)
	{
	}

	public BuildDatabaseContext(JsonSerializerOptions options)
		: base(options)
	{
	}

	private static bool TryGetTypeInfoForRuntimeCustomConverter<TJsonMetadataType>(JsonSerializerOptions options, out JsonTypeInfo<TJsonMetadataType> jsonTypeInfo)
	{
		JsonConverter runtimeConverterForType = GetRuntimeConverterForType(typeof(TJsonMetadataType), options);
		if (runtimeConverterForType != null)
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<TJsonMetadataType>(options, runtimeConverterForType);
			return true;
		}
		jsonTypeInfo = null;
		return false;
	}

	private static JsonConverter? GetRuntimeConverterForType(Type type, JsonSerializerOptions options)
	{
		for (int i = 0; i < options.Converters.Count; i++)
		{
			JsonConverter jsonConverter = options.Converters[i];
			if (jsonConverter != null && jsonConverter.CanConvert(type))
			{
				return ExpandConverter(type, jsonConverter, options, validateCanConvert: false);
			}
		}
		return null;
	}

	private static JsonConverter ExpandConverter(Type type, JsonConverter converter, JsonSerializerOptions options, bool validateCanConvert = true)
	{
		if (validateCanConvert && !converter.CanConvert(type))
		{
			throw new InvalidOperationException($"The converter '{converter.GetType()}' is not compatible with the type '{type}'.");
		}
		if (converter is JsonConverterFactory jsonConverterFactory)
		{
			converter = jsonConverterFactory.CreateConverter(type, options);
			if (converter == null || converter is JsonConverterFactory)
			{
				throw new InvalidOperationException($"The converter '{jsonConverterFactory.GetType()}' cannot return null or a JsonConverterFactory instance.");
			}
		}
		return converter;
	}

	public override JsonTypeInfo? GetTypeInfo(Type type)
	{
		base.Options.TryGetTypeInfo(type, out JsonTypeInfo typeInfo);
		return typeInfo;
	}

	JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		if (type == typeof(MinecraftBuildTypeVersion))
		{
			return Create_MinecraftBuildTypeVersion(options);
		}
		if (type == typeof(MinecraftGameTypeVersion))
		{
			return Create_MinecraftGameTypeVersion(options);
		}
		if (type == typeof(BuildDatabase))
		{
			return Create_BuildDatabase(options);
		}
		if (type == typeof(BuildInfo))
		{
			return Create_BuildInfo(options);
		}
		if (type == typeof(Dictionary<string, BuildInfo>))
		{
			return Create_DictionaryStringBuildInfo(options);
		}
		if (type == typeof(Dictionary<string, object>))
		{
			return Create_DictionaryStringObject(options);
		}
		if (type == typeof(List<Variation>))
		{
			return Create_ListVariation(options);
		}
		if (type == typeof(List<string>))
		{
			return Create_ListString(options);
		}
		if (type == typeof(DateTime))
		{
			return Create_DateTime(options);
		}
		if (type == typeof(Architecture))
		{
			return Create_Architecture(options);
		}
		if (type == typeof(Variation))
		{
			return Create_Variation(options);
		}
		if (type == typeof(int))
		{
			return Create_Int32(options);
		}
		if (type == typeof(object))
		{
			return Create_Object(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}
