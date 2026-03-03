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

using Etherna.Sdk.Credit.GenClients;
using Etherna.Sdk.Users.Credit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Credit.Clients
{
    public class EthernaUserCreditClient : IEthernaUserCreditClient
    {
        // Fields.
        private readonly UserClient generatedClient;

        // Constructor.
        public EthernaUserCreditClient(Uri baseUrl, HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);

            generatedClient = new UserClient(baseUrl.ToString(), httpClient);
        }

        // Methods.
        public async Task<UserCredit> GetUserCreditAsync(CancellationToken cancellationToken = default) =>
            new(await generatedClient.CreditAsync(cancellationToken).ConfigureAwait(false));

        public async Task<IEnumerable<UserOpLog>> GetUserOpLogsAsync(
            int? page = null,
            int? take = null,
            CancellationToken cancellationToken = default) =>
            (await generatedClient.LogsAsync(page, take, cancellationToken).ConfigureAwait(false)).Select(op => new UserOpLog(op));
    }
}
