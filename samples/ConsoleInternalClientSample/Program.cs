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

using Etherna.Sdk.Internal.Clients;
using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleInternalClientSample
{
    class Program
    {
        // Consts.
        const string HelpText =
            "Etherna Credit client service sample help:\n\n" +
            "-c\tEtherna Credit base url\n" +
            "-a\tUser address\n" +
            "-l\tSSO base url\n" +
            "-i\tSSO Etherna Credit client id\n" +
            "-s\tSSO Etherna Credit client secret\n" +
            "\n" +
            "-h\tPrint help\n";

        static async Task Main(string[] args)
        {
            // Parse arguments.
            string? address = null;
            string? creditBaseUrl = null;
            string? ssoBaseUrl = null;
            string? ssoClientId = null;
            string? ssoClientSecret = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-c": creditBaseUrl = args[++i]; break;
                    case "-a": address = args[++i]; break;
                    case "-l": ssoBaseUrl = args[++i]; break;
                    case "-i": ssoClientId = args[++i]; break;
                    case "-s": ssoClientSecret = args[++i]; break;
                    case "-h": Console.Write(HelpText); return;
                    default: throw new ArgumentException(args[i] + " is not a valid argument");
                }
            }

            // Request Credit service info.
            Console.WriteLine("Etherna Credit base url:");
            if (creditBaseUrl is null) creditBaseUrl = Console.ReadLine();
            else Console.WriteLine(creditBaseUrl);

            Console.WriteLine("User address:");
            if (address is null) address = Console.ReadLine();
            else Console.WriteLine(address);

            Console.WriteLine();

            // Request SSO server info.
            Console.WriteLine("SSO base url:");
            if (ssoBaseUrl is null) ssoBaseUrl = Console.ReadLine();
            else Console.WriteLine(ssoBaseUrl);

            Console.WriteLine("SSO Etherna Credit client id:");
            if (ssoClientId is null) ssoClientId = Console.ReadLine();
            else Console.WriteLine(ssoClientId);

            Console.WriteLine("SSO Etherna Credit client secret:");
            if (ssoClientSecret is null) ssoClientSecret = Console.ReadLine();
            else Console.WriteLine(ssoClientSecret);

            Console.WriteLine();

            // Create a client.
            var httpClient = await CreateHttpClientAsync(new Uri(ssoBaseUrl!), ssoClientId!, ssoClientSecret!);
            var client = new EthernaInternalCreditClient(new Uri(creditBaseUrl!), httpClient);

            // Consume service api.
            var credit = await client.GetUserCreditAsync(address!);
            Console.WriteLine($"Current balance: {credit.Balance} xDai");
        }

        static async Task<HttpClient> CreateHttpClientAsync(
            Uri ssoBaseUrl,
            string ssoClientId,
            string ssoClientSecret)
        {
            // Discover endpoints from metadata.
            using var httpClient = new HttpClient();
            var discoveryDoc = await httpClient.GetDiscoveryDocumentAsync(ssoBaseUrl.AbsoluteUri).ConfigureAwait(false);
            if (discoveryDoc.IsError)
                throw discoveryDoc.Exception ?? new InvalidOperationException();

            // Request token.
            using var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = discoveryDoc.TokenEndpoint,

                ClientId = ssoClientId,
                ClientSecret = ssoClientSecret,
                Scope = "ethernaCredit_serviceInteract_api"
            };
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(tokenRequest).ConfigureAwait(false);

            if (tokenResponse.IsError)
                throw tokenResponse.Exception ?? new InvalidOperationException();

            // Create client and set api bearer token.
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken!);

            return apiClient;
        }
    }
}
