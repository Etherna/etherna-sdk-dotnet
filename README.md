# Etherna SDK .Net

[![EthernaSdk.Users.Credit on NuGet](https://img.shields.io/nuget/v/EthernaSdk.Users.Credit?label=EthernaSdk.Users.Credit)](https://www.nuget.org/packages/EthernaSdk.Users.Credit/)
[![EthernaSdk.Users.Gateway on NuGet](https://img.shields.io/nuget/v/EthernaSdk.Users.Gateway?label=EthernaSdk.Users.Gateway)](https://www.nuget.org/packages/EthernaSdk.Users.Gateway/)
[![EthernaSdk.Users.Index on NuGet](https://img.shields.io/nuget/v/EthernaSdk.Users.Index?label=EthernaSdk.Users.Index)](https://www.nuget.org/packages/EthernaSdk.Users.Index/)
[![EthernaSdk.Users.Sso on NuGet](https://img.shields.io/nuget/v/EthernaSdk.Users.Sso?label=EthernaSdk.Users.Sso)](https://www.nuget.org/packages/EthernaSdk.Users.Sso/)
[![EthernaSdk.Internal on NuGet](https://img.shields.io/nuget/v/EthernaSdk.Internal?label=EthernaSdk.Internal)](https://www.nuget.org/packages/EthernaSdk.Internal/)
[![EthernaSdk.Tools.Tokens on NuGet](https://img.shields.io/nuget/v/EthernaSdk.Tools.Tokens?label=EthernaSdk.Tools.Tokens)](https://www.nuget.org/packages/EthernaSdk.Tools.Tokens/)
[![EthernaSdk.Tools.UniversalFiles on NuGet](https://img.shields.io/nuget/v/EthernaSdk.Tools.UniversalFiles?label=EthernaSdk.Tools.UniversalFiles)](https://www.nuget.org/packages/EthernaSdk.Tools.UniversalFiles/)
[![EthernaSdk.Tools.Video on NuGet](https://img.shields.io/nuget/v/EthernaSdk.Tools.Video?label=EthernaSdk.Tools.Video)](https://www.nuget.org/packages/EthernaSdk.Tools.Video/)
[![Target frameworks](https://img.shields.io/badge/.NET-9%20%7C%2010-512BD4)](#supported-frameworks)
[![License: LGPL-3.0](https://img.shields.io/badge/license-LGPL--3.0-blue)](COPYING-LESSER)

**Etherna SDK .Net** provides the .NET client libraries to consume the APIs of the
[Etherna](https://etherna.io) services: Credit, Gateway, Index and SSO. Typed clients cover the user
APIs, called on behalf of a signed-in Etherna account, and the internal service-to-service APIs. A set
of tools packages works with Etherna video manifests, with files located anywhere, and with blockchain
tokens.

Etherna SDK .Net is a set of **libraries**, not an application. Add the package of each service you
need, and call it from your .NET application with sign-in and access tokens managed for you.

## Contents

- [Features](#features)
- [Packages](#packages)
- [Installation](#installation)
- [Usage examples](#usage-examples)
  - [User application — sign in and call a service](#user-application--sign-in-and-call-a-service)
  - [Upload to Swarm through the Etherna Gateway](#upload-to-swarm-through-the-etherna-gateway)
  - [Search and publish videos on the Index](#search-and-publish-videos-on-the-index)
  - [Service-to-service — internal clients](#service-to-service--internal-clients)
  - [Read a published video manifest](#read-a-published-video-manifest)
  - [Universal files — one API for local, online and Swarm files](#universal-files--one-api-for-local-online-and-swarm-files)
  - [Token prices](#token-prices)
  - [Runnable samples](#runnable-samples)
- [Supported frameworks](#supported-frameworks)
- [Building and testing](#building-and-testing)
- [Project layout](#project-layout)
- [Package repositories](#package-repositories)
- [Contributing](#contributing)
- [Issue reports](#issue-reports)
- [Questions? Problems?](#questions-problems)
- [License](#license)

## Features

- **A client for each service** — `IEthernaUserCreditClient`, `IEthernaUserGatewayClient`,
  `IEthernaUserIndexClient` and `IEthernaUserSsoClient` call the user APIs of the services on behalf of
  the signed-in account. Each one ships in its own package: reference only the services you use.
- **Sign-in flow chosen at runtime** — a single registration wires both the interactive sign-in, on the
  system browser, and the api-key one, for scripted automation: pick the flow only when signing in.
  Access tokens are acquired, attached to every request and refreshed automatically by
  [Etherna Authentication](https://github.com/Etherna/etherna-authentication).
- **Swarm through the Etherna Gateway** — the gateway registration also provides the `ISwarmClient` of
  [SwarmSDK](https://github.com/Etherna/swarm-sdk-dotnet) over the authenticated connection, and
  `GatewayService` puts the gateway and Swarm operations behind one interface: buy a postage batch and
  wait until it is usable, upload files and directories, pin content, offer it to all users.
- **Dry mode** — rehearse an upload flow without writing anything: the writes of the Swarm client and of
  the gateway service become no-ops.
- **Internal clients for services** — `IEthernaInternalCreditClient` and `IEthernaInternalSsoClient`
  call the service-to-service APIs with the client credentials flow, each with its own access token.
  No ASP.NET dependency: usable by any kind of host.
- **Video manifests** — build an Etherna video manifest and write it as Swarm chunks, read a published
  one back with its validation errors (schema versions 1 and 2), parse HLS playlists into video
  encodings and variants.
- **Universal files** — `UUri` and `UFile` address and read a file through the same API wherever it is
  located: on the local disk, online over HTTP, or on Swarm. An experimental library.
- **Token prices** — read the BZZ and Ether prices from the Uniswap v3 pools on Ethereum mainnet.
- **Dependency injection first** — the clients are registered on the `IServiceCollection` through a
  builder, consumed through their interfaces, and send their requests on `HttpClient`s built by the
  `IHttpClientFactory`.
- **Structured logging** — the relevant actions (a postage batch purchase, a manifest written or read,
  a tolerated failure) are logged as typed events on the `ILogger` of the container.

## Packages

Thirteen NuGet packages are published. Eight are the ones to reference, chosen by what the application
does:

| Package | Use it for | Depends on |
| --- | --- | --- |
| [**EthernaSdk.Users.Credit**](https://www.nuget.org/packages/EthernaSdk.Users.Credit/) | User API client of Etherna Credit: the credit balance of the signed-in user and its operation logs. | `EthernaSdk.UsersCommon`, `EthernaSdk.Credit.Common` |
| [**EthernaSdk.Users.Gateway**](https://www.nuget.org/packages/EthernaSdk.Users.Gateway/) | User API client of the Etherna Gateway, the Swarm client over it and `GatewayService`: postage batches, uploads and downloads, pins, download funding, welcome pack. | `EthernaSdk.UsersCommon`, `EthernaSdk.Gateway.Common` |
| [**EthernaSdk.Users.Index**](https://www.nuget.org/packages/EthernaSdk.Users.Index/) | User API client of Etherna Index: search and read videos, publish and update their manifests, comments, votes, reports and moderation. | `EthernaSdk.UsersCommon`, `EthernaSdk.Index.Common`, `EthernaSdk.Tools.Video` |
| [**EthernaSdk.Users.Sso**](https://www.nuget.org/packages/EthernaSdk.Users.Sso/) | User API client of Etherna SSO: the account of the signed-in user, and the lookup of users by address or username. | `EthernaSdk.UsersCommon`, `EthernaSdk.Sso.Common` |
| [**EthernaSdk.Internal**](https://www.nuget.org/packages/EthernaSdk.Internal/) | Service-to-service clients of Etherna Credit and Etherna SSO, authenticated with the client credentials flow. For the Etherna services themselves and their workers. | `EthernaSdk.Credit.Common`, `EthernaSdk.Sso.Common` |
| [**EthernaSdk.Tools.Tokens**](https://www.nuget.org/packages/EthernaSdk.Tools.Tokens/) | Blockchain token utilities built on [Nethereum](https://nethereum.com/): the BZZ and Ether prices read from Uniswap. | — |
| [**EthernaSdk.Tools.UniversalFiles**](https://www.nuget.org/packages/EthernaSdk.Tools.UniversalFiles/) | Universal files: one uri and one file abstraction over local, online and Swarm resources. | — |
| [**EthernaSdk.Tools.Video**](https://www.nuget.org/packages/EthernaSdk.Tools.Video/) | Etherna video manifests: models, serialization, publishing as Swarm chunks, reading with validation, HLS playlists. | `EthernaSdk.Tools.UniversalFiles` |

The other five are support packages, brought in transitively by the ones above:

| Package | Content |
| --- | --- |
| [**EthernaSdk.UsersCommon**](https://www.nuget.org/packages/EthernaSdk.UsersCommon/) | The `AddEthernaUserClients` entry point with its builder, shared by the user clients. |
| [**EthernaSdk.Credit.Common**](https://www.nuget.org/packages/EthernaSdk.Credit.Common/) | The HTTP client generated from the OpenAPI definition of Etherna Credit, with its api exception. |
| [**EthernaSdk.Gateway.Common**](https://www.nuget.org/packages/EthernaSdk.Gateway.Common/) | The HTTP client generated from the OpenAPI definition of the Etherna Gateway, with its api exception. |
| [**EthernaSdk.Index.Common**](https://www.nuget.org/packages/EthernaSdk.Index.Common/) | The HTTP client generated from the OpenAPI definition of Etherna Index, with its api exception. |
| [**EthernaSdk.Sso.Common**](https://www.nuget.org/packages/EthernaSdk.Sso.Common/) | The HTTP client generated from the OpenAPI definition of Etherna SSO, with its api exception. |

The Swarm types of the public interfaces (`SwarmReference`, `PostageBatchId`, `BzzValue`, `XDaiValue`,
`EthAddress`...) come from [SwarmSDK](https://github.com/Etherna/swarm-sdk-dotnet), and the sign-in
services from [Etherna Authentication](https://github.com/Etherna/etherna-authentication): both are
dependencies of the packages, there is nothing more to install.

## Installation

```bash
# User applications, one package for each service to call:
dotnet add package EthernaSdk.Users.Credit
dotnet add package EthernaSdk.Users.Gateway
dotnet add package EthernaSdk.Users.Index
dotnet add package EthernaSdk.Users.Sso

# Etherna services calling each other:
dotnet add package EthernaSdk.Internal

# Tools, usable on their own:
dotnet add package EthernaSdk.Tools.Tokens
dotnet add package EthernaSdk.Tools.UniversalFiles
dotnet add package EthernaSdk.Tools.Video
```

`EthernaSdk.UsersCommon` and the four `EthernaSdk.*.Common` packages come in transitively: there is no
reason to reference them directly.

## Usage examples

### User application — sign in and call a service

`AddEthernaUserClients` registers the sign-in against the Etherna SSO server and returns a builder: each
service package adds its own client on it. The client id must match a client registered on the Etherna
SSO server, whose redirect url is on the loopback port passed here, and the requested scopes must
include the one of each service to call.

```csharp
using Etherna.Authentication;
using Etherna.Authentication.Native;
using Etherna.Sdk.Users;
using Etherna.Sdk.Users.Credit.Clients;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddEthernaUserClients(
        clientId: "yourClientId",                 // used by the interactive sign-in
        clientSecret: null,                       // public native clients have no secret
        returnUrlPort: 11420,                     // must match the client's redirect url on the SSO server
        scopes: [EthernaScopes.UserApiCredit])    // one scope for each service to call
    .AddEthernaCreditClient();

await using var serviceProvider = services.BuildServiceProvider();

// Choose the sign-in flow at runtime, e.g. from the user's input.
var signInService = serviceProvider.GetRequiredService<IEthernaSignInService>();
if (apiKey is null)
    await signInService.SignInAsync();        // opens the system browser and waits for the sign-in
else
    await signInService.SignInAsync(apiKey);  // signs in with the api key, without interaction

// Call the service: the access token is attached to the request, and refreshed when it expires.
var creditClient = serviceProvider.GetRequiredService<IEthernaUserCreditClient>();
var credit = await creditClient.GetUserCreditAsync();
Console.WriteLine($"Current balance: {credit.Balance} xDai");
```

Every service follows the same pattern:

| Service | Registration | Scope | Client |
| --- | --- | --- | --- |
| Credit | `AddEthernaCreditClient` | `EthernaScopes.UserApiCredit` | `IEthernaUserCreditClient` |
| Gateway | `AddEthernaGatewayClient` | `EthernaScopes.UserApiGateway` | `IEthernaUserGatewayClient`, `ISwarmClient` |
| Index | `AddEthernaIndexClient` | `EthernaScopes.UserApiIndex` | `IEthernaUserIndexClient` |
| SSO | `AddEthernaSsoClient` | `EthernaScopes.UserApiSso` | `IEthernaUserSsoClient` |

The clients are singletons. By default they call the public Etherna deployment
(`https://credit.etherna.io/`, `https://gateway.etherna.io/`, `https://index.etherna.io/`, and
`https://sso.etherna.io/` as the authority). To call another deployment, each registration takes the
base url of its service as an optional parameter, and `AddEthernaUserClients` takes the `authority`,
which must be an HTTPS url. All of them send their requests on one named `HttpClient`, that
`configureHttpClient` customizes.

A request the service refuses throws the api exception of that service (`EthernaCreditApiException`,
`EthernaGatewayApiException`, `EthernaIndexApiException`, `EthernaSsoApiException`), with the status
code and the response body.

The two sign-in flows, and how to read the identity of the signed-in user, are documented by
[Etherna Authentication](https://github.com/Etherna/etherna-authentication#native-app--sign-in-choosing-the-flow-at-runtime).

### Upload to Swarm through the Etherna Gateway

`AddEthernaGatewayClient` registers two clients over the same authenticated connection: the
`IEthernaUserGatewayClient` for the gateway's own API, and the `ISwarmClient` of SwarmSDK for the Swarm
API the gateway exposes. `GatewayService` combines them, and is the one to use for a whole upload flow.
It is not registered by the builder: add it to the container with the lifetime you need.

```csharp
using Etherna.Authentication;
using Etherna.Sdk.Users;
using Etherna.Sdk.Users.Gateway.Services;
using Etherna.SwarmSdk.Models;
using Microsoft.Extensions.DependencyInjection;

services.AddEthernaUserClients("yourClientId", null, 11420, [EthernaScopes.UserApiGateway])
    .AddEthernaGatewayClient();
services.AddTransient<IGatewayService, GatewayService>();

// ...build the service provider and sign in, as above.

var gatewayService = serviceProvider.GetRequiredService<IGatewayService>();

// Buy a postage batch lasting 30 days, and wait until it is usable.
var chainState = await gatewayService.GetChainStateAsync();
var amount = PostageBatch.CalculateAmount(chainState.CurrentPrice, TimeSpan.FromDays(30));
var (batchId, _) = await gatewayService.BuyPostageBatchAsync(
    amount,
    depth: 20,
    label: "my uploads",
    onWaitingBatchCreation: () => Console.WriteLine("Waiting for the batch to be created..."),
    onWaitingBatchUsable: () => Console.WriteLine("Waiting for the batch to be usable..."));

// Upload a file, and get back its Swarm reference.
await using var content = File.OpenRead("hello.txt");
var reference = await gatewayService.UploadFileAsync(
    content,
    batchId,
    name: "hello.txt",
    contentType: "text/plain");
Console.WriteLine($"Uploaded to {reference}");

// Offer the content to all users.
await gatewayService.FundResourceDownloadAsync(reference);
```

`BuyPostageBatchAsync` returns when the batch can stamp an upload. It waits first for the gateway to
create the batch, then for the batch to be usable, reading every 5 seconds and for up to 15 minutes
each (`GatewayService.BatchCreationTimeout`, `GatewayService.BatchUsableTimeout`): a wait that expires
throws `TimeoutException`. The optional callbacks report the progress of the two waits.

`AddEthernaGatewayClient` has three optional parameters worth knowing:

- **`dryMode`** — with `true` the Swarm client turns its writes into no-ops, and `GatewayService` skips
  the ones it sends to the gateway: no postage batch is bought (the callbacks are invoked and the zero
  batch id is returned), no download is funded or defunded. `IEthernaUserGatewayClient` used directly
  has no dry mode.
- **`apiCompatibility`** — the Swarm API the gateway is called with: `SwarmClients.Beehive` by default,
  or `SwarmClients.Bee` for the API of a Bee node. Funding and defunding downloads are not supported
  with the latter.
- **`timeouts`** — every Swarm call is bounded by the class of its operation (`SwarmClientTimeouts`).
  A gateway that doesn't answer within the bound surfaces as `SwarmClientTimeoutException`, a
  `TimeoutException`.

### Search and publish videos on the Index

The index client resolves the thumbnails of the videos it lists on Swarm: it needs an `ISwarmClient`
in the container, the one `AddEthernaGatewayClient` registers.

```csharp
using Etherna.Authentication;
using Etherna.Sdk.Users;
using Etherna.Sdk.Users.Index.Clients;
using Etherna.Sdk.Users.Index.Models;
using Microsoft.Extensions.DependencyInjection;

services.AddEthernaUserClients(
        "yourClientId",
        null,
        11420,
        [EthernaScopes.UserApiGateway, EthernaScopes.UserApiIndex])
    .AddEthernaGatewayClient()
    .AddEthernaIndexClient();

// ...build the service provider and sign in, as above.

var indexClient = serviceProvider.GetRequiredService<IEthernaUserIndexClient>();

// Search the published videos.
var videos = await indexClient.SearchVideosAsync("swarm", page: 0, take: 20);
foreach (var video in videos.Elements)
    Console.WriteLine($"{video.Id}: {video.Title}");

// Publish a video, from its manifest already uploaded to Swarm with a postage batch.
var videoId = await indexClient.PublishNewVideoAsync(manifestReference, batchId);

// Comment and vote it.
await indexClient.CreateCommentAsync(videoId, "First!");
await indexClient.VotesVideoAsync(videoId, VoteValue.Up);
```

### Service-to-service — internal clients

`AddEthernaInternalClients` registers the clients of the internal APIs, authenticated with the client
credentials flow: the application signs in with its own identity, without any user. Each client has its
own id and secret, its own access token and its own `HttpClient`. These APIs are reserved to the
Etherna services: the client must be created by the administrators of the SSO server, with the scope of
the API it calls (`EthernaScopes.EthernaCreditServiceInteract`, `EthernaScopes.EthernaSsoUserContactInfo`).

```csharp
using Etherna.Sdk.Internal;
using Etherna.Sdk.Internal.Clients;
using Etherna.SwarmSdk.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddEthernaInternalClients(new Uri("https://sso.etherna.io/"))
    .AddEthernaCreditClient(
        new Uri("https://credit.etherna.io/"),
        clientId: "yourCreditClientId",
        clientSecret: "yourCreditClientSecret")
    .AddEthernaSsoClient(
        clientId: "yourSsoClientId",
        clientSecret: "yourSsoClientSecret");

using var host = builder.Build();

// Read and update the credit of a user.
var creditClient = host.Services.GetRequiredService<IEthernaInternalCreditClient>();
var credit = await creditClient.GetUserCreditAsync(userAddress);
if (!credit.IsUnlimited)
    await creditClient.UpdateUserBalanceAsync(
        userAddress,
        XDaiValue.FromDecimal(-0.05m),    // positive for a credit, negative for a debit
        reason: "Storage usage");

// Read the contacts of a user.
var ssoClient = host.Services.GetRequiredService<IEthernaInternalSsoClient>();
var contacts = await ssoClient.GetUserContactsAsync(userAddress);
```

The SSO base url must be HTTPS, loopback addresses excepted: pass `requireHttps: false` to allow a
plain HTTP one, in a development environment.

### Read a published video manifest

`VideoManifestService` reads a video manifest published on Swarm, whatever its schema version, and
validates it. The chunks come from any SwarmSDK chunk store: here, from a Swarm client.

```csharp
using Etherna.Sdk.Tools.Video.Services;
using Etherna.SwarmSdk.Services;
using Etherna.SwarmSdk.Stores;

var videoManifestService = new VideoManifestService(new ChunkService());

var published = await videoManifestService.GetPublishedVideoManifestAsync(
    manifestReference,
    new SwarmClientChunkStore(swarmClient));

foreach (var error in published.ValidationErrors)
    Console.WriteLine($"{error.ErrorType}: {error.ErrorMessage}");

if (published.Manifest is not null)
{
    Console.WriteLine($"{published.Manifest.Title} ({published.Manifest.Duration})");
    foreach (var (uri, source) in published.Manifest.VideoSources)
        Console.WriteLine($"  {source.VideoType} {source.Quality}: {uri}");
}
```

The overload taking also an `IUFileProvider` reads the files of the manifest through it: with a Swarm
file provider each file is served whole by the gateway, with one request, instead of being composed
chunk by chunk. The same service writes a manifest: `CreateVideoManifestChunksAsync` serializes a
`VideoManifest` into the chunks of a local directory, ready to be uploaded, and returns its reference.

### Universal files — one API for local, online and Swarm files

A `UUri` is a uri that can be a local path, an online url or a Swarm address, absolute or relative,
and a `UFile` reads the file it points to. `UFileProvider` builds the files, and copies them locally
when the application needs them on disk.

```csharp
using Etherna.Sdk.Tools.UniversalFiles;
using Etherna.Sdk.Tools.UniversalFiles.Extensions;
using Etherna.SwarmSdk.Models;

var uFileProvider = new UFileProvider(httpClientFactory)
    .UseSwarmUFiles(swarmClient);   // optional, adds the support to Swarm addresses

// The kind of a uri is recognized from the uri itself.
var localFile = uFileProvider.BuildNewUFile(new BasicUUri("videos/intro.mp4"));
var onlineFile = uFileProvider.BuildNewUFile(new BasicUUri("https://example.com/intro.mp4"));
var swarmFile = uFileProvider.BuildNewUFile(new SwarmUUri(new SwarmUri(manifestReference, "thumb.jpg")));

Console.WriteLine(localFile.FileUri.UriKind);    // Relative: local or online, not known yet
Console.WriteLine(onlineFile.FileUri.UriKind);   // OnlineAbsolute
Console.WriteLine(swarmFile.FileUri.UriKind);    // OnlineAbsolute

// A relative uri can't be read until something tells where it lives: restrict the allowed kinds.
if (await localFile.ExistsAsync(allowedUriKinds: UUriKind.Local))
{
    var size = await localFile.GetByteSizeAsync(allowedUriKinds: UUriKind.Local);
    Console.WriteLine($"{await localFile.TryGetFileNameAsync(UUriKind.Local)}: {size} bytes");
}

// Get any file as a local one: a file that is already local is returned as it is.
var downloadedFile = await uFileProvider.ToLocalUFileAsync(swarmFile);
```

### Token prices

`UniswapService` reads the current prices from the Uniswap v3 pools on Ethereum mainnet, through the
JSON-RPC endpoint of a node.

```csharp
using Etherna.Sdk.Tools.Tokens.Services;
using Nethereum.Web3;

var uniswapService = new UniswapService(new Web3("https://your-ethereum-mainnet-rpc/"));

var bzzPriceInEther = await uniswapService.GetBzzPriceInEtherAsync();
var bzzPriceInUsdc = await uniswapService.GetBzzPriceInUsdcAsync();
var etherPriceInUsdc = await uniswapService.GetEtherPriceInUsdcAsync();
```

### Runnable samples

Three sample applications live under [`samples/`](samples), each one reading the credit of a user:

- [`ConsoleUserClientSample`](samples/ConsoleUserClientSample) — a console app signing in a user,
  interactively or with an api key, with the user client of Etherna Credit;
- [`ConsoleInternalClientSample`](samples/ConsoleInternalClientSample) — a console app with the
  internal client of Etherna Credit;
- [`AspNetCoreInternalClientSample`](samples/AspNetCoreInternalClientSample) — a Razor Pages web app
  with the internal client of Etherna Credit, configured from `appsettings.json`.

```bash
dotnet run --project samples/ConsoleUserClientSample -- -h   # prints the arguments
```

## Supported frameworks

The libraries multi-target **.NET 9 and 10**. Install them into any project on a compatible framework.

The user clients bring in the ASP.NET Core shared framework, through the sign-in of Etherna
Authentication: an application using them needs the ASP.NET Core runtime where it runs, a console one
included. The internal clients and the tools need the .NET runtime alone.

## Building and testing

Etherna SDK .Net builds with the standard .NET SDK:

```bash
dotnet restore EthernaSdk.sln
dotnet build   EthernaSdk.sln -c Release   # compiles every target framework
dotnet test    EthernaSdk.sln -c Release   # runs the xUnit test projects
```

`TreatWarningsAsErrors=true` and `AnalysisMode=AllEnabledByDefault` are enabled on every project, so
warnings break the build on every target framework.

The restore needs the Etherna NuGet feed beside NuGet.org, for the prerelease dependencies: the root
`nuget.config` declares both.

Versions are computed by [GitVersion](https://gitversion.net/) from the git history, there are no
manual version bumps.

The HTTP clients under `GenClients/` are generated with [NSwag](https://github.com/RicoSuter/NSwag)
from the OpenAPI definitions of the services, and are not hand-maintained: see
[tools/README.md](tools/README.md) for how to regenerate them.

Coding conventions and architecture notes live in [AGENTS.md](AGENTS.md).

## Project layout

```
src/
  EthernaSdk.Credit.Common         generated client of the Etherna Credit API
  EthernaSdk.Gateway.Common        generated client of the Etherna Gateway API
  EthernaSdk.Index.Common          generated client of the Etherna Index API
  EthernaSdk.Sso.Common            generated client of the Etherna SSO API
  EthernaSdk.Internal              service-to-service clients (→ Credit.Common, Sso.Common)
  EthernaSdk.Tools.Tokens          blockchain token utilities
  EthernaSdk.Tools.UniversalFiles  uris and files over local, online and Swarm resources
  EthernaSdk.Tools.Video           video manifests and HLS tools (→ Tools.UniversalFiles)
  EthernaSdk.UsersCommon           user clients builder, with the sign-in registration
  EthernaSdk.Users.Credit          user client of Etherna Credit (→ UsersCommon, Credit.Common)
  EthernaSdk.Users.Gateway         user client of the Etherna Gateway, gateway service (→ UsersCommon, Gateway.Common)
  EthernaSdk.Users.Index           user client of Etherna Index (→ UsersCommon, Index.Common, Tools.Video)
  EthernaSdk.Users.Sso             user client of Etherna SSO (→ UsersCommon, Sso.Common)
test/
  EthernaSdk.Internal.Tests              xUnit tests of the internal clients registration
  EthernaSdk.Tools.UniversalFiles.Tests  xUnit + Moq tests of the universal files
  EthernaSdk.Tools.Video.UnitTests       xUnit + Moq tests of the video tools
  EthernaSdk.Users.Gateway.Tests         xUnit + Moq tests of the gateway client and service
samples/
  AspNetCoreInternalClientSample   web app using the internal client of Etherna Credit
  ConsoleInternalClientSample      console app using the internal client of Etherna Credit
  ConsoleUserClientSample          console app using the user client of Etherna Credit
tools/                             NSwag configurations generating the API clients
```

## Package repositories

You can get the latest public releases from the [NuGet.org feed](https://www.nuget.org/profiles/etherna).

If you'd like to work with the latest internal releases, you can use our
[Etherna NuGet feed](https://nuget.etherna.io/v3/index.json) (NuGet V3).

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) and our
[Code of Conduct](CODE_OF_CONDUCT.md) before opening a pull request.

## Issue reports

If you've discovered a bug, or have an idea for a new feature, please report it to our issue manager
based on Jira: https://etherna.atlassian.net/projects/ESC.

Detailed reports with stack traces, actual and expected behaviours are welcome.

For a security problem, don't open a public issue: follow the [security policy](https://github.com/Etherna/etherna-sdk-dotnet/security/policy) to report it privately.

## Questions? Problems?

For questions or problems please write an email to [info@etherna.io](mailto:info@etherna.io).

## License

![LGPL Logo](https://raw.githubusercontent.com/Etherna/etherna-sdk-dotnet/main/doc/lgplv3-with-text-154x68.png)

We use the GNU Lesser General Public License v3 or later (SPDX `LGPL-3.0-or-later`) for this project:
[COPYING-LESSER](COPYING-LESSER) adds the lesser terms to the GNU GPL v3 of [COPYING](COPYING).
If you require a custom license, you can contact us at [license@etherna.io](mailto:license@etherna.io).
