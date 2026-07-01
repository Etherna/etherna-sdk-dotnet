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

using Etherna.SwarmSdk.Models;
using System;
using System.Collections.Generic;

namespace Etherna.Sdk.Users.Index.Models
{
    public class Comment(
        string id,
        DateTimeOffset creationDateTime,
        bool isEditable,
        bool isFrozen,
        EthAddress ownerAddress,
        IDictionary<string, string> textHistory,
        string videoId)
    {
        // Properties.
        public string Id { get; } = id;
        public DateTimeOffset CreationDateTime { get; } = creationDateTime;
        public bool IsEditable { get; } = isEditable;
        public bool IsFrozen { get; } = isFrozen;
        public EthAddress OwnerAddress { get; } = ownerAddress;
        public IDictionary<string, string> TextHistory { get; } = textHistory;
        public string VideoId { get; } = videoId;
    }
}