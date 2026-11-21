using BenchmarkDotNet.Attributes;

using System.Text;
using System.Text.Json;

namespace Jumoo.Json.Benchmarks;

/// <summary>
///  Serialize / deserialize entry points, plus the TryGetValueAs conversion helper
///  that sits on the hot path for the object-based overloads.
/// </summary>
[ShortRunJob]
[MemoryDiagnoser]
public class SerializationBenchmarks
{
    private ContentItem[] _items = [];
    private JsonElement _numberElement;
    private JsonElement _stringElement;
    private readonly object _boxedInt = 42;
    private byte[] _largeUtf8 = [];
    private readonly MemoryStream _stream = new(256 * 1024);

    [GlobalSetup]
    public void Setup()
    {
        _items = JsonFixtures.LargePlain.DeserializeJson<ContentItem[]>() ?? [];
        _largeUtf8 = Encoding.UTF8.GetBytes(JsonFixtures.LargePlain);

        using var document = JsonDocument.Parse("{\"number\":42,\"text\":\"forty two\"}");
        _numberElement = document.RootElement.GetProperty("number").Clone();
        _stringElement = document.RootElement.GetProperty("text").Clone();
    }

    [Benchmark]
    public ContentItem[]? TryDeserialize_Small()
        => JsonFixtures.SmallPlain.DeserializeJson<ContentItem[]>();

    [Benchmark]
    public ContentItem[]? TryDeserialize_Large()
        => JsonFixtures.LargePlain.DeserializeJson<ContentItem[]>();

    /// <summary>
    ///  Same payload as TryDeserialize_Large, but skipping the utf16 string entirely.
    /// </summary>
    [Benchmark]
    public ContentItem[]? TryDeserialize_Large_Utf8()
        => _largeUtf8.AsSpan().TryDeserialize<ContentItem[]>(out var result) ? result : null;

    [Benchmark]
    public ContentItem[]? TryDeserialize_Large_Stream()
    {
        _stream.SetLength(0);
        _stream.Write(_largeUtf8);
        _stream.Position = 0;

        return _stream.TryDeserialize<ContentItem[]>(out var result) ? result : null;
    }

    [Benchmark]
    public string? Serialize_Large_Indented()
        => _items.SerializeJsonString(true);

    [Benchmark]
    public string? Serialize_Large_Flat()
        => _items.SerializeJsonString(false);

    /// <summary>
    ///  Same output as Serialize_Large_Flat, without building the string first.
    /// </summary>
    [Benchmark]
    public bool Serialize_Large_ToStream()
    {
        _stream.SetLength(0);
        return _items.TrySerializeToStream(_stream, indent: false);
    }

    /// <summary>
    ///  string -> string. Currently routes through Umbraco's TryConvertTo.
    /// </summary>
    [Benchmark]
    public bool TryGetValueAs_StringToString()
        => JsonFixtures.PlainString.TryGetValueAs<string>(out _);

    /// <summary>
    ///  boxed int -> int. TryConvertTo boxes the result via Attempt&lt;object?&gt;.
    /// </summary>
    [Benchmark]
    public bool TryGetValueAs_IntToInt()
        => _boxedInt.TryGetValueAs<int>(out _);

    [Benchmark]
    public bool TryGetValueAs_JsonElementToInt()
        => _numberElement.TryGetValueAs<int>(out _);

    [Benchmark]
    public bool TryGetValueAs_JsonElementToString()
        => _stringElement.TryGetValueAs<string>(out _);
}
