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

using Etherna.Authentication;
using Etherna.Sdk.Gateway.GenClients;
using Etherna.SwarmSdk.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Users.Gateway.Clients
{
    public class EthernaUserGatewayClientTest
    {
        // Consts.
        private static readonly Uri GatewayUrl = new("https://gateway.test/");

        // Classes.
        public class PostageRefAnswerTestElement(
            HttpStatusCode statusCode,
            string? body,
            PostageBatchId? expectedBatchId)
        {
            public string? Body { get; } = body;
            public PostageBatchId? ExpectedBatchId { get; } = expectedBatchId;
            public HttpStatusCode StatusCode { get; } = statusCode;
        }

        private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string? body) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(statusCode);
                if (body is not null)
                    response.Content = new StringContent(body, Encoding.UTF8, "application/json");

                return Task.FromResult(response);
            }
        }

        // Data.
        public static IEnumerable<object[]> PostageRefAnswerTests
        {
            get
            {
                var batchId = PostageBatchId.FromString(new string('a', 64));
                var tests = new List<PostageRefAnswerTestElement>
                {
                    //batch bought
                    new(HttpStatusCode.OK, $"\"{batchId}\"", batchId),

                    //purchase still pending
                    new(HttpStatusCode.OK, "null", null),

                    //purchase still pending, answered without content
                    new(HttpStatusCode.NoContent, null, null),

                    //unknown reference
                    new(HttpStatusCode.NotFound, "\"Not found\"", null)
                };

                return tests.Select(t => new object[] { t });
            }
        }

        // Tests.
        [Theory, MemberData(nameof(PostageRefAnswerTests))]
        public async Task TryGetNewPostageBatchIdFromPostageRefAsync_ReturnsBatchIdWhenGatewayHasOne(
            PostageRefAnswerTestElement test)
        {
            // Setup.
            using var httpClient = new HttpClient(new StubHttpMessageHandler(test.StatusCode, test.Body));
            var client = new EthernaUserGatewayClient(Mock.Of<IEthernaOpenIdConnectClient>(), GatewayUrl, httpClient);

            // Action.
            var batchId = await client.TryGetNewPostageBatchIdFromPostageRefAsync("batch-reference");

            // Assert.
            Assert.Equal(test.ExpectedBatchId, batchId);
        }

        [Fact]
        public async Task TryGetNewPostageBatchIdFromPostageRefAsync_ThrowsWhenGatewayGaveUpPurchase()
        {
            // Setup.
            using var httpClient = new HttpClient(new StubHttpMessageHandler(
                HttpStatusCode.Gone,
                "\"The node refused the purchase with status 402\""));
            var client = new EthernaUserGatewayClient(Mock.Of<IEthernaOpenIdConnectClient>(), GatewayUrl, httpClient);

            // Action.
            var exception = await Assert.ThrowsAsync<EthernaGatewayApiException>(
                () => client.TryGetNewPostageBatchIdFromPostageRefAsync("batch-reference"));

            // Assert.
            Assert.Equal(410, exception.StatusCode);
        }
    }
}
