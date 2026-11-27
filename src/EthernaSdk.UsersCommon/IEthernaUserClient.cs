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

namespace Etherna.Sdk.Users
{
    /// <summary>
    /// A client calling the user API of an Etherna service on behalf of an authenticated user
    /// </summary>
    public interface IEthernaUserClient
    {
        /// <summary>
        /// The user the client calls the service as: the claims of the user whose access token it sends. A
        /// registered client sends the token of the user signed in with the sign in service of its container, and
        /// exposes that user also when the sign in happens after the client is built.
        /// </summary>
        IEthernaOpenIdConnectClient AuthenticatedUser { get; }
    }
}
