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


using Etherna.Sdk.Users.Gateway.Clients;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Exceptions;
using Etherna.SwarmSdk.Models;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Users.Gateway.Services
{
    public class GatewayServiceTest
    {
        [Fact]
        public async Task BuyPostageBatchAsync_KeepsWaitingForUsableBatchWhenANodeReadMissesItsBound()
        {
            // Setup.
            var batchId = PostageBatchId.Zero;
            var usableBatch = new PostageBatch(
                id: batchId,
                amount: BzzValue.FromInt32(1),
                blockNumber: 0,
                depth: PostageBatch.MinDepth,
                exists: true,
                isImmutable: false,
                isUsable: true,
                label: null,
                ttl: TimeSpan.FromDays(1),
                utilization: 0);

            var swarmClientMock = new Mock<ISwarmClient>();
            swarmClientMock.Setup(c => c.ApiCompatibility).Returns(SwarmClients.Bee);
            swarmClientMock.Setup(c => c.BuyPostageBatchAsync(
                    It.IsAny<BzzValue>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<ulong?>(),
                    It.IsAny<XDaiValue?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((batchId, EthTxHash.Zero));
            swarmClientMock.SetupSequence(c => c.GetPostageBatchAsync(batchId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SwarmClientTimeoutException("SwarmClient.GetPostageBatchAsync", TimeSpan.FromSeconds(5), null))
                .ReturnsAsync(new PostageBatchResponse(usableBatch, error: null));

            var gatewayService = new GatewayService(
                new Mock<IEthernaUserGatewayClient>().Object,
                swarmClientMock.Object);

            // Action.
            var (boughtBatchId, _) = await gatewayService.BuyPostageBatchAsync(BzzValue.FromInt32(1), PostageBatch.MinDepth);

            // Assert.
            Assert.Equal(batchId, boughtBatchId);
            swarmClientMock.Verify(c => c.GetPostageBatchAsync(batchId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
