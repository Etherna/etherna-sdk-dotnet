// Copyright 2020-present Etherna SA
// This file is part of Etherna SDK .Net.
// 
// Etherna SDK .Net is free software: you can redistribute it and/or modify it under the terms of the
// GNU Lesser General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna SDK .Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License along with Etherna SDK .Net.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.Sdk.Internal;
using Etherna.Sdk.Internal.Clients;
using Etherna.Sdk.Tools.UniversalFiles;
using Etherna.Sdk.Tools.Video.Models;
using Etherna.Sdk.Tools.Video.Services;
using Etherna.Sdk.Users;
using Etherna.Sdk.Users.Credit.Clients;
using Etherna.Sdk.Users.Gateway.Clients;
using Etherna.Sdk.Users.Index.Clients;
using Etherna.Sdk.Users.Index.Models;
using Etherna.Sdk.Users.Sso.Clients;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Models;
using Etherna.SwarmSdk.Services;
using Etherna.SwarmSdk.Stores;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.AotCompatibility
{
    // Native AOT compatibility probe for the Etherna SDK libraries. The actual compatibility gate is the ILC publish
    // (see the .csproj and CI): publishing this app fails on any trim/AOT warning. This entry point additionally
    // proves, at runtime, that the AOT-sensitive paths work once compiled ahead-of-time: the JSON of every generated
    // client resolved through its source-generated context, the enum query values read by reflection, the video
    // manifests written and read through their source-generated contexts, the HLS parsing, and the registration of
    // the clients. Each check prints what it observed, so the output on the JIT runtime and on the native binary
    // can be compared line by line.
    internal static class Program
    {
        // Consts.
        private const string Address = "0x0000000000000000000000000000000000000001";
        private const string Hash = "0000000000000000000000000000000000000000000000000000000000000001";
        private static readonly Uri ServiceUrl = new("http://localhost/");

        // Methods.
        public static async Task<int> Main()
        {
            var failures =
                await RunAsync("credit", VerifyCreditClientsAsync).ConfigureAwait(false) +
                await RunAsync("gateway", VerifyGatewayClientAsync).ConfigureAwait(false) +
                await RunAsync("index", VerifyIndexClientAsync).ConfigureAwait(false) +
                await RunAsync("sso", VerifySsoClientsAsync).ConfigureAwait(false) +
                await RunAsync("video manifest", VerifyVideoManifestAsync).ConfigureAwait(false) +
                await RunAsync("hls", VerifyHlsParsingAsync).ConfigureAwait(false) +
                await RunAsync("registration", VerifyRegistrationAsync).ConfigureAwait(false);

            Console.WriteLine(failures == 0 ? "AOT smoke test PASSED" : $"AOT smoke test FAILED ({failures})");
            return failures;
        }

        // Helpers.
        private static void Expect(bool condition, string description)
        {
            if (!condition)
                throw new InvalidOperationException(description);
        }

        private static async Task<int> RunAsync(string name, Func<Task<string>> check)
        {
            // A NotSupportedException here would mean the JsonTypeInfo metadata was missing (the classic AOT failure mode).
            try
            {
                Console.WriteLine($"[ok] {name}: {await check().ConfigureAwait(false)}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] {name}: {ex.GetType().Name}: {ex.Message}");
                return 1;
            }
        }

        private static async Task<string> VerifyCreditClientsAsync()
        {
            // User client: an object response.
            var userHandler = new CannedJsonHandler("""{"balance":12.5,"isUnlimited":false}""");
            var credit = await new EthernaUserCreditClient(ServiceUrl, new HttpClient(userHandler))
                .GetUserCreditAsync().ConfigureAwait(false);
            Expect(credit.Balance == XDaiValue.FromDecimal(12.5m) && !credit.IsUnlimited, "Unexpected user credit.");

            // Internal client: a collection response.
            var internalHandler = new CannedJsonHandler(
                $$"""[{"amount":-0.5,"author":"probe","creationDateTime":"2026-01-01T00:00:00+00:00","isApplied":true,"operationName":"probeOperation","reason":"probe","userAddress":"{{Address}}"}]""");
            var opLogs = (await new EthernaInternalCreditClient(ServiceUrl, new HttpClient(internalHandler))
                .GetUserOpLogsAsync(Address).ConfigureAwait(false)).ToArray();
            Expect(opLogs.Length == 1 && opLogs[0].OperationName == "probeOperation", "Unexpected operation logs.");

            return $"balance {credit.Balance}, {opLogs.Length} operation log, amount {opLogs[0].Amount}";
        }

        private static async Task<string> VerifyGatewayClientAsync()
        {
            // A collection request body, and a dictionary response.
            var handler = new CannedJsonHandler($$"""{"{{Hash}}":true}""");
            var funded = await new EthernaUserGatewayClient(ServiceUrl, new HttpClient(handler))
                .AreResourcesDownloadFundedAsync([SwarmHash.FromString(Hash)]).ConfigureAwait(false);
            Expect(funded.Count == 1 && funded[SwarmHash.FromString(Hash)], "Unexpected funded resources.");

            return $"request body {handler.Requests.Single().Body}, {funded.Count} funded resource";
        }

        private static async Task<string> VerifyIndexClientAsync()
        {
            using var swarmClient = new SwarmClient(
                ServiceUrl,
                SwarmClients.Beehive,
                new HttpClient(new CannedJsonHandler("{}")) { Timeout = Timeout.InfiniteTimeSpan });

            // An object request body, and a string response.
            var publishHandler = new CannedJsonHandler("\"probeVideoId\"");
            var videoId = await new EthernaUserIndexClient(ServiceUrl, swarmClient, new HttpClient(publishHandler))
                .PublishNewVideoAsync(SwarmReference.FromString(Hash), PostageBatchId.FromString(Hash)).ConfigureAwait(false);
            Expect(videoId == "probeVideoId", "Unexpected video id.");

            // An enum query value, read through its [EnumMember] attribute.
            var voteHandler = new CannedJsonHandler("{}");
            await new EthernaUserIndexClient(ServiceUrl, swarmClient, new HttpClient(voteHandler))
                .VotesVideoAsync("probeVideoId", VoteValue.Down).ConfigureAwait(false);
            var voteUri = voteHandler.Requests.Single().Uri;
            Expect(voteUri.EndsWith("?value=Down", StringComparison.Ordinal), "Unexpected vote query.");

            // A collection of objects as request body, serialized by the runtime type of its items, and a collection response.
            var validationHandler = new CannedJsonHandler(
                $$"""[{"errorDetails":[],"hash":"{{Hash}}","isValid":true,"validationTime":"2026-01-01T00:00:00+00:00","videoId":"probeVideoId"}]""");
            var validations = (await new EthernaUserIndexClient(ServiceUrl, swarmClient, new HttpClient(validationHandler))
                .GetBulkVideoValidationStatusByManifestsAsync([SwarmReference.FromString(Hash)]).ConfigureAwait(false)).ToArray();
            Expect(validations.Length == 1 && validations[0].IsValid == true, "Unexpected validation status.");

            // An object response.
            var parametersHandler = new CannedJsonHandler(
                """{"commentMaxLength":1,"videoDescriptionMaxLength":2,"videoPersonalDataMaxLength":3,"videoTitleMaxLength":4}""");
            var parameters = await new EthernaUserIndexClient(ServiceUrl, swarmClient, new HttpClient(parametersHandler))
                .GetIndexParametersAsync().ConfigureAwait(false);
            Expect(parameters.VideoTitleMaxLength == 4, "Unexpected index parameters.");

            return $"request bodies {publishHandler.Requests.Single().Body} and {validationHandler.Requests.Single().Body}, " +
                   $"vote {voteUri}, {validations.Length} validation status, title max length {parameters.VideoTitleMaxLength}";
        }

        private static async Task<string> VerifySsoClientsAsync()
        {
            // User client: an object response with a collection member.
            var userHandler = new CannedJsonHandler(
                $$"""{"accountType":"web3","email":null,"etherAddress":"{{Address}}","etherManagedPrivateKey":null,"etherPreviousAddresses":["{{Address}}"],"etherLoginAddress":null,"phoneNumber":null,"username":"probe"}""");
            var user = await new EthernaUserSsoClient(ServiceUrl, new HttpClient(userHandler))
                .GetPrivateUserInfoAsync().ConfigureAwait(false);
            Expect(user.Username == "probe" && user.EtherPreviousAddresses.Count() == 1, "Unexpected user info.");

            // Internal client.
            var internalHandler = new CannedJsonHandler("""{"email":"probe@example.com","phoneNumber":null}""");
            var contacts = await new EthernaInternalSsoClient(ServiceUrl, new HttpClient(internalHandler))
                .GetUserContactsAsync(Address).ConfigureAwait(false);
            Expect(contacts.Email == "probe@example.com", "Unexpected user contacts.");

            return $"user {user.Username} with {user.EtherPreviousAddresses.Count()} previous address, contact {contacts.Email}";
        }

        private static async Task<string> VerifyVideoManifestAsync()
        {
            var chunksDirectory = Directory.CreateTempSubdirectory("etherna-sdk-aot-").FullName;
            try
            {
                var personalData = new VideoManifestPersonalData("probeClient", "1.0", "probeSource", "probeVideo");
                var manifest = new VideoManifest(
                    1.78f,
                    new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    "probe description",
                    TimeSpan.FromSeconds(42),
                    "probe title",
                    Address,
                    personalData.Serialize(),
                    [new VideoManifestVideoSource("720p.mp4", VideoType.Mp4, "720p", 1234, [], SwarmReference.FromString(Hash))],
                    new VideoManifestImage(1.78f, "probeBlurhash", [new VideoManifestImageSource("720.jpeg", ImageType.Jpeg, 720, SwarmReference.FromString(Hash))]),
                    []);

                // Write the manifest as chunks, and read it back as a published one.
                var videoManifestService = new VideoManifestService(new ChunkService());
                var reference = await videoManifestService.CreateVideoManifestChunksAsync(manifest, chunksDirectory)
                    .ConfigureAwait(false);
                var published = await videoManifestService.GetPublishedVideoManifestAsync(
                    reference,
                    new LocalDirectoryChunkStore(chunksDirectory)).ConfigureAwait(false);

                Expect(published.ValidationErrors.Count == 0, "Unexpected validation errors.");
                Expect(published.Manifest is not null && published.Manifest.Equals(manifest), "The manifest read back differs.");
                Expect(personalData.Equals(published.Manifest!.PersonalData), "The personal data read back differs.");

                return $"reference {reference}, schema {published.SchemaVersion}, read back equal";
            }
            finally
            {
                Directory.Delete(chunksDirectory, true);
            }
        }

        private static async Task<string> VerifyHlsParsingAsync()
        {
            var directory = Directory.CreateTempSubdirectory("etherna-sdk-aot-").FullName;
            try
            {
                Directory.CreateDirectory(Path.Combine(directory, "720p"));
                await File.WriteAllTextAsync(Path.Combine(directory, "master.m3u8"), """
                    #EXTM3U
                    #EXT-X-VERSION:3
                    #EXT-X-STREAM-INF:BANDWIDTH=2149280,CODECS="mp4a.40.2,avc1.64001f",RESOLUTION=1280x720
                    720p/playlist.m3u8
                    """).ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(directory, "720p", "playlist.m3u8"), """
                    #EXTM3U
                    #EXT-X-VERSION:3
                    #EXT-X-TARGETDURATION:10
                    #EXTINF:9.009,
                    segment0.ts
                    #EXTINF:3.003,
                    segment1.ts
                    #EXT-X-ENDLIST
                    """).ConfigureAwait(false);
                await File.WriteAllBytesAsync(Path.Combine(directory, "720p", "segment0.ts"), new byte[100]).ConfigureAwait(false);
                await File.WriteAllBytesAsync(Path.Combine(directory, "720p", "segment1.ts"), new byte[50]).ConfigureAwait(false);

                await using var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
                var uFileProvider = new UFileProvider(serviceProvider.GetRequiredService<IHttpClientFactory>());
                var hlsService = new HlsService(uFileProvider);

                var masterFile = await FileBase.BuildFromUFileAsync(uFileProvider.BuildNewUFile(
                    new BasicUUri(Path.Combine(directory, "master.m3u8"), UUriKind.LocalAbsolute))).ConfigureAwait(false);
                var masterPlaylist = await hlsService.TryParseHlsMasterPlaylistFromFileAsync(masterFile).ConfigureAwait(false) ??
                    throw new InvalidOperationException("The master playlist was not recognized.");
                var encoding = await hlsService.ParseVideoEncodingFromHlsMasterPlaylistFileAsync(
                    TimeSpan.FromSeconds(12),
                    masterFile,
                    null,
                    masterPlaylist,
                    new MemoryChunkStore()).ConfigureAwait(false);

                var variant = (HlsVideoVariant)encoding.Variants.Single();
                Expect(variant is { Width: 1280, Height: 720, HlsSegmentFiles.Count: 2 }, "Unexpected HLS variant.");

                return $"variant {variant.Width}x{variant.Height} with {variant.HlsSegmentFiles.Count} segments, {variant.TotalByteSize} bytes";
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static async Task<string> VerifyRegistrationAsync()
        {
            // User clients.
            var userServices = new ServiceCollection();
            userServices.AddEthernaUserClients(
                    "probeClientId",
                    null,
                    11420,
                    authority: "https://sso.example.com/")
                .AddEthernaCreditClient()
                .AddEthernaGatewayClient()
                .AddEthernaIndexClient()
                .AddEthernaSsoClient();
            await using var userProvider = userServices.BuildServiceProvider();
            _ = userProvider.GetRequiredService<IEthernaUserCreditClient>();
            _ = userProvider.GetRequiredService<IEthernaUserGatewayClient>();
            _ = userProvider.GetRequiredService<IEthernaUserIndexClientFactory>()
                .Create(new Uri("https://index.example.com/"));
            _ = userProvider.GetRequiredService<IEthernaUserSsoClient>();
            _ = userProvider.GetRequiredService<ISwarmClient>();

            // Internal clients.
            var internalServices = new ServiceCollection();
            internalServices.AddEthernaInternalClients(new Uri("https://sso.example.com/"))
                .AddEthernaCreditClient(new Uri("https://credit.example.com/"), "probeClientId", "probeClientSecret")
                .AddEthernaSsoClient("probeClientId", "probeClientSecret");
            await using var internalProvider = internalServices.BuildServiceProvider();
            _ = internalProvider.GetRequiredService<IEthernaInternalCreditClient>();
            _ = internalProvider.GetRequiredService<IEthernaInternalSsoClient>();

            return "resolved the three user clients, the index client factory, the swarm client and the two internal clients, and created an index client";
        }
    }
}
