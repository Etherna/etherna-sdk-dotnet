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

using Etherna.Sdk.Sso.GenClients;
using Etherna.Sdk.Users.Sso.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Sso.Clients
{
    public class EthernaUserSsoClient : IEthernaUserSsoClient
    {
        // Fields.
        private readonly IdentityClient generatedClient;

        // Constructor.
        public EthernaUserSsoClient(Uri baseUrl, HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);

            generatedClient = new IdentityClient(baseUrl.ToString(), httpClient);
        }

        // Methods.
        public async Task<PrivateUserInfo> GetPrivateUserInfoAsync(CancellationToken cancellationToken = default) =>
            new(await generatedClient.IdentityAsync(cancellationToken).ConfigureAwait(false));

        public async Task<SsoUserInfo> GetUserInfoByAddressAsync(string userAddress, CancellationToken cancellationToken = default) =>
            new(await generatedClient.AddressAsync(userAddress, cancellationToken).ConfigureAwait(false));

        public async Task<SsoUserInfo> GetUserInfoByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            new(await generatedClient.UsernameAsync(username, cancellationToken).ConfigureAwait(false));

        public Task<bool> IsEmailRegisteredAsync(string email, CancellationToken cancellationToken = default) =>
            generatedClient.EmailAsync(email, cancellationToken);
    }
}
