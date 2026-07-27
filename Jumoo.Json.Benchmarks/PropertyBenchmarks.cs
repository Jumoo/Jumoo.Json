using BenchmarkDotNet.Attributes;

using System.Text.Json.Nodes;

namespace Jumoo.Json.Benchmarks;

/// <summary>
///  The per-property accessors. These are individually tiny but get called once per
///  property per item, so allocations and thrown exceptions here multiply hard.
/// </summary>
[ShortRunJob]
[MemoryDiagnoser]
public class PropertyBenchmarks
{
    private JsonObject _item = null!;
    private JsonArray _array = null!;

    [GlobalSetup]
    public void Setup()
    {
        _array = JsonFixtures.LargePlain.ToJsonArray();
        _item = _array[0]!.AsObject();
    }

    [Benchmark]
    public int GetPropertyValueOrDefault_Match()
        => _item.GetPropertyValueOrDefault("level", 0);

    /// <summary>
    ///  Type mismatch - currently throws and catches an InvalidOperationException
    ///  on every single call.
    /// </summary>
    [Benchmark]
    public int GetPropertyValueOrDefault_Mismatch()
        => _item.GetPropertyValueOrDefault("alias", 0);

    [Benchmark]
    public int GetPropertyValueOrDefault_Missing()
        => _item.GetPropertyValueOrDefault("notThere", 0);

    [Benchmark]
    public string? GetPropertyAsString_String()
        => _item.GetPropertyAsString("alias");

    /// <summary>
    ///  ToString() on a non-string node spins up an indented Utf8JsonWriter
    ///  over the whole subtree.
    /// </summary>
    [Benchmark]
    public string? GetPropertyAsString_Object()
        => _item.GetPropertyAsString("properties");

    [Benchmark]
    public bool GetPropertyAsBool()
        => _item.GetPropertyAsBool("published");

    [Benchmark]
    public bool TryGetPropertyAsJsonObject()
        => _item.TryGetPropertyAsJsonObject("properties", out _);

    [Benchmark]
    public IList<JsonObject> AsListOfJsonObjects()
        => _array.AsListOfJsonObjects();
}
