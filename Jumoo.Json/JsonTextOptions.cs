using Jumoo.Json.Converters;
using Jumoo.Json.Resolvers;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using Umbraco.Cms.Infrastructure.Serialization;

namespace Jumoo.Json;

/// <summary>
///  The shared <see cref="JsonSerializerOptions"/> used across Jumoo packages, so they all
///  read and write JSON the same way.
/// </summary>
public static class JsonTextOptions
{
    /// <summary>
    ///  Guards the rebuild in <see cref="AddConverter"/> / <see cref="RemoveConverter"/>.
    /// </summary>
    private static readonly Lock _converterLock = new();

    /// <summary>
    /// Default options for JSON serialization and deserialization.
    /// </summary>
    private static volatile JsonSerializerOptions _defaultOptions = new()
    {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new OrderedPropertiesJsonResolver(),
        MaxDepth = 64,
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
    private static volatile JsonSerializerOptions _flatOptions = new(_defaultOptions)
    {
        WriteIndented = false,
    };

    /// <summary>
    /// Options for JSON nodes (when parsing)
    /// </summary>
    private static readonly JsonNodeOptions _nodeOptions = new()
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

    /// <summary>
    ///  add another convert to the default list of converters.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  Rebuilds the options rather than adding to them in place. JsonSerializerOptions
    ///  makes itself read-only the first time it is used, so mutating Converters threw
    ///  InvalidOperationException for anything registering after the first serialize.
    /// </para>
    /// <para>
    ///  Rebuilding discards the cached type metadata, so treat this as a startup call -
    ///  it isn't something to do per operation. Anything holding an options instance from
    ///  an earlier <see cref="GetOptions"/> call keeps the instance it already has.
    /// </para>
    /// </remarks>
    public static void AddConverter(JsonConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);

        lock (_converterLock)
        {
            if (_defaultOptions.Converters.Contains(converter)) return;

            var updatedDefault = new JsonSerializerOptions(_defaultOptions);
            updatedDefault.Converters.Add(converter);

            var updatedFlat = new JsonSerializerOptions(_flatOptions);
            updatedFlat.Converters.Add(converter);

            // swap only once both have been built, so a failure leaves the pair consistent.
            _defaultOptions = updatedDefault;
            _flatOptions = updatedFlat;
        }
    }

    /// <summary>
    /// Removes a converter from the default list of converters.
    /// </summary>
    /// <remarks>
    ///  Rebuilds the options, for the same reason as <see cref="AddConverter"/>.
    /// </remarks>
    public static void RemoveConverter(JsonConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);

        lock (_converterLock)
        {
            if (_defaultOptions.Converters.Contains(converter) is false) return;

            var updatedDefault = new JsonSerializerOptions(_defaultOptions);
            updatedDefault.Converters.Remove(converter);

            var updatedFlat = new JsonSerializerOptions(_flatOptions);
            updatedFlat.Converters.Remove(converter);

            _defaultOptions = updatedDefault;
            _flatOptions = updatedFlat;
        }
    }
}
