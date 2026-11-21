using BenchmarkDotNet.Attributes;

namespace Jumoo.Json.Benchmarks;

/// <summary>
///  IsJsonEqual - the comparison path uSync-style bulk operations lean on hardest.
/// </summary>
[ShortRunJob]
[MemoryDiagnoser]
public class ComparisonBenchmarks
{
    private ContentItem[] _left = [];
    private ContentItem[] _right = [];
    private ContentItem[] _rightDifferent = [];
    private ContentItem _smallLeft = new();
    private ContentItem _smallRight = new();

    [GlobalSetup]
    public void Setup()
    {
        _left = JsonFixtures.LargePlain.DeserializeJson<ContentItem[]>() ?? [];
        _right = JsonFixtures.LargePlain.DeserializeJson<ContentItem[]>() ?? [];

        _rightDifferent = JsonFixtures.LargePlain.DeserializeJson<ContentItem[]>() ?? [];
        if (_rightDifferent.Length > 0) _rightDifferent[0].Name = "changed";

        _smallLeft = _left[0];
        _smallRight = _right[0];
    }

    [Benchmark]
    public bool IsJsonEqual_Small_Equal()
        => _smallLeft.IsJsonEqual(_smallRight);

    [Benchmark]
    public bool IsJsonEqual_Large_Equal()
        => _left.IsJsonEqual(_right);

    /// <summary>
    ///  Differs in the very first item - worst case for the current
    ///  "serialise everything then compare" approach.
    /// </summary>
    [Benchmark]
    public bool IsJsonEqual_Large_Different()
        => _left.IsJsonEqual(_rightDifferent);

    [Benchmark]
    public bool IsJsonEqual_SameReference()
        => _left.IsJsonEqual(_left);
}
