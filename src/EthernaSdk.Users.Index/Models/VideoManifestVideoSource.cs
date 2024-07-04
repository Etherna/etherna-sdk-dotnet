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

using Etherna.BeeNet.Models;

namespace Etherna.Sdk.Users.Index.Models
{
    public class VideoManifestVideoSource(
        string path,
        string type,
        string? quality,
        long size)
    {
        // Properties.
        public string Path { get; } = path;
        public string Type { get; } = type;
        public string? Quality { get; } = quality;
        public long Size { get; } = size;
    }
}