using BenchmarkDotNet.Running;

using Jumoo.Json;
using Jumoo.Json.Benchmarks;

using System.Reflection;

// dotnet run -c Release --project Jumoo.Json.Benchmarks -- --filter *
// dotnet run -c Release --project Jumoo.Json.Benchmarks -- --filter *Expansion*
// dotnet run -c Release --project Jumoo.Json.Benchmarks -- fixtures   (sanity check the payloads)

if (args is ["fixtures", ..])
{
    DescribeFixtures();
    return;
}

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);

static void DescribeFixtures()
{
    Describe("SmallPlain", JsonFixtures.SmallPlain);
    Describe("SmallEmbedded", JsonFixtures.SmallEmbedded);
    Describe("LargePlain", JsonFixtures.LargePlain);
    Describe("LargeEmbedded", JsonFixtures.LargeEmbedded);

    static void Describe(string name, string json)
    {
        var array = json.ToJsonArray();
        var expanded = json.ToJsonNode()?.ExpandAllJsonInToken();

        Console.WriteLine(
            $"{name,-15} {json.Length / 1024.0,7:0.0} kb  items: {array.Count,4}  " +
            $"parsed: {array.Count > 0,-5}  expands: {expanded is not null}");
    }
}
