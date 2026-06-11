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

`nuget.config` adds the `ethernaMyget` feed (https://www.myget.org/F/etherna/api/v3/index.json) next to nuget.org — prerelease dependencies like `SwarmSdk`/`SwarmSdk.Client` and `Etherna.Authentication.Native` come from there. Versioning is automatic via **GitVersion** (`mode: ContinuousDeployment`, `GitVersion.MsBuild` in every package project, plus SourceLink). CI (`.github/workflows/`): pushes to `dev` and `release/**` build, test, pack and push unstable packages to MyGet; tags `v*.*.*` push stable packages to NuGet.

## Architecture

Solution `EthernaSdk.sln`. Project folders are named `EthernaSdk.*` but root namespaces use `Etherna.Sdk.*`; under the root, namespace mirrors the folder path.

- **`src/EthernaSdk.{Credit,Gateway,Index,Sso}.Common`** (`Etherna.Sdk.<Service>`) — Each contains only the NSwag-generated client under `GenClients/` (namespace `Etherna.Sdk.<Service>.GenClients`, file `NSwagEtherna<Service>Client.cs`). Generated code: never edit by hand, regenerate instead (see below).
- **`src/EthernaSdk.UsersCommon`** (`Etherna.Sdk.Users`) — The user-clients builder (`IEthernaUserClientsBuilder`/`EthernaUserClientsBuilder`) and `ServiceCollectionExtensions` entry points `AddEthernaUserClientsWithApiKeyAuth` / `AddEthernaUserClientsWithCodeAuth`. Authentication ("api key" or "oauth code" flow, with access-token expiration management) comes from `Etherna.Authentication.Native`.
- **`src/EthernaSdk.Users.{Credit,Gateway,Index,Sso}`** (`Etherna.Sdk.Users.<Service>`) — The public per-service clients for user applications. `Clients/` holds the interface + implementation pair (`IEthernaUserSsoClient`/`EthernaUserSsoClient`, …); `Models/` holds the public wrapper models; `Extensions/EthernaUserClientsBuilderExtensions.cs` adds the per-service registration (`AddEthernaCreditClient`, `AddEthernaGatewayClient`, `AddEthernaIndexClient`, `AddEthernaSsoClient`), declared in the `Etherna.Sdk.Users` namespace via `// ReSharper disable CheckNamespace` so all registrations are reachable from one using. `Users.Gateway` also has `Services/GatewayService` (higher-level gateway operations, e.g. postage batch creation/waiting), with an optional `dryMode` on `AddEthernaGatewayClient`.
- **`src/EthernaSdk.Internal`** (`Etherna.Sdk.Internal`) — Service-to-service clients (`EthernaInternalCreditClient`, `EthernaInternalSsoClient`) for internal worker applications only.
- **`src/EthernaSdk.Internal.AspNetCore`** (`Etherna.Sdk.Internal.AspNetCore`) — ASP.NET Core registration adapter for the internal clients: `AddEthernaInternalClients` + `IEthernaInternalClientsBuilder`, "client credentials" flow via `Duende.AccessTokenManagement`.
- **`src/EthernaSdk.Tools.Video`** (`Etherna.Sdk.Tools.Video`) — Tools to build, serialize and parse Etherna video manifests (`VideoManifestService`, `HlsService`); versioned manifest DTOs under `Serialization/Dtos/` (`Manifest1/`, `Manifest2/`, `PersonalData1/`).
- **`src/EthernaSdk.Tools.UniversalFiles`** (`Etherna.Sdk.Tools.UniversalFiles`) — The `UUri`/`UFile` abstraction over local, online and Swarm resources (`UUriKind` flags, `UFileProvider`).
- **`src/EthernaSdk.Tools.Tokens`** (`Etherna.Sdk.Tools.Tokens`) — Blockchain token utilities on Nethereum (e.g. `UniswapService` pool price reads).
- **`test/`** — xUnit + Moq: `EthernaSdk.Tools.UniversalFiles.Tests`, `EthernaSdk.Tools.Video.UnitTests`.

### Key cross-cutting points

- **Generated clients are never exposed.** The adapter pattern is the central design: public clients wrap the NSwag `GenClients` types behind hand-written interfaces, and generated DTOs are converted into public wrapper models with `internal` constructors (e.g. `PrivateUserInfo(PrivateUserDto)`). Access across assemblies is granted by `[InternalsVisibleTo]` in `Properties/AssemblyInfo.cs` (the `*.Common` projects expose internals to the `Users.*`/`Internal` projects, and the `Tools.*` projects to their test projects) — extend those attributes when a new project needs them. Assemblies also declare `[CLSCompliant(false)]`.
- **Regenerating clients.** Run `nswag run tools/etherna-<service>-api.nswag` (NSwagStudio on Windows, NSwag CLI elsewhere) against a locally running instance of the service — the OpenAPI `url` in each `.nswag` file points to the service's localhost dev port. Output goes to the matching `*.Common/GenClients/` folder. Watch out: `tools/etherna-credit-api.nswag` still has the pre-rename output path `src/EthernaSdk.Credit/` — make sure the regenerated file lands in `src/EthernaSdk.Credit.Common/GenClients/`.
- **This is a library: always `ConfigureAwait(false)`** on every await. This is the opposite of the Etherna ASP.NET Core service repos (beehive, credit, index, sso), where `ConfigureAwait` is omitted.
- **Public API surface is the product.** These packages are published to NuGet: keep the public surface intentional, document the client interfaces with XML docs, and remember both target frameworks must compile.

## Issue tracker

Bugs and features are tracked in Jira project **ESC** (https://etherna.atlassian.net/projects/ESC). Branch names follow `feature/ESC-<id>-<slug>` / `improve/ESC-<id>-<slug>` / `fix/ESC-<id>-<slug>` — match this when creating branches. `dev` is the integration branch, `main` is stable; stable releases are tagged `v<version>` (e.g. `v0.3.3`).

# Coding Style

## General Principles

- Keep commits clean: only include changes strictly necessary for the task at hand.
- Never reference AI agents or assistants in commits or code — no agent names, no `Co-Authored-By` agent trailers, no "generated/assisted by" notes. Commit messages and code must read as the team's own work.
- Exceptions to these conventions are accepted when strictly necessary or when they significantly improve code quality. Justify with a comment where needed.
- All elements (usings, properties, methods, fields, enum members, etc.) are always alphabetically ordered within their respective sections.
- Primary constructors are preferred everywhere the constructor is a simple parameter assignment.
- Keep code clean: remove unused variables, dead code, and redundant imports.
- Every source file starts with the standard LGPL-3.0 copyright header (`// Copyright 2020-present Etherna SA` … see any existing file).

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

- `sealed` for concrete implementations not meant for inheritance (`public sealed class GatewayService`, `internal sealed class EthernaInternalClientsBuilder`)
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
- Prefer `null` over `default` as default value for optional parameters
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
- Builder pattern with extension methods: `AddEthernaUserClientsWithCodeAuth()` / `AddEthernaUserClientsWithApiKeyAuth()` + per-service `AddEtherna<Service>Client()`, `AddEthernaInternalClients()`
- Factory-based registration for complex dependencies

## Testing (xUnit + Moq)

- `[Fact]` for basic tests; `[Theory, MemberData(nameof(…))]` with test-element helper classes for parameterized cases (see `BasicUUriTest`)
- AAA pattern with section comments: `// Setup.`, `// Action.`, `// Assert.`
- Moq for mocking
- Test projects mirror the project under test (`EthernaSdk.Tools.Video.UnitTests` mirrors `EthernaSdk.Tools.Video`, same folder structure); add a new mirror test project only when a project grows logic that needs covering
