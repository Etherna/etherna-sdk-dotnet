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
using Etherna.Sdk.Tools.Video.Models;
using Etherna.SwarmSdk.Stores;
using M3U8Parser;
using Moq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Tools.Video.Services
{
    public sealed class HlsServiceTest : IDisposable
    {
        // Consts.
        private const string MasterPlaylistText =
            """
            #EXTM3U
            #EXT-X-VERSION:3
            #EXT-X-STREAM-INF:BANDWIDTH=2149280,RESOLUTION=1280x720
            720p/playlist.m3u8
            #EXT-X-STREAM-INF:BANDWIDTH=1000000,RESOLUTION=640x360
            360p/playlist.m3u8
            """;
        private const string MediaPlaylistText =
            """
            #EXTM3U
            #EXT-X-VERSION:3
            #EXT-X-TARGETDURATION:10
            #EXTINF:9.009,
            segment0.ts
            #EXTINF:3.003,
            segment1.ts
            #EXT-X-ENDLIST
            """;

        // Fields.
        private readonly string directory = Directory.CreateTempSubdirectory("hls-service-test-").FullName;
        private readonly UFileProvider uFileProvider = new(new Mock<IHttpClientFactory>().Object);

        // Methods.
        public void Dispose() => Directory.Delete(directory, true);

        // Tests.
        [Fact]
        public async Task ParseVideoEncodingFromHlsMasterPlaylistFileAsync_ReadsTheStreamsOfALocalMasterPlaylist()
        {
            // Setup.
            var hlsService = new HlsService(uFileProvider);
            var masterFile = await BuildPlaylistFileHelper("master.m3u8", MasterPlaylistText);
            foreach (var quality in new[] { "720p", "360p" })
            {
                Directory.CreateDirectory(Path.Combine(directory, quality));
                await BuildPlaylistFileHelper(Path.Combine(quality, "playlist.m3u8"), MediaPlaylistText);
                await File.WriteAllBytesAsync(Path.Combine(directory, quality, "segment0.ts"), new byte[100]);
                await File.WriteAllBytesAsync(Path.Combine(directory, quality, "segment1.ts"), new byte[50]);
            }

            // Action.
            var encoding = await hlsService.ParseVideoEncodingFromHlsMasterPlaylistFileAsync(
                TimeSpan.FromSeconds(12),
                masterFile,
                null,
                MasterPlaylist.LoadFromText(MasterPlaylistText),
                new MemoryChunkStore());

            // Assert.
            Assert.Collection(
                encoding.Variants,
                variant => Assert.Equal((720, 1280, 2), (variant.Height, variant.Width, ((HlsVideoVariant)variant).HlsSegmentFiles.Count)),
                variant => Assert.Equal((360, 640, 2), (variant.Height, variant.Width, ((HlsVideoVariant)variant).HlsSegmentFiles.Count)));
        }

        [Fact]
        public async Task TryParseHlsMasterPlaylistFromFileAsync_ParsesAMasterPlaylist()
        {
            // Setup.
            var hlsService = new HlsService(uFileProvider);
            var playlistFile = await BuildPlaylistFileHelper("master.m3u8", MasterPlaylistText);

            // Action.
            var masterPlaylist = await hlsService.TryParseHlsMasterPlaylistFromFileAsync(playlistFile);

            // Assert.
            Assert.NotNull(masterPlaylist);
            Assert.Equal(["720p/playlist.m3u8", "360p/playlist.m3u8"], masterPlaylist.Streams.ConvertAll(s => s.Uri));
        }

        [Fact]
        public async Task TryParseHlsMasterPlaylistFromFileAsync_ReturnsNullForAMediaPlaylist()
        {
            // Setup.
            var hlsService = new HlsService(uFileProvider);
            var playlistFile = await BuildPlaylistFileHelper("playlist.m3u8", MediaPlaylistText);

            // Action.
            var masterPlaylist = await hlsService.TryParseHlsMasterPlaylistFromFileAsync(playlistFile);

            // Assert.
            Assert.Null(masterPlaylist);
        }

        // Helpers.
        private async Task<FileBase> BuildPlaylistFileHelper(string fileName, string content)
        {
            var filePath = Path.Combine(directory, fileName);
            await File.WriteAllTextAsync(filePath, content);
            return await FileBase.BuildFromUFileAsync(
                uFileProvider.BuildNewUFile(new BasicUUri(filePath, UUriKind.LocalAbsolute)));
        }
    }
}
