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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Tools.UniversalFiles
{
    public class BasicUFileTest
    {
        // Consts.
        private const string OnlineFileUri = "https://user:secret@files.test/video.mp4?token=secret";
        private const string OnlineFileUriWithoutSecrets = "https://files.test/video.mp4";

        // Classes.
        private sealed class StubHttpMessageHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken) =>
                Task.FromResult(respond());
        }

        // Tests.
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
        public async Task ExistsAsync_LogsRefusedOnlineRequest()
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
            var logEvent = Assert.Single(logger.Events);
            Assert.Equal((LogLevel.Debug, "OnlineFileRequestRefused"), (logEvent.Level, logEvent.Name));
            Assert.Contains(OnlineFileUriWithoutSecrets, logEvent.Message, StringComparison.Ordinal);
            Assert.Contains("NotFound", logEvent.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("secret", logEvent.Message, StringComparison.Ordinal);
        }

        // Helpers.
        private static IHttpClientFactory BuildHttpClientFactory(Func<HttpResponseMessage> respond)
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(() => new HttpClient(new StubHttpMessageHandler(respond)));
            return httpClientFactoryMock.Object;
        }
    }
}
