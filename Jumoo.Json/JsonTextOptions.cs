using Jumoo.Json.Converters;
using Jumoo.Json.Resolvers;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using Umbraco.Cms.Infrastructure.Serialization;

namespace Jumoo.Json;
public static class JsonTextOptions
{
    /// <summary>
    /// Default options for JSON serialization and deserialization.
    /// </summary>
    private static readonly JsonSerializerOptions _defaultOptions = new()
    {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new OrderedPropertiesJsonResolver(),
        Converters =
        {
            new JsonStringEnumConverter(),
            new JsonObjectConverter(),
            new JsonBlockValueConverter(),
            new JsonUdiConverter(),
            new JsonUdiRangeConverter(),
            new JsonBooleanConverter(),
            new JsonXElementConverter(),
        }
    };

    /// <summary>
    /// Flat options for JSON serialization and deserialization.
    /// </summary>
    private static readonly JsonSerializerOptions _flatOptions = new(_defaultOptions)
    {
        WriteIndented = false,
    };

    /// <summary>
    /// Options for JSON nodes (when parsing)
    /// </summary>
    private static JsonNodeOptions _nodeOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Gets the default options for JSON serialization and deserialization.
    /// </summary>
    public static JsonSerializerOptions GetOptions(bool indent = true)
        => indent ? _defaultOptions : _flatOptions;

    /// <summary>
    /// returns the default options for JSON Node parsing.
    /// </summary>
    public static JsonNodeOptions GetNodeOptions() => _nodeOptions;
}
