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

using Etherna.Authentication.Native;
using Etherna.Sdk.Users;
using Etherna.Sdk.Users.Credit.Clients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ConsoleUserClientSample
{
    public static class Program
    {
        // Consts.
        private const string HelpText = $"""
            Etherna Credit client service sample help:

            -c    Etherna Credit base url
            -l    SSO base url
            -i    SSO Etherna Credit client id
            -s    SSO Etherna Credit client secret (optional)
            -k    Api key (optional)
            
            -ss   Skip SSO Etherna Credit client secret
            -sk   Skip api key
            
            -h    Print help";
            """;

        static async Task Main(string[] args)
        {
            // Parse arguments.
            string? apiKey = null;
            string? creditBaseUrl = null;
            bool skipApiKey = false;
            bool skipSsoClientSecret = false;
            string? ssoBaseUrl = null;
            string? ssoClientId = null;
            string? ssoClientSecret = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-c": creditBaseUrl = args[++i]; break;
                    case "-l": ssoBaseUrl = args[++i]; break;
                    case "-i": ssoClientId = args[++i]; break;
                    case "-s": ssoClientSecret = args[++i]; break;
                    case "-k": apiKey = args[++i]; break;
                    case "-ss": skipSsoClientSecret = true; break;
                    case "-sk": skipApiKey = true; break;
                    case "-h": Console.Write(HelpText); return;
                    default: throw new ArgumentException(args[i] + " is not a valid argument");
                }
            }

            // Request Credit service info.
            Console.WriteLine("Etherna Credit base url:");
            if (creditBaseUrl is null) creditBaseUrl = Console.ReadLine();
            else Console.WriteLine(creditBaseUrl);

            Console.WriteLine();

            // Request SSO server info.
            Console.WriteLine("SSO base url:");
            if (ssoBaseUrl is null) ssoBaseUrl = Console.ReadLine();
            else Console.WriteLine(ssoBaseUrl);

            Console.WriteLine("SSO Etherna Credit client id:");
            if (ssoClientId is null) ssoClientId = Console.ReadLine();
            else Console.WriteLine(ssoClientId);

            if (!skipSsoClientSecret)
            {
                Console.WriteLine("SSO Etherna Credit client secret:");
                if (ssoClientSecret is null) ssoClientSecret = Console.ReadLine();
                else Console.WriteLine(ssoClientSecret);
            }

            if (!skipApiKey)
            {
                Console.WriteLine("Api key (optional):");
                if (apiKey is null) apiKey = Console.ReadLine();
                else Console.WriteLine(apiKey);
            }

            Console.WriteLine();
            
            // Register etherna credit service client.
            var services = new ServiceCollection();
            var ethernaClientsBuilder = services.AddEthernaUserClients(
                ssoClientId!,
                ssoClientSecret,
                3000,
                ["userApi.credit"],
                authority: ssoBaseUrl!);
            ethernaClientsBuilder.AddEthernaCreditClient(creditBaseUrl!);
            
            // Get client.
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IEthernaUserCreditClient>();
            
            // Signin user, choosing the flow from the parsed input.
            var ethernaSignInService = serviceProvider.GetRequiredService<IEthernaSignInService>();
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey)) //"code" grant flow
                    await ethernaSignInService.SignInAsync();
                else //"password" grant flow
                    await ethernaSignInService.SignInAsync(apiKey);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Error during authentication");
                Console.WriteLine(e.Message);
                throw;
            }
            catch (Win32Exception e)
            {
                Console.WriteLine($"Error opening browser on local system. Try to authenticate with API key.");
                Console.WriteLine(e.Message);
                throw;
            }

            // Consume service api.
            var credit = await client.GetUserCreditAsync();
            Console.WriteLine($"Current user balance: {credit.Balance} xDai");
        }
    }
}