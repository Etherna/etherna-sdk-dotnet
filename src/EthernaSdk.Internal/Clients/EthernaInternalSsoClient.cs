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

using Etherna.Sdk.Internal.Models;
using Etherna.Sdk.Sso.GenClients;
using Etherna.SwarmSdk.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Internal.Clients
{
    public class EthernaInternalSsoClient : IEthernaInternalSsoClient
    {
        // Fields.
        private readonly ServiceInteractClient generatedClient;

        // Constructor.
        public EthernaInternalSsoClient(Uri baseUrl, HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);

            generatedClient = new ServiceInteractClient(baseUrl.ToString(), httpClient);
        }

        // Methods.
        public async Task<UserContactInfo> GetUserContactsAsync(
            EthAddress userAddress,
            CancellationToken cancellationToken = default) =>
            new(await generatedClient.ContactsAsync(userAddress.ToString(), cancellationToken).ConfigureAwait(false));
    }
}
