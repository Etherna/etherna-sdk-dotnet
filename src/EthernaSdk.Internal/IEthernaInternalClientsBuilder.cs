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

using Etherna.Sdk.Internal.Clients;
using System;

namespace Etherna.Sdk.Internal
{
    public interface IEthernaInternalClientsBuilder
    {
        /// <summary>
        /// Add the <see cref="IEthernaInternalCreditClient"/>, with its own http client and access token.
        /// </summary>
        /// <param name="creditServiceBaseUrl">The Etherna Credit service base url</param>
        /// <param name="clientId">The client id, registered on the Etherna SSO server</param>
        /// <param name="clientSecret">The client secret</param>
        /// <returns>A reference to this builder</returns>
        IEthernaInternalClientsBuilder AddEthernaCreditClient(
            Uri creditServiceBaseUrl,
            string clientId,
            string clientSecret);

        /// <summary>
        /// Add the <see cref="IEthernaInternalSsoClient"/>, with its own http client and access token.
        /// </summary>
        /// <param name="clientId">The client id, registered on the Etherna SSO server</param>
        /// <param name="clientSecret">The client secret</param>
        /// <returns>A reference to this builder</returns>
        IEthernaInternalClientsBuilder AddEthernaSsoClient(
            string clientId,
            string clientSecret);
    }
}
