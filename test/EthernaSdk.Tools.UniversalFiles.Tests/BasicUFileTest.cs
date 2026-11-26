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

using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Tools.UniversalFiles
{
    public class BasicUFileTest
    {
        // Consts.
        private const string OnlineFileCharSet = "iso-8859-1";
        private const string OnlineFileUri = "https://user:secret@files.test/video.mp4?token=secret";
        private const string OnlineFileUriWithoutSecrets = "https://files.test/video.mp4";

        // Fields.
        private static readonly byte[] OnlineFileContent = Encoding.Latin1.GetBytes("Città di provincia");

        // Classes.
        private sealed class DisposalTrackingStream(byte[] buffer) : MemoryStream(buffer)
        {
            public bool IsDisposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        private sealed class InterruptedContent : HttpContent
        {
            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                await stream.WriteAsync(OnlineFileContent.AsMemory(0, 4));
                throw new IOException("The connection was reset");
            }

            protected override bool TryComputeLength(out long length)
            {
                length = OnlineFileContent.Length;
                return true;
            }
        }

        private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = respond(request);
                response.RequestMessage = request;
                return Task.FromResult(response);
            }
        }

        /// <summary>
        /// A content with no declared length, as the one of a response without Content-Length
        /// </summary>
        private sealed class UnsizedContent : HttpContent
        {
            protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
                Task.CompletedTask;

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }

        // Tests.
        [Fact]
        public async Task ExistsAsync_DownloadsAnOnlineFileWhoseHeadIsRefused()
        {
            // Setup.
            var requestMethods = new List<HttpMethod>();
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(request =>
                {
                    requestMethods.Add(request.Method);
                    return request.Method == HttpMethod.Head ?
                        new HttpResponseMessage(HttpStatusCode.MethodNotAllowed) :
                        BuildOnlineFileResponse();
                }));

            // Action.
            var exists = await uFile.ExistsAsync();

            // Assert.
            Assert.True(exists);
            Assert.Equal([HttpMethod.Head, HttpMethod.Get], requestMethods);
        }

        [Fact]
        public async Task ExistsAsync_FindsAServedOnlineFileWithAHeadRequest()
        {
            // Setup.
            var requestMethods = new List<HttpMethod>();
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(request =>
                {
                    requestMethods.Add(request.Method);
                    return request.Method == HttpMethod.Head ?
                        new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnsizedContent() } :
                        BuildOnlineFileResponse();
                }));

            // Action.
            var exists = await uFile.ExistsAsync();

            // Assert.
            Assert.True(exists);
            Assert.Equal([HttpMethod.Head], requestMethods);
        }

        [Fact]
        public async Task ExistsAsync_LogsFailedOnlineRequestsWithoutUriSecrets()
        {
            // Setup.
            var logger = new RecordingLogger<BasicUFile>();
            var requestFailure = new HttpRequestException("Connection refused");
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(() => throw requestFailure),
                logger);

            // Action.
            var exists = await uFile.ExistsAsync();

            // Assert.
            Assert.False(exists);
            Assert.Equal(
                new[]
                {
                    (LogLevel.Warning, "OnlineFileHeadRequestFailed"),
                    (LogLevel.Warning, "OnlineFileRequestFailed")
                },
                logger.Events.Select(e => (e.Level, e.Name!)));
            Assert.All(logger.Events, e =>
            {
                Assert.Same(requestFailure, e.Exception);
                Assert.Contains(OnlineFileUriWithoutSecrets, e.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("secret", e.Message, StringComparison.Ordinal);
            });
        }

        [Fact]
        public async Task ExistsAsync_LogsRefusedOnlineRequests()
        {
            // Setup.
            var logger = new RecordingLogger<BasicUFile>();
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(() => new HttpResponseMessage(HttpStatusCode.NotFound)),
                logger);

            // Action.
            var exists = await uFile.ExistsAsync();

            // Assert.
            Assert.False(exists);
            Assert.Equal(
                new[]
                {
                    (LogLevel.Debug, "OnlineFileHeadRequestRefused"),
                    (LogLevel.Debug, "OnlineFileRequestRefused")
                },
                logger.Events.Select(e => (e.Level, e.Name!)));
            Assert.All(logger.Events, e =>
            {
                Assert.Contains(OnlineFileUriWithoutSecrets, e.Message, StringComparison.Ordinal);
                Assert.Contains("NotFound", e.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("secret", e.Message, StringComparison.Ordinal);
            });
        }

        [Fact]
        public async Task GetByteSizeAsync_DownloadsAnOnlineFileWhoseHeadDeclaresNoSize()
        {
            // Setup.
            var requestMethods = new List<HttpMethod>();
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(request =>
                {
                    requestMethods.Add(request.Method);
                    return request.Method == HttpMethod.Head ?
                        new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnsizedContent() } :
                        BuildOnlineFileResponse();
                }));

            // Action.
            var byteSize = await uFile.GetByteSizeAsync();

            // Assert.
            Assert.Equal(OnlineFileContent.Length, byteSize);
            Assert.Equal([HttpMethod.Head, HttpMethod.Get], requestMethods);
        }

        [Fact]
        public async Task GetByteSizeAsync_DownloadsAnOnlineFileWhoseHeadIsRefused()
        {
            // Setup.
            var requestMethods = new List<HttpMethod>();
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(request =>
                {
                    requestMethods.Add(request.Method);
                    return request.Method == HttpMethod.Head ?
                        new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new ByteArrayContent(new byte[243]) } :
                        BuildOnlineFileResponse();
                }));

            // Action.
            var byteSize = await uFile.GetByteSizeAsync();

            // Assert.
            Assert.Equal(OnlineFileContent.Length, byteSize);
            Assert.Equal([HttpMethod.Head, HttpMethod.Get], requestMethods);
        }

        [Fact]
        public async Task GetByteSizeAsync_SizesAServedOnlineFileWithAHeadRequest()
        {
            // Setup.
            var requestMethods = new List<HttpMethod>();
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(request =>
                {
                    requestMethods.Add(request.Method);
                    return BuildOnlineFileResponse();
                }));

            // Action.
            var byteSize = await uFile.GetByteSizeAsync();

            // Assert.
            Assert.Equal(OnlineFileContent.Length, byteSize);
            Assert.Equal([HttpMethod.Head], requestMethods);
        }

        [Theory]
        [InlineData(nameof(UFile.GetByteSizeAsync))]
        [InlineData(nameof(UFile.ReadToByteArrayAsync))]
        [InlineData(nameof(UFile.ReadToStreamAsync))]
        public async Task OnlineReads_ThrowIOExceptionNamingTheUriWithoutSecrets(string readMethod)
        {
            // Setup.
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(() => new HttpResponseMessage(HttpStatusCode.NotFound)));

            // Action.
            var exception = await Assert.ThrowsAsync<IOException>(() => readMethod switch
            {
                nameof(UFile.GetByteSizeAsync) => uFile.GetByteSizeAsync(),
                nameof(UFile.ReadToByteArrayAsync) => uFile.ReadToByteArrayAsync(),
                nameof(UFile.ReadToStreamAsync) => uFile.ReadToStreamAsync(),
                _ => throw new ArgumentOutOfRangeException(nameof(readMethod))
            });

            // Assert.
            Assert.Contains(OnlineFileUriWithoutSecrets, exception.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("secret", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ReadToByteArrayAsync_LogsInterruptedOnlineDownload()
        {
            // Setup.
            var logger = new RecordingLogger<BasicUFile>();
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(() => new HttpResponseMessage(HttpStatusCode.OK) { Content = new InterruptedContent() }),
                logger);

            // Action.
            await Assert.ThrowsAsync<IOException>(() => uFile.ReadToByteArrayAsync());

            // Assert.
            var logEvent = Assert.Single(logger.Events);
            Assert.Equal((LogLevel.Warning, "OnlineFileRequestFailed"), (logEvent.Level, logEvent.Name));
        }

        [Fact]
        public async Task ReadToByteArrayAsync_ReadsAServedOnlineFile()
        {
            // Setup.
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(BuildOnlineFileResponse));

            // Action.
            var (byteArray, encoding) = await uFile.ReadToByteArrayAsync();

            // Assert.
            Assert.Equal(OnlineFileContent, byteArray);
            Assert.Equal(OnlineFileCharSet, encoding?.WebName);
        }

        [Fact]
        public async Task ReadToStreamAsync_DisposesTheResponseWithTheStream()
        {
            // Setup.
            var responseContentStream = new DisposalTrackingStream(OnlineFileContent);
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(responseContentStream)
                }));

            // Action.
            var (stream, _) = await uFile.ReadToStreamAsync();
            var isDisposedBeforeStream = responseContentStream.IsDisposed;
            await stream.DisposeAsync();

            // Assert.
            Assert.False(isDisposedBeforeStream);
            Assert.True(responseContentStream.IsDisposed);
        }

        [Fact]
        public async Task ReadToStreamAsync_ReadsAServedOnlineFile()
        {
            // Setup.
            var uFile = new BasicUFile(
                new BasicUUri(OnlineFileUri),
                BuildHttpClientFactory(BuildOnlineFileResponse));

            // Action.
            var (stream, encoding) = await uFile.ReadToStreamAsync();
            await using var _ = stream;
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            // Assert.
            Assert.Equal(OnlineFileContent, memoryStream.ToArray());
            Assert.Equal(OnlineFileCharSet, encoding?.WebName);
        }

        // Helpers.
        private static IHttpClientFactory BuildHttpClientFactory(Func<HttpResponseMessage> respond) =>
            BuildHttpClientFactory(_ => respond());

        private static IHttpClientFactory BuildHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(() => new HttpClient(new StubHttpMessageHandler(respond)));
            return httpClientFactoryMock.Object;
        }

        private static HttpResponseMessage BuildOnlineFileResponse() =>
            new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(OnlineFileContent)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = OnlineFileCharSet } }
                }
            };
    }
}
