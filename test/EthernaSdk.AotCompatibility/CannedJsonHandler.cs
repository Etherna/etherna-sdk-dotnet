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

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.AotCompatibility
{
    // Test message handler answering every request with the same canned JSON body, and recording each request, so
    // the probe can drive a client without a live service and check what the client sent.
    internal sealed class CannedJsonHandler(string json) : HttpMessageHandler
    {
        // Properties.
        public List<(HttpMethod Method, string Uri, string? Body)> Requests { get; } = [];

        // Protected methods.
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Requests.Add((request.Method, request.RequestUri!.ToString(), body));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                RequestMessage = request
            };
        }
    }
}
