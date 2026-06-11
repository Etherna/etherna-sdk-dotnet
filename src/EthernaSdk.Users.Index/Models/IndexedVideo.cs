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
using System;

namespace Etherna.Sdk.Users.Index.Models
{
    public class IndexedVideo(
        string id,
        PostageBatchId? batchId,
        DateTimeOffset creationDateTime,
        VoteValue? currentVoteValue,
        string? description,
        SwarmReference? lastValidManifestReference,
        string ownerAddress,
        VideoManifestPersonalData? personalData,
        string? title,
        long totDownvotes,
        long totUpvotes)
    {
        // Properties.
        public string Id { get; } = id;
        public PostageBatchId? BatchId { get; } = batchId;
        public DateTimeOffset CreationDateTime { get; } = creationDateTime;
        public VoteValue? CurrentVoteValue { get; } = currentVoteValue;
        public string? Description { get; } = description;
        public SwarmReference? LastValidManifestReference { get; } = lastValidManifestReference;
        public string OwnerAddress { get; } = ownerAddress;
        public VideoManifestPersonalData? PersonalData { get; } = personalData;
        public string? Title { get; } = title;
        public long TotDownvotes { get; } = totDownvotes;
        public long TotUpvotes { get; } = totUpvotes;
    }
}