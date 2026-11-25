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

using Etherna.Sdk.Tools.UniversalFiles.Extensions;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Exceptions;
using Etherna.SwarmSdk.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Tools.UniversalFiles
{
    public class UFileProviderTest
    {
        // Tests.
        [Fact]
        public async Task BuildNewUFile_SharesItsLoggerWithSwarmUFiles()
        {
            // Setup.
            var logger = new RecordingLogger<UFileProvider>();
            var headersFailure = new SwarmSdkApiException(
                "Service Unavailable",
                503,
                null,
                new Dictionary<string, IEnumerable<string>>(),
                null);
            var swarmClientMock = new Mock<ISwarmClient>();
            swarmClientMock.Setup(c => c.TryGetFileHeadersAsync(
                    It.IsAny<SwarmAddress>(),
                    It.IsAny<long?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<RedundancyLevel?>(),
                    It.IsAny<RedundancyStrategy?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(headersFailure);
            var swarmFileAddress = SwarmAddress.FromString($"{new string('a', 64)}/video.mp4");

            var uFileProvider = new UFileProvider(new Mock<IHttpClientFactory>().Object, logger)
                .UseSwarmUFiles(swarmClientMock.Object);

            // Action.
            var exists = await uFileProvider.BuildNewUFile(new SwarmUUri(swarmFileAddress)).ExistsAsync();

            // Assert.
            Assert.False(exists);
            var logEvent = Assert.Single(logger.Events);
            Assert.Equal((LogLevel.Warning, "SwarmFileHeadersRequestFailed"), (logEvent.Level, logEvent.Name));
            Assert.Same(headersFailure, logEvent.Exception);
            Assert.Contains(swarmFileAddress.ToString(), logEvent.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ToLocalUFileAsync_LogsTheLocalCopy()
        {
            // Setup.
            var logger = new RecordingLogger<UFileProvider>();
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
                .ReturnsAsync(() => new FileResponse(
                    new SwarmResponseHeaders(new Dictionary<string, IEnumerable<string>>()),
                    new MemoryStream([1, 2, 3])));
            var swarmFileAddress = SwarmAddress.FromString($"{new string('a', 64)}/video.mp4");
            var localFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var uFileProvider = new UFileProvider(new Mock<IHttpClientFactory>().Object, logger)
                .UseSwarmUFiles(swarmClientMock.Object);

            try
            {
                // Action.
                await uFileProvider.ToLocalUFileAsync(
                    uFileProvider.BuildNewUFile(new SwarmUUri(swarmFileAddress)),
                    outputUUri: new BasicUUri(localFilePath, UUriKind.LocalAbsolute));

                // Assert.
                Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(localFilePath));
                var logEvent = Assert.Single(logger.Events);
                Assert.Equal((LogLevel.Debug, "UFileCopiedToLocalFile"), (logEvent.Level, logEvent.Name));
                Assert.Contains(swarmFileAddress.ToString(), logEvent.Message, StringComparison.Ordinal);
                Assert.Contains(localFilePath, logEvent.Message, StringComparison.Ordinal);
            }
            finally
            {
                File.Delete(localFilePath);
            }
        }
    }
}
