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

using Etherna.BeeNet;
using Etherna.Sdk.Tools.Video.Services;
using Etherna.Sdk.Users.Index.Clients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

// ReSharper disable CheckNamespace
namespace Etherna.Sdk.Users
{
    public static class EthernaUserClientsBuilderExtensions
    {
        [SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
        public static IEthernaUserClientsBuilder AddEthernaIndexClient(
            this IEthernaUserClientsBuilder builder,
            string indexBaseUrl = EthernaUserClientsBuilder.DefaultIndexUrl)
        {
            ArgumentNullException.ThrowIfNull(builder);
            
            builder.Services.AddScoped<IVideoManifestService, VideoManifestService>();
            
            // Register client.
            builder.Services.AddSingleton<IEthernaUserIndexClient>(serviceProvider =>
            {
                var swarmClient = serviceProvider.GetRequiredService<ISwarmClient>();
                var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                return new EthernaUserIndexClient(
                    new Uri(indexBaseUrl, UriKind.Absolute),
                    swarmClient,
                    clientFactory.CreateClient(builder.HttpClientName));
            });

            return builder;
        }
    }
}