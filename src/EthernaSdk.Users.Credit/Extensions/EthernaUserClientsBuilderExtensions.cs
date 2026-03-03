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

using Etherna.Sdk.Users.Credit.Clients;
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
        public static IEthernaUserClientsBuilder AddEthernaCreditClient(
            this IEthernaUserClientsBuilder builder,
            string creditServiceBaseUrl = EthernaUserClientsBuilder.DefaultCreditUrl)
        {
            ArgumentNullException.ThrowIfNull(builder);
            
            // Register client.
            builder.Services.AddSingleton<IEthernaUserCreditClient>(serviceProvider =>
            {
                var clientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
                return new EthernaUserCreditClient(
                    new Uri(creditServiceBaseUrl, UriKind.Absolute),
                    clientFactory.CreateClient(builder.HttpClientName));
            });

            return builder;
        }
    }
}