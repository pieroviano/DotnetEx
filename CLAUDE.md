# CLAUDE.md

Guidance for Claude Code when working in this repository.

## What this repo is

A set of **polyfill libraries that back-port modern BCL types to .NET Framework 3.5**
(`net3.5-client`). Fork of `wherewhere/DotnetEx` (origin: `pieroviano/DotnetEx`), repackaged
under the `Net35.*` package family. Most code is ported verbatim from `dotnet/runtime` and keeps
the .NET Foundation MIT header.

Three shipping projects, all in `src/`:

| Project | Assembly | Provides |
|---|---|---|
| `mscorlib.NET35/Net35.MsCorlib.csproj` | `Net35.MsCorlib` | `IProgress`/`Progress`, `IReadOnly{Collection,Dictionary,List,Set}`, `DnsEndPoint`, `SpinWait`, `Volatile`, plus `*Ex` helper types |
| `System.Buffers.NET35/Net35.System.Buffers.csproj` | `Net35.Buffers` | `System.Buffers.ArrayPool<T>` (no ETW logging) |
| `System.Runtime.InteropServices.RuntimeInformation.NET35/Net35.System.Runtime.InteropServices.RuntimeInformation.csproj` | `Net35.Runtime.InteropServices.RuntimeInformation` | `RuntimeInformation`, `OSPlatform`, `Architecture` |

Each library has an NUnit test project beside it at the repository root - note these sit at the
root, **not** under `tests/`:

| Test project | Covers | Tests |
|---|---|---|
| `Net35.MsCorLib.Tests/` | `Net35.MsCorlib` | 154 |
| `Net35.System.Buffers.Tests/` | `Net35.Buffers` | 25 |
| `Net35.System.Runtime.InteropServices.RuntimeInformation.Tests/` | `Net35.Runtime.InteropServices.RuntimeInformation` | 20 |

`tests/DotnetEx.Test.NET35/` is the original console `Exe` smoke test (`Program.cs`). It ends in
`Console.ReadKey()`, so never run it in a non-interactive shell.

## Two parallel csproj sets — read before editing project files

Each `src/` directory contains **two .csproj files for the same sources**:

- `Net35.*.csproj` / `Net35.Buffers.Test.NET35.csproj` → referenced by **`Net35.Buffers.sln`**. This is
  the active fork: packable, versioned from `Directory.Build.Props`, uses `Net4x.NuGetUtility`.
- `Polyfill.*.csproj` / `DotnetEx.Test.NET35.csproj` → referenced by the legacy **`DotnetEx.sln`**
  (upstream layout, also carries the `*.NET40` projects). Not maintained here.

Work against `Net35.Buffers.sln` and the `Net35.*` csproj unless told otherwise. Because two
project files share a directory, **never run a build/restore by pointing at a folder** — it is
ambiguous. Always name the `.sln` or the specific `.csproj`.

`src/mscorlib.NET40/`, `src/System.Buffers.NET40/`,
`src/System.Runtime.InteropServices.RuntimeInformation.NET40/` and `tests/DotnetEx.Test.NET40/`
are legacy NET40 siblings, **not** part of `Net35.Buffers.sln`.

## Building

`dotnet build` **does not work** on this repo:

```
error : ResGen.exe not supported on .NET Core MSBuild
```

The `.resx` resources for a `net3.5-client` target require desktop MSBuild. Use full MSBuild:

```bash
MSBUILD="C:/Program Files/Microsoft Visual Studio/18/Enterprise/MSBuild/Current/Bin/MSBuild.exe"
"$MSBUILD" Net35.Buffers.sln -t:Restore,Build -p:Configuration=Debug -v:m -nologo
```

(Locate it portably with
`"C:/Program Files (x86)/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild/**/Bin/MSBuild.exe"`.)

`dotnet restore` on the solution does work and is enough to refresh `project.assets.json`.

### Every build produces NuGet packages

All three `Net35.*` projects set `GeneratePackageOnBuild=True` and
`PackageOutputPath=$(SolutionDir)Packages\`. `Packages` is a **symlink to `/d/starb/Packages`**, a
local NuGet feed shared with the other repos under `D:\CommonLibrary` (it also holds `Gtk.*`
packages from a sibling repo). `NuGet.Config` registers it as the `Local` source. So a plain Debug
build writes `.nupkg`/`.snupkg` into that shared feed — expect it, and don't clean the folder.

### Versioning

`Directory.Build.Props` → `VersionPrefix = $(Net35BuffersVersion)` (defined as `1.0.0` in
`Directory.Nuget.Props`), `VersionSuffix = <yy><dayOfYear>`. Projects compose
`Version = $(VersionPrefix).$(VersionSuffix)`, i.e. a four-part `1.0.0.26240`, not a prerelease tag.
Bump `Net35BuffersVersion` in `Directory.Nuget.Props` to change the release version.

`Directory.Build.Props` imports `Directory.NuGet.props`; the file on disk is `Directory.Nuget.Props`
— this only resolves because the filesystem is case-insensitive. Keep it that way on Windows.

The `Net4x.NuGetUtility` imports at the top and bottom of each `Net35.*.csproj` are **conditional on
the package already existing in `~/.nuget/packages`** — a first build without it silently skips
those settings (which include Obfuscar on Release builds). Don't "fix" the conditions.

## Code conventions

- **`Ex` suffix for collisions.** A polyfill whose type already exists in .NET 3.5 gets an `Ex`
  suffix and lives as a separate static/derived type: `StringEx.Join`, `EnvironmentEx.Is64BitProcess`,
  `OperatingSystemEx`, `EnumEx.TryParse`, `StringBuilderEx.Clear`, `ArgumentNullExceptionEx`,
  `EventHandlerEx<T>`. Types genuinely absent from 3.5 use their **real** name and namespace
  (`ArrayPool<T>`, `RuntimeInformation`, `IReadOnlyList<T>`, `Volatile`, `SpinWait`) so consumer code
  compiles unchanged.
- **Modern C# syntax, 3.5 API surface.** `LangVersion` is `latest`/`preview`, so target-typed `new()`,
  `is`/`or` patterns, string interpolation and `using` declarations are fine. But the *runtime* is 3.5:
  no `Array.Empty<T>()`, no `MethodImplOptions.AggressiveInlining` (written as `(MethodImplOptions)256`),
  no `Volatile` unless the polyfill is referenced. Follow the workarounds already in the ported files.
- **Nullable annotations without `#nullable enable`.** `string?` is used throughout; this emits
  `CS8632` warnings by design. Builds are also noisy with `CS1591` (missing XML docs) because
  `GenerateDocumentationFile=True`. Both are pre-existing — don't chase them, and don't judge a build
  by warning count.
- **Ported code stays close to upstream.** When touching a file with the .NET Foundation header, keep
  its structure and comments; make the minimal change needed for the 3.5 target rather than modernizing.
- **Resources.** Each project has `Resources/Strings.resx` + a generated `Strings.Designer.cs` with a
  per-project `CustomToolNamespace` (`System.Resources`, none, or
  `System.Runtime.InteropServices.Resources`). Add strings to the `.resx` and let the ResX generator
  rewrite the designer; don't hand-edit `Strings.Designer.cs`.

## Verifying a change

1. Build `Net35.Buffers.sln` with desktop MSBuild (above).
2. Run the three NUnit suites. **`dotnet test` does not run them**: the test projects target `net35`,
   and VSTest silently executes nothing for a `net35` assembly - it prints `Build succeeded` and no
   test run at all, so a green `dotnet test` here means nothing was executed. There is also no test
   adapter in the projects (`NUnit` 2.6.1 alone provides none), and `coverlet.collector` is a VSTest
   data collector, so it cannot produce coverage for these targets either.

   Use the NUnit 2.x console runner against the built assemblies:

```bash
RUNNER=~/.nuget/packages/nunit.runners/2.7.0/tools/nunit-console.exe
"$RUNNER" Net35.MsCorLib.Tests/bin/Debug/net35/Net35.MsCorLib.Tests.dll -nologo -noshadow
```

   (In Git Bash pass options with `-`, not `/` - MSYS rewrites `/nologo` into a path.)

   To make `dotnet test` and coverage work, the projects would need NUnit 3.14 plus
   `NUnit3TestAdapter`, which is exactly what the repo owner's own `Net4x.NunitTests` package pins for
   `.NETFramework3.5`. The tests are written against the classic assertion API
   (`Assert.AreEqual`/`IsTrue`/`Throws<T>`, `StringAssert`, `CollectionAssert`) so they compile
   unchanged on both NUnit 2.6.1 and NUnit 3.x.

3. The old `DotnetEx.Test.NET35` smoke test still exists; it blocks on `Console.ReadKey()`, so run it
   only in an interactive terminal (`! <path>` in Claude Code).

### Test conventions

- Namespaces avoid segments that shadow the type under test. `Net35.RuntimeInformation.Tests` makes
  `RuntimeInformation.OSDescription` resolve to the *namespace*, not the class, so the
  RuntimeInformation suite uses `Net35.InteropServices.Tests`. Watch for this when adding a project.
- Windows-only tests call a local `RequireWindows()` helper that invokes `Assert.Ignore`, rather than
  failing off-platform.
- Several tests deliberately pin *incorrect* behaviour that the polyfills currently ship (notably the
  transposed `paramName`/`message` arguments in every `ArgumentNullExceptionEx` constructor). They
  carry a `[Description]` saying so. If you fix the underlying defect, update the paired test rather
  than assuming the suite has regressed.
