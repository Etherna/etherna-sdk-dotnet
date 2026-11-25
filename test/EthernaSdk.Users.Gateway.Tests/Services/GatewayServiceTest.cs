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


using Etherna.Sdk.Gateway.GenClients;
using Etherna.Sdk.Users.Gateway.Clients;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Exceptions;
using Etherna.SwarmSdk.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Users.Gateway.Services
{
    public class GatewayServiceTest
    {
        // Consts.
        private static readonly PostageBatchId BatchId = PostageBatchId.FromString(new string('a', 64));
        private const string BatchReferenceId = "batch-reference";

        // Classes.
        public class BatchNotCreatedYetTestElement(Func<Task<PostageBatchId?>> referenceRead)
        {
            public Func<Task<PostageBatchId?>> ReferenceRead { get; } = referenceRead;
        }

        /// <summary>
        /// A time provider whose timers fire at once, moving its clock forward by their due time: a wait made of
        /// delays completes without waiting, and still measures the time it would have taken.
        /// </summary>
        private sealed class InstantTimeProvider : TimeProvider
        {
            // Fields.
            private long timestamp;

            // Properties.
            public List<TimeSpan> Delays { get; } = [];
            public override long TimestampFrequency => TimeSpan.TicksPerSecond;

            // Methods.
            public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            {
                Delays.Add(dueTime);
                timestamp += dueTime.Ticks;
                callback(state);

                return Mock.Of<ITimer>();
            }

            public override long GetTimestamp() => timestamp;
        }

        public class TransientBatchReadFailureTestElement(Exception readFailure)
        {
            public Exception ReadFailure { get; } = readFailure;
        }

        // Data.
        public static IEnumerable<object[]> BatchNotCreatedYetTests
        {
            get
            {
                var tests = new List<BatchNotCreatedYetTestElement>
                {
                    //no batch id for the reference
                    new(() => Task.FromResult<PostageBatchId?>(null)),

                    //gateway failing to answer
                    new(() => Task.FromException<PostageBatchId?>(new EthernaGatewayApiException(
                        "Service Unavailable",
                        503,
                        null,
                        new Dictionary<string, IEnumerable<string>>(),
                        null)))
                };

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> TransientBatchReadFailureTests
        {
            get
            {
                var tests = new List<TransientBatchReadFailureTestElement>
                {
                    //read not answered within its bound
                    new(new SwarmClientTimeoutException(
                        "SwarmClient.GetPostageBatchAsync",
                        TimeSpan.FromSeconds(5),
                        null)),

                    //gateway timeout
                    new(new SwarmSdkApiException(
                        "Gateway Timeout",
                        504,
                        null,
                        new Dictionary<string, IEnumerable<string>>(),
                        null))
                };

                return tests.Select(t => new object[] { t });
            }
        }

        // Tests.
        [Theory]
        [InlineData(SwarmClients.Bee)]
        [InlineData(SwarmClients.Beehive)]
        public async Task BuyPostageBatchAsync_BuysNoBatchInDryMode(SwarmClients apiCompatibility)
        {
            // Setup.
            var ethernaGatewayClientMock = BuildEthernaGatewayClientMock();
            var swarmClientMock = BuildSwarmClientMock(apiCompatibility, isDryMode: true);
            var notifications = new List<string>();

            var gatewayService = new GatewayService(
                ethernaGatewayClientMock.Object,
                swarmClientMock.Object,
                new InstantTimeProvider());

            // Action.
            var (boughtBatchId, txHash) = await gatewayService.BuyPostageBatchAsync(
                BzzValue.FromInt32(1),
                PostageBatch.MinDepth,
                onWaitingBatchCreation: () => notifications.Add("waiting creation"),
                onBatchCreated: batchId => notifications.Add($"created {batchId}"),
                onWaitingBatchUsable: () => notifications.Add("waiting usable"),
                onBatchUsable: () => notifications.Add("usable"));

            // Assert.
            Assert.Equal(PostageBatchId.Zero, boughtBatchId);
            Assert.Equal(EthTxHash.Zero, txHash);
            Assert.Equal(
                new[] { "waiting creation", $"created {PostageBatchId.Zero}", "waiting usable", "usable" },
                notifications);
            ethernaGatewayClientMock.VerifyNoOtherCalls();
            swarmClientMock.Verify(
                c => c.GetPostageBatchAsync(It.IsAny<PostageBatchId>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory, MemberData(nameof(BatchNotCreatedYetTests))]
        public async Task BuyPostageBatchAsync_KeepsWaitingForBatchCreationWhenGatewayHasNoBatchIdYet(
            BatchNotCreatedYetTestElement test)
        {
            // Setup.
            var timeProvider = new InstantTimeProvider();
            var ethernaGatewayClientMock = BuildEthernaGatewayClientMock();
            ethernaGatewayClientMock.SetupSequence(c => c.TryGetNewPostageBatchIdFromPostageRefAsync(
                    BatchReferenceId,
                    It.IsAny<CancellationToken>()))
                .Returns(test.ReferenceRead)
                .ReturnsAsync(BatchId);

            var gatewayService = new GatewayService(
                ethernaGatewayClientMock.Object,
                BuildSwarmClientMock(SwarmClients.Beehive).Object,
                timeProvider);

            // Action.
            var (boughtBatchId, _) = await gatewayService.BuyPostageBatchAsync(BzzValue.FromInt32(1), PostageBatch.MinDepth);

            // Assert.
            Assert.Equal(BatchId, boughtBatchId);
            Assert.Equal(GatewayService.BatchCheckTimeSpan, Assert.Single(timeProvider.Delays));
            ethernaGatewayClientMock.Verify(
                c => c.TryGetNewPostageBatchIdFromPostageRefAsync(BatchReferenceId, It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Theory, MemberData(nameof(TransientBatchReadFailureTests))]
        public async Task BuyPostageBatchAsync_KeepsWaitingForUsableBatchWhenAReadFailsTransiently(
            TransientBatchReadFailureTestElement test)
        {
            // Setup.
            var timeProvider = new InstantTimeProvider();
            var swarmClientMock = BuildSwarmClientMock(SwarmClients.Bee);
            swarmClientMock.SetupSequence(c => c.GetPostageBatchAsync(BatchId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(test.ReadFailure)
                .ReturnsAsync(BuildPostageBatchResponse(isUsable: true));

            var gatewayService = new GatewayService(
                BuildEthernaGatewayClientMock().Object,
                swarmClientMock.Object,
                timeProvider);

            // Action.
            var (boughtBatchId, _) = await gatewayService.BuyPostageBatchAsync(BzzValue.FromInt32(1), PostageBatch.MinDepth);

            // Assert.
            Assert.Equal(BatchId, boughtBatchId);
            Assert.Equal(GatewayService.BatchCheckTimeSpan, Assert.Single(timeProvider.Delays));
            swarmClientMock.Verify(c => c.GetPostageBatchAsync(BatchId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task BuyPostageBatchAsync_StopsWaitingForBatchCreationWhenCancelled()
        {
            // Setup.
            using var cancellationTokenSource = new CancellationTokenSource();
            var ethernaGatewayClientMock = BuildEthernaGatewayClientMock();
            ethernaGatewayClientMock.SetupSequence(c => c.TryGetNewPostageBatchIdFromPostageRefAsync(
                    BatchReferenceId,
                    It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await cancellationTokenSource.CancelAsync();
                    return null;
                })
                .ReturnsAsync(BatchId);

            var gatewayService = new GatewayService(
                ethernaGatewayClientMock.Object,
                BuildSwarmClientMock(SwarmClients.Beehive).Object,
                new InstantTimeProvider());

            // Action.
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => gatewayService.BuyPostageBatchAsync(
                    BzzValue.FromInt32(1),
                    PostageBatch.MinDepth,
                    cancellationToken: cancellationTokenSource.Token));

            // Assert.
            ethernaGatewayClientMock.Verify(
                c => c.TryGetNewPostageBatchIdFromPostageRefAsync(BatchReferenceId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task BuyPostageBatchAsync_StopsWaitingForBatchCreationWhenGatewayGaveUpPurchase()
        {
            // Setup.
            var ethernaGatewayClientMock = BuildEthernaGatewayClientMock();
            ethernaGatewayClientMock.SetupSequence(c => c.TryGetNewPostageBatchIdFromPostageRefAsync(
                    BatchReferenceId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new EthernaGatewayApiException(
                    "Gone",
                    410,
                    "\"The node refused the purchase with status 402\"",
                    new Dictionary<string, IEnumerable<string>>(),
                    null))
                .ReturnsAsync(BatchId);

            var gatewayService = new GatewayService(
                ethernaGatewayClientMock.Object,
                BuildSwarmClientMock(SwarmClients.Beehive).Object,
                new InstantTimeProvider());

            // Action.
            var exception = await Assert.ThrowsAsync<EthernaGatewayApiException>(
                () => gatewayService.BuyPostageBatchAsync(BzzValue.FromInt32(1), PostageBatch.MinDepth));

            // Assert.
            Assert.Equal(410, exception.StatusCode);
            ethernaGatewayClientMock.Verify(
                c => c.TryGetNewPostageBatchIdFromPostageRefAsync(BatchReferenceId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task BuyPostageBatchAsync_StopsWaitingForUsableBatchWhenCancelled()
        {
            // Setup.
            using var cancellationTokenSource = new CancellationTokenSource();
            var swarmClientMock = BuildSwarmClientMock(SwarmClients.Bee);
            swarmClientMock.SetupSequence(c => c.GetPostageBatchAsync(BatchId, It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await cancellationTokenSource.CancelAsync();
                    return BuildPostageBatchResponse(isUsable: false);
                })
                .ReturnsAsync(BuildPostageBatchResponse(isUsable: true));

            var gatewayService = new GatewayService(
                BuildEthernaGatewayClientMock().Object,
                swarmClientMock.Object,
                new InstantTimeProvider());

            // Action.
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => gatewayService.BuyPostageBatchAsync(
                    BzzValue.FromInt32(1),
                    PostageBatch.MinDepth,
                    cancellationToken: cancellationTokenSource.Token));

            // Assert.
            swarmClientMock.Verify(c => c.GetPostageBatchAsync(BatchId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuyPostageBatchAsync_ThrowsTimeoutWhenBatchIsNotCreatedInTime()
        {
            // Setup.
            var timeProvider = new InstantTimeProvider();
            var ethernaGatewayClientMock = BuildEthernaGatewayClientMock();
            ethernaGatewayClientMock.Setup(c => c.TryGetNewPostageBatchIdFromPostageRefAsync(
                    BatchReferenceId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((PostageBatchId?)null);
            var swarmClientMock = BuildSwarmClientMock(SwarmClients.Beehive);

            var gatewayService = new GatewayService(
                ethernaGatewayClientMock.Object,
                swarmClientMock.Object,
                timeProvider);

            // Action.
            var exception = await Assert.ThrowsAsync<TimeoutException>(
                () => gatewayService.BuyPostageBatchAsync(BzzValue.FromInt32(1), PostageBatch.MinDepth));

            // Assert.
            Assert.Contains(BatchReferenceId, exception.Message, StringComparison.Ordinal);
            Assert.Equal(GatewayService.BatchCreationTimeout, timeProvider.GetElapsedTime(0));
            Assert.All(timeProvider.Delays, delay => Assert.Equal(GatewayService.BatchCheckTimeSpan, delay));
            swarmClientMock.Verify(
                c => c.GetPostageBatchAsync(It.IsAny<PostageBatchId>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task BuyPostageBatchAsync_ThrowsTimeoutWhenBatchIsNotUsableInTime()
        {
            // Setup.
            var timeProvider = new InstantTimeProvider();
            var swarmClientMock = BuildSwarmClientMock(SwarmClients.Bee);
            swarmClientMock.Setup(c => c.GetPostageBatchAsync(BatchId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildPostageBatchResponse(isUsable: false));

            var gatewayService = new GatewayService(
                BuildEthernaGatewayClientMock().Object,
                swarmClientMock.Object,
                timeProvider);

            // Action.
            await Assert.ThrowsAsync<TimeoutException>(
                () => gatewayService.BuyPostageBatchAsync(BzzValue.FromInt32(1), PostageBatch.MinDepth));

            // Assert.
            Assert.Equal(GatewayService.BatchUsableTimeout, timeProvider.GetElapsedTime(0));
            Assert.All(timeProvider.Delays, delay => Assert.Equal(GatewayService.BatchCheckTimeSpan, delay));
        }

        [Fact]
        public async Task DefundResourceDownloadAsync_DefundsNothingInDryMode()
        {
            // Setup.
            var ethernaGatewayClientMock = BuildEthernaGatewayClientMock();

            var gatewayService = new GatewayService(
                ethernaGatewayClientMock.Object,
                BuildSwarmClientMock(SwarmClients.Beehive, isDryMode: true).Object);

            // Action.
            var isDefunded = await gatewayService.DefundResourceDownloadAsync(SwarmReference.PlainZero);

            // Assert.
            Assert.False(isDefunded);
            ethernaGatewayClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task FundResourceDownloadAsync_FundsNothingInDryMode()
        {
            // Setup.
            var ethernaGatewayClientMock = BuildEthernaGatewayClientMock();

            var gatewayService = new GatewayService(
                ethernaGatewayClientMock.Object,
                BuildSwarmClientMock(SwarmClients.Beehive, isDryMode: true).Object);

            // Action.
            await gatewayService.FundResourceDownloadAsync(SwarmReference.PlainZero);

            // Assert.
            ethernaGatewayClientMock.VerifyNoOtherCalls();
        }

        // Helpers.
        private static Mock<IEthernaUserGatewayClient> BuildEthernaGatewayClientMock()
        {
            var ethernaGatewayClientMock = new Mock<IEthernaUserGatewayClient>();
            ethernaGatewayClientMock.Setup(c => c.BuyPostageBatchAsync(
                    It.IsAny<BzzValue>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(BatchReferenceId);
            return ethernaGatewayClientMock;
        }

        private static PostageBatchResponse BuildPostageBatchResponse(bool isUsable) =>
            new(
                new PostageBatch(
                    id: BatchId,
                    amount: BzzValue.FromInt32(1),
                    blockNumber: 0,
                    depth: PostageBatch.MinDepth,
                    exists: true,
                    isImmutable: false,
                    isUsable: isUsable,
                    label: null,
                    ttl: TimeSpan.FromDays(1),
                    utilization: 0),
                error: null);

        private static Mock<ISwarmClient> BuildSwarmClientMock(SwarmClients apiCompatibility, bool isDryMode = false)
        {
            var swarmClientMock = new Mock<ISwarmClient>();
            swarmClientMock.Setup(c => c.ApiCompatibility).Returns(apiCompatibility);
            swarmClientMock.Setup(c => c.IsDryMode).Returns(isDryMode);
            swarmClientMock.Setup(c => c.BuyPostageBatchAsync(
                    It.IsAny<BzzValue>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<ulong?>(),
                    It.IsAny<XDaiValue?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((BatchId, EthTxHash.Zero));
            swarmClientMock.Setup(c => c.GetPostageBatchAsync(BatchId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildPostageBatchResponse(isUsable: true));
            return swarmClientMock;
        }
    }
}
