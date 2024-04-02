﻿//   Copyright 2020-present Etherna SA
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.Sdk.Common.GenClients.Sso;
using Etherna.Sdk.Common.Models;
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
            ArgumentNullException.ThrowIfNull(baseUrl, nameof(baseUrl));

            generatedClient = new ServiceInteractClient(baseUrl.ToString(), httpClient);
        }

        // Methods.
        public async Task<UserContactInfo> ContactsAsync(
            string userAddress,
            CancellationToken cancellationToken = default) =>
            new(await generatedClient.ContactsAsync(userAddress, cancellationToken).ConfigureAwait(false));
    }
}
