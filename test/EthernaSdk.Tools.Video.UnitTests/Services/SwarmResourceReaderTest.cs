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

using Etherna.Sdk.Tools.UniversalFiles;
using Etherna.Sdk.Tools.UniversalFiles.Extensions;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Hashing;
using Etherna.SwarmSdk.Hashing.Pipeline;
using Etherna.SwarmSdk.Hashing.Postage;
using Etherna.SwarmSdk.Manifest;
using Etherna.SwarmSdk.Models;
using Etherna.SwarmSdk.Services;
using Etherna.SwarmSdk.Stores;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Tools.Video.Services
{
    public class SwarmResourceReaderTest
    {
        // Classes.
        private sealed class CountingChunkStore(IReadOnlyChunkStore sourceChunkStore)
            : ReadOnlyChunkStoreBase
        {
            // Properties.
            public Dictionary<SwarmHash, int> Reads { get; } = new();

            // Methods.
            public override async Task<SwarmChunk> GetAsync(SwarmHash hash, CancellationToken cancellationToken = default)
            {
                Reads[hash] = Reads.GetValueOrDefault(hash) + 1;
                return await sourceChunkStore.GetAsync(hash, cancellationToken);
            }

            public override Task<bool> HasChunkAsync(SwarmHash hash, CancellationToken cancellationToken = default) =>
                sourceChunkStore.HasChunkAsync(hash, cancellationToken);
        }

        // Fields.
        private readonly MemoryChunkStore memoryChunkStore = new();

        // Tests.
        [Fact]
        public async Task GetFileStreamAsync_WithFileProvider_ReadsFilesThroughSwarmClient()
        {
            // Setup.
            var rootReference = await UploadManifestHelper(memoryChunkStore, new Dictionary<string, SwarmReference>
            {
                ["preview"] = await UploadStringFileHelper("preview content", memoryChunkStore),
                ["details"] = await UploadStringFileHelper("details content", memoryChunkStore)
            });
            var chunkStore = new CountingChunkStore(memoryChunkStore);
            List<SwarmAddress> requestedAddresses = [];
            var uFileProvider = new UFileProvider(new Mock<IHttpClientFactory>().Object)
                .UseSwarmUFiles(BuildSwarmClientMockHelper(memoryChunkStore, requestedAddresses).Object);
            var resourceReader = new SwarmResourceReader(new ChunkService(), chunkStore, uFileProvider);

            // Action.
            var previewContent = await ReadToStringHelper(
                await resourceReader.GetFileStreamAsync(rootReference));
            var detailsContent = await ReadToStringHelper(
                await resourceReader.GetFileStreamAsync(new SwarmAddress(rootReference, "details")));

            // Assert.
            Assert.Equal("preview content", previewContent);
            Assert.Equal("details content", detailsContent);
            Assert.Equal(
                [new SwarmAddress(rootReference), new SwarmAddress(rootReference, "details")],
                requestedAddresses);
            Assert.Empty(chunkStore.Reads);
        }

        [Fact]
        public async Task ResolveReferenceAsync_FetchesEachManifestNodeOnce()
        {
            // Setup.
            var rootReference = await UploadManifestHelper(memoryChunkStore, new Dictionary<string, SwarmReference>
            {
                ["a/1"] = "0000000000000000000000000000000000000000000000000000000000000001",
                ["a/2"] = "0000000000000000000000000000000000000000000000000000000000000002",
                ["b/3"] = "0000000000000000000000000000000000000000000000000000000000000003"
            });
            var chunkStore = new CountingChunkStore(memoryChunkStore);
            var resourceReader = new SwarmResourceReader(new ChunkService(), chunkStore);

            // Action.
            List<SwarmReference> references = [];
            foreach (var path in new[] { "a/1", "a/2", "b/3", "a/1" })
                references.Add(await resourceReader.ResolveReferenceAsync(new SwarmAddress(rootReference, path)));

            // Assert.
            Assert.Equal(
                [
                    "0000000000000000000000000000000000000000000000000000000000000001",
                    "0000000000000000000000000000000000000000000000000000000000000002",
                    "0000000000000000000000000000000000000000000000000000000000000003",
                    "0000000000000000000000000000000000000000000000000000000000000001"
                ],
                references);
            Assert.Equal(1, chunkStore.Reads[rootReference.Hash]);
            Assert.All(chunkStore.Reads.Values, reads => Assert.Equal(1, reads));
        }

        // Helpers.
        private static Mock<ISwarmClient> BuildSwarmClientMockHelper(
            IReadOnlyChunkStore chunkStore,
            List<SwarmAddress> requestedAddresses)
        {
            var swarmClientMock = new Mock<ISwarmClient>();
            swarmClientMock.Setup(c => c.GetFileAsync(
                    It.IsAny<SwarmAddress>(),
                    It.IsAny<bool?>(),
                    It.IsAny<RedundancyLevel?>(),
                    It.IsAny<RedundancyStrategy?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string?>(),
                    It.IsAny<int?>(),
                    It.IsAny<long?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new InvocationFunc(invocation =>
                {
                    var address = (SwarmAddress)invocation.Arguments[0];
                    requestedAddresses.Add(address);
                    return GetFileResponseHelper(address, chunkStore);
                }));
            return swarmClientMock;
        }

        private static async Task<FileResponse> GetFileResponseHelper(
            SwarmAddress address,
            IReadOnlyChunkStore chunkStore) =>
            new(null,
                new Dictionary<string, IEnumerable<string>>(),
                await new ChunkService().GetFileStreamFromAddressAsync(
                    address,
                    ManifestPathResolver.BrowserResolver,
                    chunkStore));

        private static async Task<string> ReadToStringHelper(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        private static async Task<SwarmReference> UploadManifestHelper(
            IChunkStore chunkStore,
            IReadOnlyDictionary<string, SwarmReference> files)
        {
            var manifest = new WritableMantarayManifest(
                chunkStore,
                new FakePostageStamper(),
                RedundancyLevel.None,
                false,
                0,
                null);
            manifest.Add(
                MantarayManifestBase.RootPath,
                ManifestEntry.NewDirectory(
                    new Dictionary<string, string>
                    {
                        [ManifestEntry.WebsiteIndexDocPathKey] = "preview"
                    }));
            foreach (var (path, reference) in files)
                manifest.Add(path, ManifestEntry.NewFile(reference, new Dictionary<string, string>()));
            return await manifest.GetReferenceAsync(new Hasher());
        }

        private static async Task<SwarmReference> UploadStringFileHelper(
            string strValue,
            IChunkStore chunkStore)
        {
            using var fileHasherPipeline = HasherPipelineBuilder.BuildNewHasherPipeline(
                chunkStore,
                new FakePostageStamper(),
                RedundancyLevel.None,
                false,
                0,
                null);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(strValue));
            return await fileHasherPipeline.HashDataAsync(stream);
        }
    }
}
