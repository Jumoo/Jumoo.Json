# Changelog

Notable changes to `Jumoo.Json`. This library ships one branch per Umbraco major
(`v18/main`, `v17/main`, …), so versions track the Umbraco major they target.

## Unreleased

### Added

- Helpers needed by uSync, which now consumes this library rather than keeping its own copy of it:
  - `ConvertToJsonNode(string)` / `ConvertToJsonNode(object)` — the non-`Try` form of
    `TryConvertToJsonNode`, so a value that isn't json comes back as a string node.
  - `TryGetValueAs(object?, Type, out object?)` — non-generic companion for callers that only have
    a runtime `Type`, with the same `JsonElement` pre-check as the generic overload.
  - `AddOrRemoveIfNull<TNode>(JsonObject?, string, TNode?)`.
  - `IsNonStringJsonValue(object?)`.
  - `GetPropertyAsBool(JsonObject?, string, bool defaultValue)` overload.
- BenchmarkDotNet project (`Jumoo.Json.Benchmarks`) with a documented allocation baseline.
- UTF-8 span and `Stream` overloads on `JsonSerialization` — `TryDeserialize`,
  `DeserializeJsonAsync`, `TrySerializeToStream`, `TrySerializeToStreamAsync`.
- XML documentation now ships in the package, so consumers get IntelliSense.
- Repository standards: `LICENSE`, `.editorconfig`, `global.json`, `Directory.Build.props`,
  `Directory.Packages.props`, dependabot, issue and PR templates.

### Changed

- Substantial allocation reductions across the core paths — comparison, expansion, validation
  and the property accessors. See `Jumoo.Json.Benchmarks/README.md` for before/after numbers.
- `TryExpandJsonNodeValue` now returns the original node when there is nothing to expand, rather
  than always returning a clone. Do not mutate the result unless something was expanded.
- `TryGetValueAs<TObject>` takes `this object?` rather than `this object`. It already null-checked
  internally; the annotation just stops nullable callers having to null-forgive.
- Dependency moved from `Umbraco.Cms.Web.Common` 18.0.0-rc2 to the released 18.0.0.

### Fixed

- `JsonTextOptions.AddConverter` / `RemoveConverter` threw `InvalidOperationException` whenever they
  were called after anything had already been serialized, because `JsonSerializerOptions` makes
  itself read-only on first use. They now rebuild the options instead of mutating them.
- `TryConvertToJsonNode` returned `false` for any string containing a quote, backslash or control
  character. It built its fallback node with `JsonNode.Parse($"\"{value}\"")`, which produced
  invalid JSON for those inputs.
- Package metadata was silently dropped: the project used `ProjectUrl` and `PackageReadme` instead
  of `PackageProjectUrl` and `PackageReadmeFile`, so the published package had no project link and
  NuGet never rendered the readme. `RepositoryUrl` was never set either.
- CI never ran. The workflow triggered on `main` while the default branch is `v18/main`, and
  pinned .NET 9 against `net10.0` projects.

## 18.0.1

- Umbraco 18 support.

## 17.0.0

- Umbraco 17 support.

## 16.0.0

- Initial release, Umbraco 16 support.
