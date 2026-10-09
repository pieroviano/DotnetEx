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

Each library has an xUnit test project under `tests/`:

| Test project | Covers | Tests |
|---|---|---|
| `tests/Net35.MsCorLib.Tests/` | `Net35.MsCorlib` | 154 |
| `tests/Net35.System.Buffers.Tests/` | `Net35.Buffers` | 25 |
| `tests/Net35.System.Runtime.InteropServices.RuntimeInformation.Tests/` | `Net35.Runtime.InteropServices.RuntimeInformation` | 20 |

### The test projects target net48 but test the net3.5-client assemblies

xUnit and the VSTest adapter have no `net35` assets, so the suites target **net48** while still
compiling and running against the libraries' `net3.5-client` output. Each one therefore has
**no `ProjectReference`** - it binds the built assembly directly:

```xml
<Reference Include="Net35.MsCorlib" Aliases="global,shim">
  <HintPath>..\..\src\mscorlib.NET35\bin\$(Configuration)\net3.5-client\Net35.MsCorlib.dll</HintPath>
  <Private>true</Private>
</Reference>
```

Consequences worth knowing before editing a test project:

- **Transitive libraries must be named explicitly.** A bare `Reference` carries no project graph, so
  the RuntimeInformation suite lists `Net35.MsCorlib` as well. A missing one is a run-time
  `FileNotFoundException`, not a build error.
- **Build order comes from the `.sln`.** A `HintPath` carries none, so each test project has a
  `ProjectSection(ProjectDependencies)` naming the libraries it binds. Keep it in sync.
- **Keep `$(Configuration)` in the `HintPath`** so a Release build picks up the Release assembly.
- **`GenerateTargetFrameworkAttribute` is `false`**, and `IsTestProject` is `true`.
- Every shim type that net48 also defines is named through the `shim` alias, so the test exercises the
  backport. See *Test conventions* below.

## Two parallel csproj sets — read before editing project files

Each `src/` directory contains **two .csproj files for the same sources**:

- `Net35.*.csproj` → referenced by **`Net35.Buffers.sln`**, the only solution in the repo. This is
  the active fork: packable, versioned from `Directory.Build.Props`, uses `Net4x.NuGetUtility`.
- `Polyfill.*.csproj` → the legacy upstream layout (`DotnetEx.sln`, which is no longer checked in).
  Left on disk, referenced by nothing, not maintained here.

Work against `Net35.Buffers.sln` and the `Net35.*` csproj unless told otherwise. Because two
project files share a directory, **never run a build/restore by pointing at a folder** — it is
ambiguous. Always name the `.sln` or the specific `.csproj`.

`src/mscorlib.NET40/`, `src/System.Buffers.NET40/` and
`src/System.Runtime.InteropServices.RuntimeInformation.NET40/` are legacy NET40 siblings, **not**
part of `Net35.Buffers.sln`.

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

1. Build `Net35.Buffers.sln` with desktop MSBuild (above). `dotnet build` still cannot build the
   libraries (ResGen), so build first, then run the tests with `--no-build`.
2. Run the three xUnit suites. Since they target `net48`, the adapter works and so does coverage:

```bash
dotnet test Net35.Buffers.sln --no-build -c Debug
```

   Or VSTest directly against a built assembly:

```bash
VSTEST="/c/Program Files/Microsoft Visual Studio/18/Enterprise/Common7/IDE/Extensions/TestPlatform/vstest.console.exe"
"$VSTEST" tests/Net35.MsCorLib.Tests/bin/Debug/net48/Net35.MsCorLib.Tests.dll
```

   199 tests, all passing: 154 + 25 + 20, 0 skipped on Windows. A runner reporting *zero* tests with
   exit code 0 is a broken run, not a pass.

   (In Git Bash pass runner options with `-`, not `/` - MSYS rewrites `/nologo` into a path. Quoted
   heredocs also collapse `\\`, so write throwaway scripts to a file rather than piping them in.)

### Test conventions

- **Namespaces avoid segments that shadow the type under test.** `Net35.RuntimeInformation.Tests`
  makes `RuntimeInformation.OSDescription` resolve to the *namespace*, not the class, so the
  RuntimeInformation suite uses `Net35.InteropServices.Tests`. Watch for this when adding a project.
- **A shim type that net48 also defines is named through the `shim` alias**, otherwise the test
  silently exercises the BCL. `extern alias shim;` at the top of the file, then either a type alias
  (`using Volatile = shim::System.Threading.Volatile;`, which needs no call-site change) or, for a
  generic - C# cannot alias an open generic - a namespace alias plus qualified sites
  (`shimcol.IReadOnlyList<T>`). This covers `DnsEndPoint`, `SpinWait`, `Volatile`, `IProgress`,
  `Progress`, `IReadOnly*`, `RuntimeInformation`, `OSPlatform` and `Architecture`. Note the compiler
  does **not** always report the collision, so after a change confirm the binding by disassembly
  (`ildasm /out=… /nobar`) and check the typeref says `[Net35.MsCorlib]`, not `[mscorlib]`.
- **A backported extension method whose name matches a real net48 instance member must be called in
  static form.** `StringBuilderEx.Clear(builder)`, never `builder.Clear()` - the instance member
  always wins, so the implicit call never reaches the shim. An extension *property* with a BCL
  namesake is unreachable on net48 altogether; only an old-TFM test project could cover one.
- **Windows-only tests carry `[WindowsOnlyFact]`**, a `FactAttribute` subclass that sets `Skip` when
  the platform is not Windows. xUnit 2.x cannot skip from inside a test body, so there is no
  `Assert.Ignore` equivalent to call. There is one copy per test project, deliberately.
- Several tests deliberately pin *incorrect* behaviour that the polyfills currently ship (notably the
  transposed `paramName`/`message` arguments in every `ArgumentNullExceptionEx` constructor). They
  carry a `[Trait("Description", …)]` saying so. If you fix the underlying defect, update the paired
  test rather than assuming the suite has regressed.
