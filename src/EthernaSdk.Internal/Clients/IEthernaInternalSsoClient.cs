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
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Internal.Clients
{
    public interface IEthernaInternalSsoClient
    {
        /// <summary>
        /// Get contact information about a user.
        /// </summary>
        /// <param name="userAddress">User's ethereum address</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>User contact information</returns>
        /// <exception cref="EthernaSsoApiException">A server side error occurred.</exception>
        Task<UserContactInfo> GetUserContactsAsync(EthAddress userAddress, CancellationToken cancellationToken = default);
    }
}