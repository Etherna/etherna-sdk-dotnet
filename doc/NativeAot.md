# Native AOT / trimming compatibility

Every library of Etherna SDK .Net but `EthernaSdk.Tools.Tokens` is **Native AOT and trim compatible**: each
one declares `<IsAotCompatible>true</IsAotCompatible>`, and the whole surface is verified by an ahead-of-time
publish (see [Verifying](#verifying)). `EthernaSdk.Tools.Tokens` is built on Nethereum, whose ABI decoding and
JSON-RPC client work by reflection.

This document records the constraints that keep them AOT-safe.

## Verifying

Two gates, the second one wider than the first:

- **The build.** `IsAotCompatible` turns the Roslyn trim and AOT analyzers on, and `TreatWarningsAsErrors`
  makes their warnings errors: a reflection-based `JsonSerializer` call fails the build of the project, in
  hand-written code and in the NSwag-generated clients alike (IL2026, IL3050).
- **The AOT publish.** `test/EthernaSdk.AotCompatibility` is a console probe that exists only to be published
  with Native AOT. It roots every library as a `TrimmerRootAssembly`, so the ILC compiler analyzes every
  operation (not just what `Main` reaches), the third-party dependencies included, which the build analyzers
  never see. Its `Main` also runs a smoke test, so the native binary proves at runtime the paths that a clean
  publish can't: the JSON of each generated client through its source-generated context, an enum query value,
  a video manifest written and read back, an HLS playlist parsed, the registration of the clients.

```bash
dotnet publish test/EthernaSdk.AotCompatibility -c Release -r <rid>   # e.g. linux-x64, win-x64, osx-arm64
./test/EthernaSdk.AotCompatibility/bin/Release/net10.0/<rid>/publish/EthernaSdk.AotCompatibility
```

The probe sets `TreatWarningsAsErrors=true` / `MSBuildTreatWarningsAsErrors=true`, so any trim or AOT warning
fails the publish. A clean publish and an `AOT smoke test PASSED` exit mean the libraries are AOT-clean. The
publish needs the native toolchain of the platform (`clang` and `zlib1g-dev` on Linux).

CI runs exactly this on every pull request and on pushes to `main` (`.github/workflows/aot-compatibility.yml`),
and both deploy workflows depend on it: an AOT regression blocks the packages.

## Constraint 1 — System.Text.Json must use source generation

Reflection-based `System.Text.Json` (any `JsonSerializer.Serialize/Deserialize` overload taking
`JsonSerializerOptions` instead of a `JsonTypeInfo`) is not AOT-safe. Use a source-generated
`JsonSerializerContext`.

`EthernaSdk.Tools.Video` has two, under `Serialization/`, whose options are the ones the reflection-based
serializers had, so the documents are the same byte for byte:

- `ManifestJsonSerializerContext` writes the preview and the details manifests and reads and writes the personal
  data: camel case names, null members left out.
- `PublishedManifestJsonSerializerContext` reads the published manifests of every schema version: names matched
  whatever their case.

A new DTO serialized by the library goes into the `[JsonSerializable]` list of the context of its use.

## Constraint 2 — manual AOT fixes on the NSwag generated clients

NSwag doesn't emit a source-generation context, so the four generated clients
(`src/EthernaSdk.<Service>.Common/GenClients/NSwagEtherna<Service>Client.cs`) need a few manual
post-generation fixes. They **must be re-applied after every regeneration** (`nswag run …`) until NSwag
supports Native AOT, at which point they — and the `*.Aot.cs` files — can be deleted. The build fails until
they are.

**Hand-written companion files (not regenerated, edit freely):**
`GenClients/NSwagEtherna<Service>Client.Aot.cs`, one for each `*.Common` project.

Each declares a `<Service>JsonSerializerContext` listing the root types the clients exchange (every request
body and response type: nested DTOs and enums are pulled in transitively), implements the generated
`UpdateJsonSerializerSettings` partial hook of each client class to add that context to the client's
`JsonSerializerOptions.TypeInfoResolverChain`, and, in the client classes sending a JSON body, provides the
`SerializeBody<T>` helper used by the first edit below. When a regeneration adds a request body or a response
type, or a client class, add it there. A missing root type passes the build and the publish, and fails at
runtime with `NotSupportedException`: the smoke test of the probe covers one operation for each client.

**Edits inside the generated `NSwagEtherna<Service>Client.cs` files (re-apply after regeneration):**

1. Every `System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings)` →
   `SerializeBody(body)`. The helper resolves the `JsonTypeInfo` from the **static** type `T` of the body, so a
   null body still serializes as `"null"` like the original call.
2. In `ReadObjectResponseAsync<T>`, the two reflection-based deserialization calls → the non-generic
   `JsonTypeInfo` overloads:
   - `var typedBody = (T)System.Text.Json.JsonSerializer.Deserialize(responseText, JsonSerializerSettings.GetTypeInfo(typeof(T)))!;`
   - `var typedBody = (T)(await System.Text.Json.JsonSerializer.DeserializeAsync(responseStream, JsonSerializerSettings.GetTypeInfo(typeof(T)), cancellationToken).ConfigureAwait(false))!;`
3. On every `ConvertToString(object?, CultureInfo)`, add
   `[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "…")]`.
   The method reflects over the fields of an enum to read its `[EnumMember]` value for a query parameter: Native
   AOT keeps the field metadata and the attributes of the enums the API signatures reference, so the reflection
   resolves (the smoke test sends a `VoteValue` and checks the query carries its `[EnumMember]` value).

## Constraint 3 — third-party dependencies

A library can't fix the reflection of the packages it depends on. The probe exempts each of them on its own
(their warnings collapse into a single IL2104 that the probe suppresses), and the exemption holds only while
the runtime behaviour is proven:

- **M3U8Parser** (HLS playlists, `EthernaSdk.Tools.Video`) reads the tags and attributes of a playlist by
  reflection over its own types, with no trimming annotation: trimmed, the parsing throws
  `MissingMethodException`. `EthernaSdk.Tools.Video` embeds `ILLink.Descriptors.xml`, which preserves the whole
  assembly in every application trimming or AOT-compiling it, and the smoke test parses a playlist in the native
  binary. An application publishing with AOT still sees one `IL2104` warning for `M3U8Parser`.
- **Microsoft.Extensions.Caching.Hybrid**, registered by the token management of
  `Etherna.Authentication.Native` under the user clients, warns on its default reflection-based JSON serializer
  factory. It is exempted as `Etherna.Authentication` exempts it, which documents the fallback as never
  exercised: the tokens are cached through Duende's own serializers. Three of its IL2026 warnings escape the
  per-assembly collapse in ILC, so the probe suppresses IL2026 globally: the build analyzers keep guarding the
  libraries against it.
