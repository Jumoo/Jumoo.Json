# Jumoo.Json.Benchmarks

Allocation and throughput benchmarks for the core library.

```bash
dotnet run -c Release --project Jumoo.Json.Benchmarks -- --filter '*'
```

Quote the `*` — in bash an unquoted `*` is expanded to filenames before BenchmarkDotNet sees it.
Filter to one area with e.g. `--filter '*Expansion*'`, and check the fixture payloads with
`-- fixtures`.

`Allocated` is the number that matters most here. The library sits underneath bulk operations,
so several of the changes below barely move mean time but remove a lot of GC pressure —
particularly large object heap traffic, visible as a non-zero `Gen2` column.

## Fixtures

Deterministic, Umbraco-shaped payloads (fixed seed, see `JsonFixtures.cs`):

| Fixture | Size | Notes |
| --- | --- | --- |
| `SmallPlain` | 1.2 kb | no embedded json |
| `SmallEmbedded` | 1.7 kb | string values holding escaped json |
| `LargePlain` | 203 kb | no embedded json - the majority case |
| `LargeEmbedded` | 230 kb | string values holding escaped json |
| `LargePlainPadded` | 203 kb | `LargePlain` with surrounding whitespace |

The plain / embedded split matters: "plain" is the case where expansion has nothing to do and
all the work is wasted.

## Results

Measured on .NET 10.0.10, Intel Core Ultra 7 265, ShortRun job (3 warmup / 3 iterations).
"Before" is the library at commit `6bd9e9d`.

### Where the big wins were

| Benchmark | Time before → after | Allocated before → after |
| --- | --- | --- |
| `IsJsonEqual_SameReference` | 320,388 ns → **0.39 ns** | 815.7 KB → **0 B** |
| `IsJsonEqual_Large_Equal` | 281,293 ns → **173,750 ns** | 815.7 KB → **960 B** |
| `GetPropertyValueOrDefault_Mismatch` | 1,509 ns → **10.3 ns** | 672 B → **0 B** |
| `IsValidJsonString_Large` | 139,969 ns → **98,770 ns** | 320,401 B → **72 B** |
| `Expand_Large_Plain` | 685,772 ns → **162,230 ns** | 2,186 KB → **399 KB** |
| `Serialize_Large_ToStream` (vs `_Flat`) | 171,466 ns → **81,107 ns** | 417,222 B → **312 B** |
| `TryParseToJsonNode_Large_Padded` | 168,520 ns → **133,017 ns** | 736,695 B → **320,350 B** |
| `TryConvertToJsonNode_PlainString` | 65.9 ns → **5.1 ns** | 320 B → **56 B** |
| `GetPropertyAsBool` | 40.6 ns → **16.5 ns** | 32 B → **0 B** |
| `AsListOfJsonObjects` | 856 ns → **404 ns** | 2,736 B → 2,616 B |
| `TryGetValueAs_StringToString` | 12.5 ns → **2.2 ns** | 0 B → 0 B |

`IsJsonEqual` and `IsValidJsonString` also dropped from ~196 and ~92 Gen2 collections per 1000
operations to zero — those were large object heap allocations.

### What did not move

Worth recording so nobody re-optimises them expecting a win:

| Benchmark | Result | Why |
| --- | --- | --- |
| `TryDeserialize_Large_Utf8` | same as the string overload | `JsonSerializer` already transcodes through pooled buffers. The utf8 overload pays off by letting the *caller* skip creating the string at all (`File.ReadAllBytes` over `File.ReadAllText`), which this benchmark can't show because the fixture is already a string. |
| `TryDeserialize_Large_Stream` | slightly slower | stream read overhead; use it for the memory profile, not the speed |
| `GetEscapedJsonValue_Large` | unchanged | it is a parse-then-reserialise round trip by definition |
| `GetPropertyAsString_*` | unchanged | the 48 B is `JsonElement.GetString()` materialising the string for the first time; the 1,040 B is the serialisation the method exists to perform |
| `TryParseToJsonNode_Large` (unpadded) | unchanged | building the node graph dominates; the detection gate was never the cost here |
| `Serialize_Large_Indented` / `_Flat` | unchanged | untouched - `TrySerializeToStream` is the faster path |
