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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Credit.Clients
{
    public interface IEthernaUserCreditClient : IEthernaUserClient
    {
        /// <summary>
        /// Get credit status for current user
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaCreditApiException">A server side error occurred.</exception>
        Task<UserCredit> GetUserCreditAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transaction logs for current user
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Current page on list</returns>
        /// <exception cref="EthernaCreditApiException">A server side error occurred.</exception>
        Task<IEnumerable<UserOpLog>> GetUserOpLogsAsync(
            int? page = null,
            int? take = null,
            CancellationToken cancellationToken = default);
    }
}