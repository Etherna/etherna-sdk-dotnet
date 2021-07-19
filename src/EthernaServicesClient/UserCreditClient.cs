﻿//   Copyright 2020-present Etherna Sagl
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

using System;
using System.Net.Http;

namespace Etherna.ServicesClient
{
    public class UserCreditClient : IUserCreditClient
    {
        // Fields.
        private readonly Uri baseUrl;
        private readonly Func<HttpClient> createHttpClient;

        // Constructor.
        public UserCreditClient(
            Uri baseUrl,
            Func<HttpClient> createHttpClient)
        {
            this.baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            this.createHttpClient = createHttpClient;
        }

        // Properties.
        public IUserClient UserClient =>
            new UserClient(baseUrl.AbsoluteUri, createHttpClient());
    }
}