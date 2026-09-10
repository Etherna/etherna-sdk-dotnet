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
using Etherna.Sdk.Tools.Video.Models;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Hashing;
using Etherna.SwarmSdk.Hashing.Pipeline;
using Etherna.SwarmSdk.Hashing.Postage;
using Etherna.SwarmSdk.Manifest;
using Etherna.SwarmSdk.Models;
using Etherna.SwarmSdk.Services;
using Etherna.SwarmSdk.Stores;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Tools.Video.Services
{
    public class VideoManifestServiceTest
    {
        // Classes.
        public class ParseManifestTestElement(
            Func<IChunkStore, Task<SwarmReference>> uploadContentsAsync,
            PublishedVideoManifest expectedManifest)
        {
            public Func<IChunkStore, Task<SwarmReference>> UploadContentsAsync { get; } = uploadContentsAsync;
            public PublishedVideoManifest ExpectedManifest { get; } = expectedManifest;
        }

        // Data.
        public static IEnumerable<object[]> ParseManifestTests
        {
            get
            {
                var tests = new List<ParseManifestTestElement>();
                
                //v2.1
                {
                    tests.Add(new(
                        async chunkStore =>
                        {
                            var manifest = BuildNewManifestHelper(chunkStore);
                            AddRootFileToManifestHelper(manifest, "preview", await UploadStringFileHelper(
                                """{"v":"2.1","title":"I'm a title","createdAt":1724285934,"updatedAt":1724285934,"ownerAddress":"0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246","duration":587,"thumbnail":{"aspectRatio":1.7777778,"blurhash":"UEENPeIpX=w=f*j[ngRktTj=adtSaKjYo#R*","sources":[{"width":480,"type":"jpeg","path":"thumb/270.jpg"},{"width":960,"type":"jpeg","path":"thumb/540.jpg"}]}}""", chunkStore));
                            AddFileToManifestHelper(manifest, "details", await UploadStringFileHelper(
                                """{"description":"my description","aspectRatio":1.7777778,"batchId":"9d4d4e923cc054a94f7884772f5f4e588be0c3ec10c3ec397b64c7307c4c3336","personalData":"{\u0022v\u0022:\u00221\u0022,\u0022cliName\u0022:\u0022EthernaImporter\u0022,\u0022cliV\u0022:\u00220.3.9.0\u0022,\u0022srcName\u0022:\u0022youtube\u0022,\u0022srcVId\u0022:\u00225a7ace3d1c209f7f1982fa4ed0113d8fb35bdbf2698761cb4f8da276bbef3993\u0022}","sources":[{"type":"hls","path":"sources/hls/master.m3u8","size":0},{"type":"hls","quality":"480p","path":"sources/hls/480p/playlist.m3u8","size":2471},{"type":"hls","quality":"360p","path":"sources/hls/360p/playlist.m3u8","size":2448}],"captions":[{"label":"Arabic","lang":"ar","path":"captions/0.vtt"},{"label":"Bulgarian","lang":"bg","path":"captions/1.vtt"},{"label":"Catalan","lang":"ca","path":"captions/2.vtt"}]}""", chunkStore));
                            AddFileToManifestHelper(manifest, "sources/hls/master.m3u8", "0000000000000000000000000000000000000000000000000000000000000012");
                            AddFileToManifestHelper(manifest, "sources/hls/480p/playlist.m3u8", await UploadStringFileHelper(
                                """
                                #EXTM3U
                                #EXT-X-VERSION:3
                                #EXT-X-TARGETDURATION:13
                                #EXT-X-MEDIA-SEQUENCE:0
                                #EXT-X-PLAYLIST-TYPE:VOD
                                #EXTINF:9.800000,
                                0.ts
                                #EXTINF:10.000000,
                                1.ts
                                #EXTINF:10.000000,
                                2.ts
                                #EXT-X-ENDLIST
                                """, chunkStore));
                            AddFileToManifestHelper(manifest, "sources/hls/480p/0.ts", "0000000000000000000000000000000000000000000000000000000000000001");
                            AddFileToManifestHelper(manifest, "sources/hls/480p/1.ts", "0000000000000000000000000000000000000000000000000000000000000002");
                            AddFileToManifestHelper(manifest, "sources/hls/480p/2.ts", "0000000000000000000000000000000000000000000000000000000000000003");
                            AddFileToManifestHelper(manifest, "sources/hls/360p/playlist.m3u8", await UploadStringFileHelper(
                                """
                                #EXTM3U
                                #EXT-X-VERSION:3
                                #EXT-X-TARGETDURATION:14
                                #EXT-X-MEDIA-SEQUENCE:0
                                #EXT-X-PLAYLIST-TYPE:VOD
                                #EXTINF:9.800000,
                                0.ts
                                #EXTINF:10.000000,
                                1.ts
                                #EXTINF:10.000000,
                                2.ts
                                #EXT-X-ENDLIST
                                """, chunkStore));
                            AddFileToManifestHelper(manifest, "sources/hls/360p/0.ts", "0000000000000000000000000000000000000000000000000000000000000004");
                            AddFileToManifestHelper(manifest, "sources/hls/360p/1.ts", "0000000000000000000000000000000000000000000000000000000000000005");
                            AddFileToManifestHelper(manifest, "sources/hls/360p/2.ts", "0000000000000000000000000000000000000000000000000000000000000006");
                            AddFileToManifestHelper(manifest, "thumb/270.jpg", "0000000000000000000000000000000000000000000000000000000000000010");
                            AddFileToManifestHelper(manifest, "thumb/540.jpg", "0000000000000000000000000000000000000000000000000000000000000011");
                            AddFileToManifestHelper(manifest, "captions/0.vtt", "0000000000000000000000000000000000000000000000000000000000000007");
                            AddFileToManifestHelper(manifest, "captions/1.vtt", "0000000000000000000000000000000000000000000000000000000000000008");
                            AddFileToManifestHelper(manifest, "captions/2.vtt", "0000000000000000000000000000000000000000000000000000000000000009");

                            return (await manifest.GetReferenceAsync(new Hasher()).ConfigureAwait(false)).Hash;
                        },
                        new PublishedVideoManifest(
                            "845238e3bd8110f88709768946394b8bcf562f0f0253c3c39df1cacaf3800ddd", 
                            new VideoManifest(
                                1.7777778f,
                                DateTimeOffset.Parse("8/22/2024 12:18:54 AM +00:00"),
                                "my description",
                                TimeSpan.FromSeconds(587),
                                "I'm a title",
                                "0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246",
                                """{"v":"1","cliName":"EthernaImporter","cliV":"0.3.9.0","srcName":"youtube","srcVId":"5a7ace3d1c209f7f1982fa4ed0113d8fb35bdbf2698761cb4f8da276bbef3993"}""",
                                [
                                    new VideoManifestVideoSource(
                                        "master.m3u8",
                                        VideoType.Hls,
                                        null,
                                        0,
                                        [],
                                        "0000000000000000000000000000000000000000000000000000000000000012"),
                                    new VideoManifestVideoSource(
                                        "480p/playlist.m3u8",
                                        VideoType.Hls,
                                        "480p",
                                        2471,
                                        [
                                            new VideoManifestVideoSourceAdditionalFile("480p/0.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000001"),
                                            new VideoManifestVideoSourceAdditionalFile("480p/1.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000002"),
                                            new VideoManifestVideoSourceAdditionalFile("480p/2.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000003")
                                        ],
                                        "98551f01c134dcf52cc97b66d4dfb8974f3667768de29c050b66ee7b9031cc90"),
                                    new VideoManifestVideoSource(
                                        "360p/playlist.m3u8",
                                        VideoType.Hls,
                                        "360p",
                                        2448,
                                        [
                                            new VideoManifestVideoSourceAdditionalFile("360p/0.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000004"),
                                            new VideoManifestVideoSourceAdditionalFile("360p/1.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000005"),
                                            new VideoManifestVideoSourceAdditionalFile("360p/2.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000006")
                                        ],
                                        "91e5edc7824b52cd711041f597f2ff3e1f32a646f3d648a486246ffca484712d")
                                ],
                                new VideoManifestImage(
                                    1.7777778f,
                                    "UEENPeIpX=w=f*j[ngRktTj=adtSaKjYo#R*",
                                    [
                                        new VideoManifestImageSource(
                                            "270.jpg",
                                            ImageType.Jpeg,
                                            480,
                                            "0000000000000000000000000000000000000000000000000000000000000010"),
                                        new VideoManifestImageSource(
                                            "540.jpg",
                                            ImageType.Jpeg,
                                            960,
                                            "0000000000000000000000000000000000000000000000000000000000000011")
                                    ]),
                                [
                                    new VideoManifestCaptionSource(
                                        "Arabic",
                                        "ar",
                                        "0.vtt",
                                        "0000000000000000000000000000000000000000000000000000000000000007"),
                                    new VideoManifestCaptionSource(
                                        "Bulgarian",
                                        "bg",
                                        "1.vtt",
                                        "0000000000000000000000000000000000000000000000000000000000000008"),
                                    new VideoManifestCaptionSource(
                                        "Catalan",
                                        "ca",
                                        "2.vtt",
                                        "0000000000000000000000000000000000000000000000000000000000000009")
                                ],
                                DateTimeOffset.Parse("8/22/2024 12:18:54 AM +00:00")),
                            [],
                            new Version(2, 1))));
                }
                
                //v2.0
                {
                    tests.Add(new(
                        async chunkStore =>
                        {
                            var manifest = BuildNewManifestHelper(chunkStore);
                            AddRootFileToManifestHelper(manifest, "preview", await UploadStringFileHelper(
                                """{"v":"2.0","title":"I'm a title","createdAt":1724285934,"updatedAt":1724285934,"ownerAddress":"0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246","duration":587,"thumbnail":{"aspectRatio":1.7777778,"blurhash":"UEENPeIpX=w=f*j[ngRktTj=adtSaKjYo#R*","sources":[{"width":480,"type":"jpeg","path":"thumb/270.jpg"},{"width":960,"type":"jpeg","path":"thumb/540.jpg"}]}}""", chunkStore));
                            AddFileToManifestHelper(manifest, "details", await UploadStringFileHelper(
                                """{"description":"my description","aspectRatio":1.7777778,"batchId":"9d4d4e923cc054a94f7884772f5f4e588be0c3ec10c3ec397b64c7307c4c3336","personalData":"{\u0022v\u0022:\u00221\u0022,\u0022cliName\u0022:\u0022EthernaImporter\u0022,\u0022cliV\u0022:\u00220.3.9.0\u0022,\u0022srcName\u0022:\u0022youtube\u0022,\u0022srcVId\u0022:\u00225a7ace3d1c209f7f1982fa4ed0113d8fb35bdbf2698761cb4f8da276bbef3993\u0022}","sources":[{"type":"hls","path":"sources/hls/master.m3u8","size":0},{"type":"hls","quality":"480p","path":"sources/hls/480p/playlist.m3u8","size":2471},{"type":"hls","quality":"360p","path":"sources/hls/360p/playlist.m3u8","size":2448}]}""", chunkStore));
                            AddFileToManifestHelper(manifest, "sources/hls/master.m3u8", "0000000000000000000000000000000000000000000000000000000000000012");
                            AddFileToManifestHelper(manifest, "sources/hls/480p/playlist.m3u8", await UploadStringFileHelper(
                                """
                                #EXTM3U
                                #EXT-X-VERSION:3
                                #EXT-X-TARGETDURATION:13
                                #EXT-X-MEDIA-SEQUENCE:0
                                #EXT-X-PLAYLIST-TYPE:VOD
                                #EXTINF:9.800000,
                                0.ts
                                #EXTINF:10.000000,
                                1.ts
                                #EXTINF:10.000000,
                                2.ts
                                #EXT-X-ENDLIST
                                """, chunkStore));
                            AddFileToManifestHelper(manifest, "sources/hls/480p/0.ts", "0000000000000000000000000000000000000000000000000000000000000001");
                            AddFileToManifestHelper(manifest, "sources/hls/480p/1.ts", "0000000000000000000000000000000000000000000000000000000000000002");
                            AddFileToManifestHelper(manifest, "sources/hls/480p/2.ts", "0000000000000000000000000000000000000000000000000000000000000003");
                            AddFileToManifestHelper(manifest, "sources/hls/360p/playlist.m3u8", await UploadStringFileHelper(
                                """
                                #EXTM3U
                                #EXT-X-VERSION:3
                                #EXT-X-TARGETDURATION:14
                                #EXT-X-MEDIA-SEQUENCE:0
                                #EXT-X-PLAYLIST-TYPE:VOD
                                #EXTINF:9.800000,
                                0.ts
                                #EXTINF:10.000000,
                                1.ts
                                #EXTINF:10.000000,
                                2.ts
                                #EXT-X-ENDLIST
                                """, chunkStore));
                            AddFileToManifestHelper(manifest, "sources/hls/360p/0.ts", "0000000000000000000000000000000000000000000000000000000000000004");
                            AddFileToManifestHelper(manifest, "sources/hls/360p/1.ts", "0000000000000000000000000000000000000000000000000000000000000005");
                            AddFileToManifestHelper(manifest, "sources/hls/360p/2.ts", "0000000000000000000000000000000000000000000000000000000000000006");
                            AddFileToManifestHelper(manifest, "thumb/270.jpg", "0000000000000000000000000000000000000000000000000000000000000010");
                            AddFileToManifestHelper(manifest, "thumb/540.jpg", "0000000000000000000000000000000000000000000000000000000000000011");

                            return (await manifest.GetReferenceAsync(new Hasher()).ConfigureAwait(false)).Hash;
                        },
                        new PublishedVideoManifest(
                            "1e43dd2d93290662dc37598b2d12ce1e3ac39f85f07195087f593a768c69b4c1",
                            new VideoManifest(
                                1.7777778f,
                                DateTimeOffset.Parse("8/22/2024 12:18:54 AM +00:00"),
                                "my description",
                                TimeSpan.FromSeconds(587),
                                "I'm a title",
                                "0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246",
                                """{"v":"1","cliName":"EthernaImporter","cliV":"0.3.9.0","srcName":"youtube","srcVId":"5a7ace3d1c209f7f1982fa4ed0113d8fb35bdbf2698761cb4f8da276bbef3993"}""",
                                [
                                    new VideoManifestVideoSource(
                                        "master.m3u8",
                                        VideoType.Hls,
                                        null,
                                        0,
                                        [],
                                        "0000000000000000000000000000000000000000000000000000000000000012"),
                                    new VideoManifestVideoSource(
                                        "480p/playlist.m3u8",
                                        VideoType.Hls,
                                        "480p",
                                        2471,
                                        [
                                            new VideoManifestVideoSourceAdditionalFile("480p/0.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000001"),
                                            new VideoManifestVideoSourceAdditionalFile("480p/1.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000002"),
                                            new VideoManifestVideoSourceAdditionalFile("480p/2.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000003")
                                        ],
                                        "98551f01c134dcf52cc97b66d4dfb8974f3667768de29c050b66ee7b9031cc90"),
                                    new VideoManifestVideoSource(
                                        "360p/playlist.m3u8",
                                        VideoType.Hls,
                                        "360p",
                                        2448,
                                        [
                                            new VideoManifestVideoSourceAdditionalFile("360p/0.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000004"),
                                            new VideoManifestVideoSourceAdditionalFile("360p/1.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000005"),
                                            new VideoManifestVideoSourceAdditionalFile("360p/2.ts",
                                                "0000000000000000000000000000000000000000000000000000000000000006")
                                        ],
                                        "91e5edc7824b52cd711041f597f2ff3e1f32a646f3d648a486246ffca484712d")
                                ],
                                new VideoManifestImage(
                                    1.7777778f,
                                    "UEENPeIpX=w=f*j[ngRktTj=adtSaKjYo#R*",
                                    [
                                        new VideoManifestImageSource(
                                            "270.jpg",
                                            ImageType.Jpeg,
                                            480,
                                            "0000000000000000000000000000000000000000000000000000000000000010"),
                                        new VideoManifestImageSource(
                                            "540.jpg",
                                            ImageType.Jpeg,
                                            960,
                                            "0000000000000000000000000000000000000000000000000000000000000011")
                                    ]),
                                [],
                                DateTimeOffset.Parse("8/22/2024 12:18:54 AM +00:00")),
                            [],
                            new Version(2, 0))));
                }
                
                //v1.1
                {
                    tests.Add(new(
                        async chunkStore =>
                        {
                            var manifest = BuildNewManifestHelper(chunkStore);
                            AddRootFileToManifestHelper(manifest, "manifest", await UploadStringFileHelper(
                                """{"v":"1.1","title":"title 3","description":"test!!!","duration":19,"originalQuality":"1954p","ownerAddress":"0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246","createdAt":1660397733617,"updatedAt":1660397733617,"thumbnail":{"blurhash":"UTHoa;-VEVO=??v]SlOu2ep0slR:kisia*bJ","aspectRatio":1.7777777777777777,"sources":{"720w":"5d69d94f1ffa17560a88abc4a99aa40b0cabe6012766f51e5c19193887adacb1","480w":"0b7425036143ed65932ac64cd6c4ddb4f2fd3e9bd51ed0f13bd406926c45c325"}},"sources":[{"reference":"e44671417466df08d3b67d74a081021ab2bba70224fc0d6e4d00c35d80328c6c","quality":"1954p","size":3739997,"bitrate":1574736}],"batchId":"5d35cbf4cea6349c1f74340ce9f0befd7a60a17426508da7b205871d683a3a23"}""", chunkStore));
                
                            return (await manifest.GetReferenceAsync(new Hasher()).ConfigureAwait(false)).Hash;
                        },
                        new PublishedVideoManifest(
                            "8c831938f7f10cc57a8a68a55f473868324e0c3dafe9313bfd70f4343abedb91",
                            new VideoManifest(
                                1.7777777777777777f,
                                DateTimeOffset.Parse("8/13/2022 1:35:33.617 PM +00:00"),
                                "test!!!",
                                TimeSpan.FromSeconds(19),
                                "title 3",
                                "0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246",
                                personalData: null,
                                [
                                    new VideoManifestVideoSource(
                                        "1954p.mp4",
                                        VideoType.Mp4,
                                        "1954p",
                                        3739997,
                                        [],
                                        "e44671417466df08d3b67d74a081021ab2bba70224fc0d6e4d00c35d80328c6c")
                                ],
                                new VideoManifestImage(
                                    1.7777777777777777f,
                                    "UTHoa;-VEVO=??v]SlOu2ep0slR:kisia*bJ",
                                    [
                                        new VideoManifestImageSource(
                                            "720.jpg",
                                            ImageType.Jpeg,
                                            720,
                                            "5d69d94f1ffa17560a88abc4a99aa40b0cabe6012766f51e5c19193887adacb1"),
                                        new VideoManifestImageSource(
                                            "480.jpg",
                                            ImageType.Jpeg,
                                            480,
                                            "0b7425036143ed65932ac64cd6c4ddb4f2fd3e9bd51ed0f13bd406926c45c325")
                                    ]),
                                [],
                                updatedAt: DateTimeOffset.Parse("8/13/2022 1:35:33.617 PM +00:00")),
                            [],
                            new Version(1, 1))));
                }
                
                //v1.0
                {
                    tests.Add(new(
                        async chunkStore =>
                        {
                            var manifest = BuildNewManifestHelper(chunkStore);
                            AddRootFileToManifestHelper(manifest, "manifest", await UploadStringFileHelper(
                                """{"title":"Test 1","description":"desc","createdAt":1645091199100,"duration":18,"originalQuality":"720p","ownerAddress":"0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246","thumbnail":{"blurhash":"UTHoa;-VEVO=??v]SlOu2ep0slR:kisia*bJ","aspectRatio":1.7777777777777777,"sources":{"720w":"5d69d94f1ffa17560a88abc4a99aa40b0cabe6012766f51e5c19193887adacb1","480w":"0b7425036143ed65932ac64cd6c4ddb4f2fd3e9bd51ed0f13bd406926c45c325"}},"sources":[{"quality":"720p","reference":"94f4fcb1a902597c2bc53c5b48637af952a99328ec299f33e129740818a9e302","size":448350,"bitrate":216398}],"v":"1.0"}""", chunkStore));
                
                            return (await manifest.GetReferenceAsync(new Hasher()).ConfigureAwait(false)).Hash;
                        },
                        new PublishedVideoManifest(
                            "4a6dc04a9c07b9987ae18e3e8ea3185e828efdce9e27ea30ad644b762b531cb4",
                            new VideoManifest(
                                1.7777777777777777f,
                                DateTimeOffset.Parse("2/17/2022 9:46:39.100 AM +00:00"),
                                "desc",
                                TimeSpan.FromSeconds(18),
                                "Test 1",
                                "0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246",
                                personalData: null,
                                [
                                    new VideoManifestVideoSource(
                                        "720p.mp4",
                                        VideoType.Mp4,
                                        "720p",
                                        448350,
                                        [],
                                        "94f4fcb1a902597c2bc53c5b48637af952a99328ec299f33e129740818a9e302")
                                ],
                                new VideoManifestImage(
                                    1.7777777777777777f,
                                    "UTHoa;-VEVO=??v]SlOu2ep0slR:kisia*bJ",
                                    [
                                        new VideoManifestImageSource(
                                            "720.jpg",
                                            ImageType.Jpeg,
                                            720,
                                            "5d69d94f1ffa17560a88abc4a99aa40b0cabe6012766f51e5c19193887adacb1"),
                                        new VideoManifestImageSource(
                                            "480.jpg",
                                            ImageType.Jpeg,
                                            480,
                                            "0b7425036143ed65932ac64cd6c4ddb4f2fd3e9bd51ed0f13bd406926c45c325")
                                    ]),
                                [],
                                updatedAt: null),
                            [],
                            new Version(1, 0))));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        // Tests.
        [Fact]
        public async Task CreateVideoManifestChunksAsync()
        {
            // Setup.
            var videoManifestService = new VideoManifestService(new ChunkService());
            
            var videoManifest = new VideoManifest(
                aspectRatio: 0.123f,
                createdAt: new DateTimeOffset(2024, 07, 04, 16, 45, 42, TimeSpan.Zero),
                description: "My description",
                duration: TimeSpan.FromSeconds(42),
                title: "I'm a title",
                ownerEthAddress: "0x7cd4878e21d9ce3da6611ae27a1b73827af81374",
                personalData: "my personal data",
                videoSources:
                [
                    new VideoManifestVideoSource(
                        sourceRelativePath: "master.m3u8",
                        videoType: VideoType.Hls,
                        quality: null,
                        totalSourceSize: 0,
                        additionalFiles: [],
                        directContentReference: SwarmReference.PlainZero),
                    new VideoManifestVideoSource(
                        sourceRelativePath: "720p/playlist.m3u8",
                        videoType: VideoType.Hls,
                        quality: null,
                        totalSourceSize: 45678,
                        additionalFiles:
                        [
                            new("1.ts", SwarmReference.PlainZero),
                            new("2.ts", SwarmReference.PlainZero)
                        ],
                        directContentReference: SwarmReference.PlainZero)
                ],
                thumbnail: new VideoManifestImage(
                    aspectRatio: 0.123f,
                    "UcGkx38v?CKhoej[j[jtM|bHs:jZjaj[j@ay",
                    [
                        new VideoManifestImageSource(
                            fileName: "720.png",
                            imageType: ImageType.Png,
                            width: 720,
                            directContentReference: SwarmReference.PlainZero)
                    ]),
                captionSources:
                [
                    new VideoManifestCaptionSource(
                        "eng",
                        "en-uk",
                        "0.ts",
                        SwarmReference.PlainZero)
                ],
                updatedAt: new DateTimeOffset(2024, 07, 12, 12, 01, 08, TimeSpan.Zero));
            var chunkDirectory = Directory.CreateTempSubdirectory();

            // Run.
            var result = await videoManifestService.CreateVideoManifestChunksAsync(
                videoManifest,
                chunkDirectory.FullName);
            
            // Assert.
            Assert.Equal("1bca0eebb358c5b846188359d049d36675119317de0fc9ca5f750f49a6529e90", result);
            Assert.Equal(
                [
                    "0cc878d32c96126d47f63fbe391114ee1438cd521146fc975dea1546d302b6c0.cac",
                    "15ed8d465d278faa9a8f305fccb7d315f68659503d87a3e0e2a9f9ec9d0a99f2.cac",
                    "1bca0eebb358c5b846188359d049d36675119317de0fc9ca5f750f49a6529e90.cac",
                    "30bc8210952c3dfc47f134847e91118cd961b05ca9b45463a70ef9eb6b1530bc.cac",
                    "31accd890b1fa475306c6993587494c59b1ddc3725db2f73cf3787d918540867.cac",
                    "8504f2a107ca940beafc4ce2f6c9a9f0968c62a5b5893ff0e4e1e2983048d276.cac",
                    "adcb138c9638ba3b0a7abab11775416c300a51cc142219c691e0f05e1e6fa46c.cac",
                    "e250fc8865894b98b21a28002decf162874f00a81f44c8af96c1249bef84c3fc.cac"
                ],
                Directory.GetFiles(chunkDirectory.FullName).Select(Path.GetFileName).Order());
            
            // Cleanup.
            Directory.Delete(chunkDirectory.FullName, true);
        }
        
        [Theory, MemberData(nameof(ParseManifestTests))]
        public async Task ParseManifestAsync(ParseManifestTestElement test)
        {
            ArgumentNullException.ThrowIfNull(test);
            
            // Setup.
            var chunkStore = new MemoryChunkStore();
            var rootHash = await test.UploadContentsAsync(chunkStore);
            VideoManifestService videoManifestService = new(new ChunkService());
        
            // Action.
            var videoManifest = await videoManifestService.GetPublishedVideoManifestAsync(rootHash, chunkStore);
        
            // Assert.
            Assert.Equal(test.ExpectedManifest, videoManifest);
        }

        [Theory, MemberData(nameof(ParseManifestTests))]
        public async Task ParseManifestWithFileProviderAsync(ParseManifestTestElement test)
        {
            ArgumentNullException.ThrowIfNull(test);
            
            // Setup.
            var chunkStore = new MemoryChunkStore();
            var rootHash = await test.UploadContentsAsync(chunkStore);
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
                    GetFileResponseHelper((SwarmAddress)invocation.Arguments[0], chunkStore)));
            var uFileProvider = new UFileProvider(new Mock<IHttpClientFactory>().Object)
                .UseSwarmUFiles(swarmClientMock.Object);
            VideoManifestService videoManifestService = new(new ChunkService());
        
            // Action.
            var videoManifest = await videoManifestService.GetPublishedVideoManifestAsync(rootHash, chunkStore, uFileProvider);
        
            // Assert.
            Assert.Equal(test.ExpectedManifest, videoManifest);
            swarmClientMock.Verify(c => c.GetFileAsync(
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
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
        
        // Helpers.
        private static void AddFileToManifestHelper(
            WritableMantarayManifest manifest,
            string path,
            SwarmReference fileReference) =>
            manifest.Add(path, ManifestEntry.NewFile(fileReference, new Dictionary<string, string>()));
        
        private static void AddRootFileToManifestHelper(
            WritableMantarayManifest manifest,
            string rootFileName,
            SwarmReference rootFileReference)
        {
            manifest.Add(
                MantarayManifestBase.RootPath,
                ManifestEntry.NewDirectory(
                    new Dictionary<string, string>
                    {
                        [ManifestEntry.WebsiteIndexDocPathKey] = rootFileName,
                    }));

            AddFileToManifestHelper(manifest, rootFileName, rootFileReference);
        }

        private static WritableMantarayManifest BuildNewManifestHelper(IChunkStore chunkStore)
        {
            var manifest = new WritableMantarayManifest(
                chunkStore,
                new FakePostageStamper(),
                RedundancyLevel.None,
                false,
                0,
                null);
            return manifest;
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
            return await fileHasherPipeline.HashDataAsync(stream).ConfigureAwait(false);
        }
    }
}