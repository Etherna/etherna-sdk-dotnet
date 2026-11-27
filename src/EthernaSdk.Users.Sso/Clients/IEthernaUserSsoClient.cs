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
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Sso.Clients
{
    public interface IEthernaUserSsoClient : IEthernaUserClient
    {
        /// <summary>
        /// Get current user private information.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Current user private information</returns>
        /// <exception cref="EthernaSsoApiException">A server side error occurred.</exception>
        Task<PrivateUserInfo> GetPrivateUserInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get information about user by its ethereum address.
        /// </summary>
        /// <param name="userAddress">User's ethereum address</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>User information</returns>
        /// <exception cref="EthernaSsoApiException">A server side error occurred.</exception>
        Task<SsoUserInfo> GetUserInfoByAddressAsync(string userAddress, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get information about user by its username.
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>User information</returns>
        /// <exception cref="EthernaSsoApiException">A server side error occurred.</exception>
        Task<SsoUserInfo> GetUserInfoByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify if an email is registered.
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>True if email is registered, false otherwise</returns>
        /// <exception cref="EthernaSsoApiException">A server side error occurred.</exception>
        Task<bool> IsEmailRegisteredAsync(string email, CancellationToken cancellationToken = default);
    }
}