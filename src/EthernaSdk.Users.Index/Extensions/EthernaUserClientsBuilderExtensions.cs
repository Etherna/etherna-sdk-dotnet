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

using Etherna.Authentication;
using Etherna.Sdk.Tools.Video.Services;
using Etherna.Sdk.Users.Index.Clients;
using Etherna.SwarmSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

// ReSharper disable CheckNamespace
namespace Etherna.Sdk.Users
{
    public static class EthernaUserClientsBuilderExtensions
    {
        public static IEthernaUserClientsBuilder AddEthernaIndexClient(
            this IEthernaUserClientsBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            
            builder.Services.AddScoped<IVideoManifestService, VideoManifestService>();
            
            // Register the factory of the index clients: there is no default index, each client is created on
            // the base url of its own index.
            builder.Services.TryAddSingleton<IEthernaUserIndexClientFactory>(serviceProvider =>
                new EthernaUserIndexClientFactory(
                    serviceProvider.GetRequiredService<IEthernaOpenIdConnectClient>(),
                    builder.HttpClientName,
                    serviceProvider.GetRequiredService<IHttpClientFactory>(),
                    serviceProvider.GetRequiredService<ISwarmClient>(),
                    serviceProvider.GetService<ILogger<EthernaUserIndexClient>>()));

            // Request the default api scope of the service at sign in, unless the sign in has custom scopes.
            builder.TryAddEthernaUserDefaultApiScope(EthernaScopes.UserApiIndex);

            return builder;
        }
    }
}