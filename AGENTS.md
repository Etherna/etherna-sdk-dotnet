# Etherna SDK .Net

Etherna SDK .Net is a set of **.NET client libraries** (NuGet packages) to consume the Etherna services APIs (Credit, Gateway, Index, SSO) — it is a library solution, not a runnable application. NSwag-generated HTTP clients are wrapped behind clean, hand-written client interfaces; companion `Tools.*` packages provide utilities for Etherna videos, universal file/URI handling, and blockchain tokens.

## Build, run, test

Source projects multi-target **net9.0 and net10.0** (test projects target net10.0 only). There is no `Directory.Build.props`: every `.csproj` carries its own full property block — `TreatWarningsAsErrors=true`, `AnalysisMode=AllEnabledByDefault`, `Nullable=enable`, `EnableNETAnalyzers=true` (and `IsAotCompatible=true` on the client packages) — warnings break the build. When adding a project, copy the property block from an existing one.

```bash
dotnet restore EthernaSdk.sln
dotnet build EthernaSdk.sln -c Release
dotnet test  EthernaSdk.sln -c Release              # runs all xUnit test projects
dotnet test test/EthernaSdk.Tools.Video.UnitTests/EthernaSdk.Tools.Video.UnitTests.csproj  # single project
dotnet test --filter "FullyQualifiedName~BasicUUriTest"                                    # single class
dotnet test --filter "FullyQualifiedName~BasicUUriTest.GetUriKind"                         # single test
```

There is nothing to "run": to exercise the clients use the projects under `samples/` (`ConsoleUserClientSample`, `ConsoleInternalClientSample`, `AspNetCoreInternalClientSample`).

`nuget.config` adds the `ethernaNuget` feed (https://nuget.etherna.io/v3/index.json, Etherna's own NuGet server) next to nuget.org — prerelease dependencies like `SwarmSdk`/`SwarmSdk.Client` and `Etherna.Authentication.Native` come from there. Versioning is automatic via **GitVersion** (`mode: ContinuousDeployment`, `GitVersion.MsBuild` in every package project, plus SourceLink). CI (`.github/workflows/`): pushes to `dev` and `release/**` build, test, pack and push unstable packages to nuget.etherna.io; tags `v*.*.*` push stable packages to NuGet.

## Architecture

Solution `EthernaSdk.sln`. Project folders are named `EthernaSdk.*` but root namespaces use `Etherna.Sdk.*`; under the root, namespace mirrors the folder path.

- **`src/EthernaSdk.{Credit,Gateway,Index,Sso}.Common`** (`Etherna.Sdk.<Service>`) — Each contains only the NSwag-generated client under `GenClients/` (namespace `Etherna.Sdk.<Service>.GenClients`, file `NSwagEtherna<Service>Client.cs`). Generated code: never edit by hand, regenerate instead (see below).
- **`src/EthernaSdk.UsersCommon`** (`Etherna.Sdk.Users`) — The user-clients builder (`IEthernaUserClientsBuilder`/`EthernaUserClientsBuilder`) and the `ServiceCollectionExtensions` entry point `AddEthernaUserClients`. Authentication comes from `Etherna.Authentication.Native`, which registers both sign-in flows together ("oauth code" and "api key", with access-token expiration management); the flow is chosen only at sign-in time via the `IEthernaSignInService.SignInAsync()` / `SignInAsync(apiKey)` overloads, with no registration-time commitment.
- **`src/EthernaSdk.Users.{Credit,Gateway,Index,Sso}`** (`Etherna.Sdk.Users.<Service>`) — The public per-service clients for user applications. `Clients/` holds the interface + implementation pair (`IEthernaUserSsoClient`/`EthernaUserSsoClient`, …); `Models/` holds the public wrapper models; `Extensions/EthernaUserClientsBuilderExtensions.cs` adds the per-service registration (`AddEthernaCreditClient`, `AddEthernaGatewayClient`, `AddEthernaIndexClient`, `AddEthernaSsoClient`), declared in the `Etherna.Sdk.Users` namespace via `// ReSharper disable CheckNamespace` so all registrations are reachable from one using. `Users.Gateway` also has `Services/GatewayService` (higher-level gateway operations, e.g. postage batch creation/waiting), with an optional `dryMode` on `AddEthernaGatewayClient`. `AddEthernaGatewayClient` builds the `ISwarmClient` over the shared named `HttpClient`, lifting the timeout of that one instance to infinite: the `SwarmClient` bounds every call by the class of its operation (`SwarmClientTimeouts`, overridable through the `timeouts` parameter) and refuses an `HttpClient` with a shorter timeout, while the REST clients keep the consumer's timeout. A node that misses its bound surfaces as `SwarmClientTimeoutException` (a `TimeoutException`).
- **`src/EthernaSdk.Internal`** (`Etherna.Sdk.Internal`) — Service-to-service clients (`EthernaInternalCreditClient`, `EthernaInternalSsoClient`) for internal worker applications only, with their registration: `AddEthernaInternalClients` + `IEthernaInternalClientsBuilder`, usable by any host (ASP.NET Core, worker, console). The "client credentials" flow comes from `Etherna.Authentication.ClientCredentials`, which registers the token management and builds the token endpoint from the SSO base url. Each internal client has its own token client and its own named `HttpClient` (the `EthernaInternalCreditHttpClientName` and `EthernaInternalSsoHttpClientName` constants of `EthernaInternalClientsBuilder`): a shared one would carry the token handlers of both, and send to both services the token of the client registered last. Keep it so when adding a client.
- **`src/EthernaSdk.Tools.Video`** (`Etherna.Sdk.Tools.Video`) — Tools to build, serialize and parse Etherna video manifests (`VideoManifestService`, `HlsService`); versioned manifest DTOs under `Serialization/Dtos/` (`Manifest1/`, `Manifest2/`, `PersonalData1/`). A published manifest is read through `Services/SwarmResourceReader`: file contents (preview, details, HLS playlists) go through the `IUFileProvider` when the caller passes one — a Swarm file is then served whole by the bzz endpoint, one request per file — and chunk by chunk through the chunk store otherwise; entry references (sources, thumbnails, captions, HLS segments) are always resolved through the chunk store, walking each mantaray manifest once per read.
- **`src/EthernaSdk.Tools.UniversalFiles`** (`Etherna.Sdk.Tools.UniversalFiles`) — The `UUri`/`UFile` abstraction over local, online and Swarm resources (`UUriKind` flags, `UFileProvider`).
- **`src/EthernaSdk.Tools.Tokens`** (`Etherna.Sdk.Tools.Tokens`) — Blockchain token utilities on Nethereum (e.g. `UniswapService` pool price reads).
- **`test/`** — xUnit + Moq: `EthernaSdk.Internal.Tests`, `EthernaSdk.Tools.UniversalFiles.Tests`, `EthernaSdk.Tools.Video.UnitTests`, `EthernaSdk.Users.Gateway.Tests`.

### Key cross-cutting points

- **Generated clients are never exposed.** The adapter pattern is the central design: public clients wrap the NSwag `GenClients` types behind hand-written interfaces, and generated DTOs are converted into public wrapper models with `internal` constructors (e.g. `PrivateUserInfo(PrivateUserDto)`). Access across assemblies is granted by `[InternalsVisibleTo]` in `Properties/AssemblyInfo.cs` (the `*.Common` projects expose internals to the `Users.*`/`Internal` projects, and the `Tools.*` projects to their test projects) — extend those attributes when a new project needs them. Assemblies also declare `[CLSCompliant(false)]`.
- **Regenerating clients.** Run `nswag run tools/etherna-<service>-api.nswag` (NSwagStudio on Windows, NSwag CLI elsewhere) against a locally running instance of the service — the OpenAPI `url` in each `.nswag` file points to the service's localhost dev port. Output goes to the matching `*.Common/GenClients/` folder. Watch out: `tools/etherna-credit-api.nswag` still has the pre-rename output path `src/EthernaSdk.Credit/` — make sure the regenerated file lands in `src/EthernaSdk.Credit.Common/GenClients/`.
- **Dry mode is completed by `GatewayService`.** `dryMode` on `AddEthernaGatewayClient` reaches only the `SwarmClient`, whose writes become no-ops. `GatewayService` reads `ISwarmClient.IsDryMode` to keep dry also the writes it routes to the REST client (the batch purchase, the download funding and defunding): a new write routed there must do the same. `IEthernaUserGatewayClient` used directly has no dry mode.
- **A batch purchase is two waits.** `GatewayService.BuyPostageBatchAsync` first waits for the batch to be created, with the gateway only: the purchase answers with a reference, read until the gateway resolves it to the batch id. No batch id yet (`TryGetNewPostageBatchIdFromPostageRefAsync` returns `null` while the purchase is pending, or when the reference is unknown) and any error of the gateway count as "not created yet", except the `410` of a purchase the gateway gave up, which propagates. Then it waits for the batch to be usable, through SwarmSdk's `PostageBatchUsablePoller`: a read that misses its bound, a `404` and a `504` of the node count as "not usable yet". Both waits read every `BatchCheckTimeSpan`, are cancelled by the caller's token, throw `TimeoutException` when their timeout elapses (`BatchCreationTimeout`, `BatchUsableTimeout`), and measure time through the `TimeProvider` of the service, an optional constructor parameter that the tests replace to run without real delays.
- **Logging** uses strongly-typed `LoggerMessage.Define` delegates in each project's `Extensions/LoggerExtensions.cs` (internal), grouped by level and alphabetical within the group, with incremental never-reused event ids, per project; the header comment `Last event id is: N` is the source of truth for the next free id — update it when adding a delegate. Add an event there, never an inline `logger.LogX` call. A logger is an optional last constructor parameter (`ILogger<T>? logger = null`, replaced by `NullLogger` when missing): the container injects it into the services it builds, the `AddEtherna…Client` factories pass it to the clients they build with `new`, and a consumer building a type by hand gets no logs until it passes one — a required parameter would break those callers. `UFileProvider` shares its logger with the files it builds, so their events carry its category. Levels: Information for an action with an effect outside the process, or one that dry mode skipped (a postage batch purchase requested, created, usable); Debug for the steps of a wait and for what an operation produced (a batch not created or not usable yet, a manifest written or read, a file copied, an online request refused); Warning for a failure the code tolerates instead of throwing (a failed read of a batch reference, a video left out of the previews, a failed online or Swarm request). A failed read of the batch during the usable wait is Debug: the SwarmSdk poller decides whether to tolerate it, and it reaches the caller when it does not. What is thrown to the caller is not logged: the caller owns it. An online file uri is logged without its user info and query, which can carry credentials. The REST calls are not logged by the SDK: the `HttpClient` factory already logs each request on its `System.Net.Http.HttpClient.<name>` categories.
- **This is a library: always `ConfigureAwait(false)`** on every await. This is the opposite of the Etherna ASP.NET Core service repos (beehive, credit, index, sso), where `ConfigureAwait` is omitted.
- **Public API surface is the product.** These packages are published to NuGet: keep the public surface intentional, document the client interfaces with XML docs, and remember both target frameworks must compile.
- **The license is GNU LGPL v3 or later**, stated by the `COPYING` + `COPYING.LESSER` text pair the GitHub detector reads as LGPL-3.0 (it recognises the lesser terms by that exact file name), and declared to the package registries by the `PackageLicenseExpression` of every packable `.csproj` — `LGPL-3.0-or-later`, the SPDX form of the source header wording, mutually exclusive with `PackageLicenseFile`, which no project sets (NuGet accepts a single license file anyway, and a file carries no machine readable identifier). Both texts are packed as package content.
- **Package icon**: every packable `.csproj` declares `PackageIcon` over the root `icon.png`, packed as content beside the license texts and the readme. It is the Etherna symbol (the eye/node, without lettering: these packages carry the company brand, having no product mark of their own), cyan `#00ABBF` on a transparent 256 px square canvas — the same file, byte for byte, that Etherna Authentication ships, so the two families show one icon. The file is derived from the vector symbol of the brand kit (`logo-symbol.svg`, https://info.etherna.io/brand-kit/): a change to the artwork regenerates it from there for both repositories, never by editing the derived file.
- **Readme images**: `README.md` is packed as the readme of every package, and nuget.org renders only images hosted on its allowed domains (`img.shields.io`, `raw.githubusercontent.com`...): relative paths and other hosts are dropped, with a warning shown to the package owners. Every image in the readme is an absolute URL on an allowed host; the LGPL logo is the copy in `doc/lgplv3-with-text-154x68.png`, referenced through `raw.githubusercontent.com` on `main`.

## Issue tracker

Bugs and features are tracked in Jira project **ESC** (https://etherna.atlassian.net/projects/ESC). Branch names follow `feature/ESC-<id>-<slug>` / `improve/ESC-<id>-<slug>` / `fix/ESC-<id>-<slug>` — match this when creating branches. `dev` is the integration branch, `main` is stable; stable releases are tagged `v<version>` (e.g. `v0.3.3`).

# Coding Style

## General Principles

- Keep commits clean: only include changes strictly necessary for the task at hand.
- Keep `README.md` aligned: when a change touches packages, features, or build steps, update `README.md` in the same change. Its badges and packages tables name every published package, and its code snippets are compiled against the `src/` projects before they are committed, since no build covers them.
- A security fix keeps its commit message and PR title neutral: the Jira key and what changes, never the vulnerability. The write-up stays in the Jira work item, hidden until the fixed version is released; outside reporters are directed to [SECURITY.md](SECURITY.md).
- Never reference AI agents or assistants in commits or code — no agent names, no `Co-Authored-By` agent trailers, no "generated/assisted by" notes. Commit messages and code must read as the team's own work.
- Exceptions to these conventions are accepted when strictly necessary or when they significantly improve code quality. Justify with a comment where needed.
- All elements (usings, properties, methods, fields, enum members, etc.) are always alphabetically ordered within their respective sections.
- Primary constructors are preferred everywhere the constructor is a simple parameter assignment.
- Keep code clean: remove unused variables, dead code, and redundant imports.
- Don't extract a private helper method for logic used in a single place — inline it. Reserve helpers for code shared by two or more call sites (or when extraction materially clarifies an otherwise long, complex method).
- Every source file starts with the standard LGPL-3.0 copyright header (`// Copyright 2020-present Etherna SA` … see any existing file). The Razor pages of the samples carry the same text in a `@* … *@` comment.

## Naming

- **Classes/Structs**: PascalCase (`EthernaUserSsoClient`, `VideoManifest`, `GatewayService`)
- **Interfaces**: `I` prefix (`IEthernaUserSsoClient`, `IGatewayService`, `IUFileProvider`)
- **Async methods**: always `Async` suffix (`GetPrivateUserInfoAsync`, `BuyPostageBatchAsync`)
- **Properties**: PascalCase (`Balance`, `AspectRatio`, `IsUnlimited`)
- **Private fields**: `_camelCase` only when backing a same-named property; otherwise plain `camelCase`
- **Primary constructor parameters**: `camelCase` without underscore
- **Constants**: PascalCase (`BzzDecimals`, `DetailsManifestFileName`, `BatchCheckTimeSpan`)
- **Enums**: PascalCase type and members (`ImageType.Avif`, `UUriKind.LocalAbsolute`)
- **Namespaces**: `Etherna.Sdk.<Module>.<Feature>` (e.g. `Etherna.Sdk.Users.Sso.Clients`)
- **Wrapper models**: public class, `internal` constructor taking the generated DTO
- **Generated clients**: `NSwagEtherna<Service>Client` in `GenClients/`, namespace `Etherna.Sdk.<Service>.GenClients`
- **Builder classes**: descriptive suffix (`EthernaUserClientsBuilder`, `EthernaInternalClientsBuilder`)

## Code Organization

- One class per file, filename matches class name
- Namespace mirrors folder structure exactly (under the `Etherna.Sdk` root namespace)
- Block-scoped namespaces: `namespace X { ... }` — NOT file-scoped
- Using directives at the top of the file, before the namespace block, always alphabetically ordered and kept to the minimum necessary
- No global usings — each file declares its own imports
- `// ReSharper disable CheckNamespace` when the declared namespace intentionally differs from the file path (the `EthernaUserClientsBuilderExtensions` files)

## Comments

Principal comments (generally multiline, important):
```csharp
// Capital start, ending period.
// Continued on next line if needed.
```

Secondary/separator comments:
```csharp
//no space, no capital, no ending period
```

## Member Ordering Within a Class

Use principal-style section comments (singular `// Constructor.` when there is only one):

```csharp
// Consts.
// Fields.
// Constructors.
// Static properties.
// Properties.
// Static methods.
// Methods.
// Protected methods.
// Helpers.
```

## Class Design

- `sealed` for concrete implementations not meant for inheritance (`public sealed class GatewayService`, `internal sealed class Manifest2DetailsDto`)
- `abstract` for base implementations (`VideoEncodingBase`, `VideoVariantBase`, `UUri`)
- `internal` constructors on wrapper models for controlled creation from generated DTOs
- Primary constructors everywhere the constructor is a simple assignment
- Adapter/wrapper pattern: wrap NSwag-generated clients behind clean interfaces
- Builder pattern for DI setup

## Async Patterns

- Always suffix with `Async`
- `CancellationToken cancellationToken = default` as the optional last parameter, always propagated to the generated client calls
- **Always use `ConfigureAwait(false)`** — this is a library:
  ```csharp
  public async Task<PrivateUserInfo> GetPrivateUserInfoAsync(CancellationToken cancellationToken = default) =>
      new(await generatedClient.IdentityAsync(cancellationToken).ConfigureAwait(false));
  ```
- Return `Task` or `Task<T>`, never `async void`

## Null Handling

- Nullable reference types enabled
- `ArgumentNullException.ThrowIfNull()` for parameter validation
- `is null` / `is not null` pattern matching
- Prefer `null` over `default` wherever the type admits it: optional parameter defaults, late-init member initializers (`= null!`, not `= default!`), returns and assignments. Keep `default` only where `null` can't apply: non-nullable value types (e.g. `CancellationToken cancellationToken = default`) and unconstrained generic type parameters.
- `??` and `??=` operators

## XML Documentation

Expected on the public client interfaces (`IEthernaUser*Client`, `IEthernaInternal*Client`); not compiler-enforced elsewhere:
```csharp
/// <summary>
/// Get current user private information.
/// </summary>
/// <param name="cancellationToken">A cancellation token.</param>
/// <returns>Current user private information</returns>
/// <exception cref="EthernaSsoApiException">A server side error occurred.</exception>
Task<PrivateUserInfo> GetPrivateUserInfoAsync(CancellationToken cancellationToken = default);
```

## Formatting

- Allman braces (opening brace on new line)
- 4-space indentation (2 spaces in `.csproj` files, per `.editorconfig`)
- Expression-bodied members for single-expression methods/properties
- LINQ method chains: one operation per line, aligned
- Blank line between member sections

## C# Language Features

- Pattern matching: `is`, `is not`, type patterns, property patterns
- Switch expressions
- Primary constructors everywhere applicable
- Collection expressions: `[]`, `[..spread]`
- Target-typed `new()` when type is clear
- Lock fields: prefer the dedicated `System.Threading.Lock` type (.NET 9+) over a plain `object` — more expressive, and the compiler enforces correct `lock` usage on it.
- Raw string literals for embedded data:
  ```csharp
  private const string UniswapV3Abi =
      """
      [{"inputs":[{"internalType":"address","name":"_factory","type":"address"}, ...
      """;
  ```

## LINQ

- Method syntax preferred
- Fluent chaining, one operation per line

## Dependency Injection

- Constructor injection exclusively
- Builder pattern with extension methods: `AddEthernaUserClients()` + per-service `AddEtherna<Service>Client()`, `AddEthernaInternalClients()`
- Factory-based registration for complex dependencies

## Testing (xUnit + Moq)

- `[Fact]` for basic tests; `[Theory, MemberData(nameof(…))]` with test-element helper classes for parameterized cases (see `BasicUUriTest`)
- AAA pattern with section comments: `// Setup.`, `// Action.`, `// Assert.`
- Moq for mocking
- Test projects mirror the project under test (`EthernaSdk.Tools.Video.UnitTests` mirrors `EthernaSdk.Tools.Video`, same folder structure); add a new mirror test project only when a project grows logic that needs covering
