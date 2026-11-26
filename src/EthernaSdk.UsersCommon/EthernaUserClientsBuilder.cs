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

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.Sdk.Users
{
    public class EthernaUserClientsBuilder(
        IServiceCollection services,
        string httpClientName,
        Uri ssoBaseUrl)
        : IEthernaUserClientsBuilder
    {
        // Consts.
        public const string DefaultCreditUrl = "https://credit.etherna.io/";
        public const string DefaultGatewayUrl = "https://gateway.etherna.io/";
        public const string DefaultIndexUrl = "https://index.etherna.io/";
        public const string DefaultSsoUrl = "https://sso.etherna.io/";

        // Properties.
        public string HttpClientName { get; } = httpClientName;
        public IServiceCollection Services { get; } = services;
        public Uri SsoBaseUrl { get; } = ssoBaseUrl;

        // Internal properties.
        /// <summary>
        /// True when the sign in requests the custom scopes passed to AddEthernaUserClients, as they are: the
        /// client registrations then add no default scope
        /// </summary>
        internal bool HasCustomScopes { get; init; }
    }
}