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

using Etherna.Sdk.Tools.Video.Models;
using Etherna.SwarmSdk.Models;

namespace Etherna.Sdk.Users.Index.Models
{
    public class VideoPreview(
        string id,
        long? createdAt,
        long? duration,
        SwarmReference? reference,
        string ownerAddress,
        VideoManifestImage thumbnail,
        string? title,
        long? updatedAt)
    {
        // Properties.
        public string Id { get; } = id;
        public long? CreatedAt { get; } = createdAt;
        public long? Duration { get; } = duration;
        public string OwnerAddress { get; } = ownerAddress;
        public SwarmReference? Reference { get; } = reference;
        public VideoManifestImage Thumbnail { get; } = thumbnail;
        public string? Title { get; } = title;
        public long? UpdatedAt { get; } = updatedAt;
    }
}