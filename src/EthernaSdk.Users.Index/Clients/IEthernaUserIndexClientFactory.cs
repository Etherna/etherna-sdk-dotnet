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

using System;

namespace Etherna.Sdk.Users.Index.Clients
{
    /// <summary>
    /// Create the user clients of the indexes, one for each index on its own base url. AddEthernaIndexClient
    /// registers it in place of a client of a default index, that doesn't exist
    /// </summary>
    public interface IEthernaUserIndexClientFactory
    {
        /// <summary>
        /// Create a user client of an index. It calls the index on behalf of the signed in user, over the http
        /// client shared by the user clients: it sends the access token of the user to that index
        /// </summary>
        /// <param name="indexBaseUrl">The absolute base url of the index</param>
        /// <returns>The user client of the index</returns>
        /// <exception cref="ArgumentException">The index base url is not absolute</exception>
        IEthernaUserIndexClient Create(Uri indexBaseUrl);
    }
}
