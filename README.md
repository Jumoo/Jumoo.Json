# Jumoo.Json

A helper library for `System.Text.Json` manipulation while using Umbraco.

This library helps us maintain some consistency when running with `System.Text.Json` as opposed to
Newtonsoft. It is a support library for Jumoo's Umbraco packages rather than something intended for
general use — but we're not stopping you.

Published as `Jumoo.Json`, targeting `net10.0` / Umbraco 18.

## What's in it

| Area | Entry points |
| --- | --- |
| Shared serializer options | `JsonTextOptions.GetOptions()` — camelCase, case-insensitive, ordered properties, Umbraco converters |
| Serialize / deserialize | `SerializeJsonString`, `DeserializeJson<T>`, `TryDeserialize<T>`, plus `Stream` and UTF-8 overloads |
| Parsing | `ToJsonNode`, `ToJsonObject`, `ToJsonArray`, `IsValidJsonString` |
| Property access | `GetPropertyAsString`, `GetPropertyAsBool`, `GetPropertyValueOrDefault<T>` |
| Expansion | `ExpandAllJsonInToken` — unpacks JSON stored as an escaped string inside another value |
| Comparison | `IsJsonEqual` |

Property ordering is deliberate: it keeps serialized files stable, which means smaller diffs and
faster change detection for the packages built on top of this.

## Building

Requires the .NET SDK pinned in [`global.json`](global.json).

```bash
dotnet build Jumoo.Json.slnx -c Release
```

```bash
dotnet test Jumoo.Json.slnx
```

Package versions are managed centrally in
[`Directory.Packages.props`](Directory.Packages.props); shared build and package metadata lives in
[`Directory.Build.props`](Directory.Build.props). Restores are locked — if you change a dependency,
commit the updated `packages.lock.json` files alongside it.

## Benchmarks

The library sits underneath bulk operations, so allocation matters. See
[`Jumoo.Json.Benchmarks/README.md`](Jumoo.Json.Benchmarks/README.md) for the current baseline.

```bash
dotnet run -c Release --project Jumoo.Json.Benchmarks -- --filter '*'
```

Quote the `*` — in bash an unquoted `*` is expanded to filenames before BenchmarkDotNet sees it.

## Branching

One long-lived branch per Umbraco major, named `v{major}/main` — `v18/main` is current and is the
default branch. CI triggers on `*/main`, so new major branches are picked up automatically.

Work happens on a branch off `v{major}/main` and merges back via PR.

## Releasing

Pushes to `*/main` run [`package-build.yml`](.github/workflows/package-build.yml), which versions
with GitVersion, packs, and uploads the `.nupkg` as a workflow artifact. It does **not** publish —
pushing to a feed stays a deliberate manual step using the artifact from that run.

[`dist/build-package.ps1`](dist/build-package.ps1) is the older local path and still works for
building on a dev machine.

## Licence

[MPL-2.0](LICENSE)
