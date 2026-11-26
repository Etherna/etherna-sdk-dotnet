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

namespace Etherna.Sdk.Tools.UniversalFiles.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// An online uri can carry credentials, as user info or in its query: neither must reach the logs,
        /// nor the messages of the exceptions
        /// </summary>
        public static string RemoveUriSecrets(this string fileUri) =>
            Uri.TryCreate(fileUri, UriKind.Absolute, out var uri) && !uri.IsFile ?
                uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped) :
                fileUri;
    }
}
