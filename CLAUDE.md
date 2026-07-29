# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`Jumoo.Json` is a small published NuGet library (~800 lines) wrapping `System.Text.Json` for Jumoo's
Umbraco packages. It is a *support* library — the consumers are Jumoo.TranslationManager and its
connectors, plus Jumoo.Processing. It is not a general-purpose JSON library.

It sits underneath bulk operations that serialize/compare/expand thousands of items, so **allocation
matters more than raw speed here**. Several public methods have been optimised specifically to keep
large payloads off the large object heap; see `Jumoo.Json.Benchmarks/README.md` for the baseline and,
importantly, a list of things that were measured and found *not* worth changing.

## Commands

```bash
dotnet build Jumoo.Json.slnx -c Release
```

```bash
dotnet test Jumoo.Json.slnx
```

Run a single test, or a whole test class:

```bash
dotnet test Jumoo.Json.Tests/Jumoo.Json.Tests.csproj --filter "FullyQualifiedName~IsJsonEqual_ReturnsTrue_ForSameReference"
```

```bash
dotnet test Jumoo.Json.Tests/Jumoo.Json.Tests.csproj --filter "FullyQualifiedName~JsonExpansionsTests"
```

Benchmarks — quote the `*`, or bash expands it to filenames before BenchmarkDotNet sees it:

```bash
dotnet run -c Release --project Jumoo.Json.Benchmarks -- --filter '*'
```

```bash
dotnet run -c Release --project Jumoo.Json.Benchmarks -- fixtures
```

The `fixtures` argument prints the generated test payloads and their sizes — useful for sanity
checking before a long run.

## Architecture

Everything is static extension methods over `System.Text.Json`. There is no DI, no state, and no
services. Three ideas hold it together:

**`JsonTextOptions` is the single source of truth.** Every entry point routes through
`JsonTextOptions.GetOptions(indent)`, which returns one of two shared static
`JsonSerializerOptions` instances (indented / flat). This is deliberate: consuming packages must all
read and write JSON identically. Never construct a local `JsonSerializerOptions` — take it from here.

**Property ordering is a feature, not a detail.** `OrderedPropertiesJsonResolver` sorts object
properties alphabetically so serialized output is stable. That means smaller diffs in the files
downstream packages write, and faster change detection. Do not "simplify" it away.

**Try\* methods must not throw and must not use exceptions as control flow.** A type mismatch is an
ordinary outcome that returns `false`/the default, not an exception. Several methods were rewritten
for exactly this reason (`GetPropertyValueOrDefault` went from 1,509 ns to 10 ns by switching
`GetValue<T>` for `TryGetValue<T>`). Before adding a `try`/`catch`, check whether a non-throwing
API exists.

### Things that look wrong but are intentional

- **`JsonDetection.LooksLikeJson`** duplicates Umbraco's `DetectIsJson` on purpose. Umbraco's calls
  `string.Trim()`, which copies the whole payload — 416 KB of garbage on a 200 KB value — before
  anything has been parsed. The local version trims over a span.
- **`TryExpandJsonNodeValue` returns the *original* node when there is nothing to expand**, rather
  than always a clone. That is what makes skipping the `DeepClone` possible (2,186 KB → 399 KB on a
  200 KB document). It means callers must not mutate the result unless something was expanded.
- **`PooledByteBufferWriter`** exists so `IsJsonEqual` can compare UTF-8 bytes from pooled buffers
  instead of materialising two large strings. Keeps big comparisons entirely off the LOH.
- **`ExpandNode` only replaces a string node when the string genuinely parses as JSON.** Rebuilding
  equivalent nodes for every plain string was most of the cost of expansion.

### Umbraco coupling

Only two files reference Umbraco:

- `JsonSerialization.cs` — `Umbraco.Extensions.TryConvertTo<T>()`, used once in `TryGetValueAs`
- `JsonTextOptions.cs` — five converters from `Umbraco.Cms.Infrastructure.Serialization`

Of those converters, `JsonObjectConverter` and `JsonBooleanConverter` are pure `System.Text.Json`,
but `JsonUdiConverter`, `JsonUdiRangeConverter` and `JsonBlockValueConverter` need Umbraco domain
types. They are load-bearing — TranslationManager serializes `BlockValue` through this library.

Removing the Umbraco dependency has been investigated and **deliberately parked**: it would require
splitting into `Jumoo.Json` + `Jumoo.Json.Umbraco`, and every consumer is an Umbraco package anyway,
so the breaking change isn't worth it yet.

**`JsonTextOptions.AddConverter` rebuilds the options rather than mutating them**, because
`System.Text.Json` makes `JsonSerializerOptions` read-only the first time it is used — adding to
`Converters` in place threw `InvalidOperationException` for anything registering after the first
serialize. Two consequences: it is a startup-time call (each rebuild discards the cached type
metadata), and anything holding an instance from an earlier `GetOptions()` call keeps the old one.

## Build and release

Shared settings live in `Directory.Build.props` (target framework, package metadata, symbols, XML
docs) and `Directory.Packages.props` (central package management — the individual `.csproj` files
carry no versions). The SDK is pinned in `global.json`.

Note `Directory.Build.props` deliberately shadows one further up the disk that set `NuGetAuditMode`;
that setting is carried over explicitly so local and CI builds agree.

### Branching

One long-lived branch per Umbraco major, named `v{major}/main`. **`v18/main` is the default branch,
not `main`.** Workflows trigger on `*/main` so new major branches are picked up automatically — a
workflow targeting `main` will silently never run.

Work on a branch off `v{major}/main` and merge back via PR.

### Workflows

| Workflow | Trigger | Does |
| --- | --- | --- |
| `dotnet-build.yml` | PR to `*/main` | restore `--locked-mode`, build, test, upload coverage |
| `package-build.yml` | push to `*/main`, or manual | GitVersion, build, test, pack, upload `.nupkg` artifact |
| `benchmarks.yml` | manual only | full benchmark run, uploads results |
| `codeql.yml` | PR + push to `*/main`, weekly | code scanning for `csharp` and `actions` |

`package-build.yml` intentionally has **no `dotnet nuget push`** — it produces an artifact for
review, and publishing stays a deliberate manual step. `dist/build-package.ps1` is the older local
packaging path.

Because `package-build.yml` only runs on `*/main`, changes to it can't be validated by a PR build.
Test it against a branch before merging:

```bash
gh workflow run "Build and Package" --ref your-branch
```

### Lock files

Restores are locked. If you change a dependency, regenerate all three lock files and commit them:

```bash
dotnet restore Jumoo.Json.slnx --force-evaluate
```

This matters most when bumping Umbraco. `Umbraco.Cms.Web.Common` is the only package that flows
through the project reference into the test and benchmark projects as a `CentralTransitive` entry,
so editing `Directory.Packages.props` alone leaves those two lock files stale and CI fails with
`NU1004`.

### Dependabot exclusions

Two `ignore` entries in `.github/dependabot.yml`, both with reasons that aren't obvious:

- **`Umbraco.*`** — the version here becomes the *minimum* for every downstream package, so it moves
  when we choose. Note this also opts Umbraco out of dependabot *security* updates.
- **`gittools/actions`** — v4 of the action requires GitVersion ≥ 6.2 while `GitVersion.yml` is v5
  schema. Dependabot can't see that the action version and the `versionSpec` input are coupled, and
  bumping it broke the release pipeline once. Unpinning means migrating the config
  (`mode` → `deployment-mode`, `tag` → `label`).

## Packaging gotcha

MSBuild silently ignores unrecognised properties, and this package shipped for two versions with no
readme and no project URL because the csproj used `ProjectUrl` and `PackageReadme` instead of
`PackageProjectUrl` and `PackageReadmeFile`.

**Verify packaging changes against the artifact, never the csproj:**

```bash
dotnet pack Jumoo.Json/Jumoo.Json.csproj -c Release -o ./pack-check
```

Unzip the `.nupkg` and check the generated `.nuspec` has what you expect, and that `lib/net10.0/`
contains both `Jumoo.Json.dll` and `Jumoo.Json.xml`.

## Repository constraints

The repository is public, which is what makes code scanning and branch protection available at all
— both returned 403 while it was private on this plan.

CodeQL uses `build-mode: none`, so it reads the source without compiling. That means it needs no SDK
setup, no restore, and doesn't interact with the locked-mode restore the other workflows use.
