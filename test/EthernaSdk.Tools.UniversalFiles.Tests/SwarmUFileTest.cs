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

using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Tools.UniversalFiles
{
    public class SwarmUFileTest
    {
        // Fields.
        private static readonly SwarmAddress FileAddress = SwarmAddress.FromString($"{new string('a', 64)}/video.mp4");

        // Tests.
        [Fact]
        public async Task GetByteSizeAsync_ThrowsInvalidOperationExceptionForAFileWithNoDeclaredLength()
        {
            // Setup.
            var swarmUFile = new SwarmUFile(
                BuildSwarmClientHelper(new SwarmResponseHeaders(new Dictionary<string, IEnumerable<string>>
                {
                    ["Content-Type"] = ["video/mp4"]
                })),
                new SwarmUUri(FileAddress));

            // Action.
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => swarmUFile.GetByteSizeAsync());

            // Assert.
            Assert.Contains(FileAddress.ToString(), exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByteSizeAsync_ThrowsIOExceptionForAFileTheNodeDoesNotFind()
        {
            // Setup.
            var swarmUFile = new SwarmUFile(
                BuildSwarmClientHelper(null),
                new SwarmUUri(FileAddress));

            // Action.
            var exception = await Assert.ThrowsAsync<IOException>(() => swarmUFile.GetByteSizeAsync());

            // Assert.
            Assert.Contains(FileAddress.ToString(), exception.Message, StringComparison.Ordinal);
        }

        // Helpers.
        private static ISwarmClient BuildSwarmClientHelper(SwarmResponseHeaders? fileHeaders)
        {
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
                .ReturnsAsync(fileHeaders);
            return swarmClientMock.Object;
        }
    }
}
