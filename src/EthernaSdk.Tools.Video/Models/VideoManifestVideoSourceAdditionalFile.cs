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
using System.IO;

namespace Etherna.Sdk.Tools.Video.Models
{
    public class VideoManifestVideoSourceAdditionalFile(
        string sourceRelativePath,
        SwarmReference swarmReference)
    {
        // Properties.
        /// <summary>
        /// The file name, used to set the download file name in mantaray
        /// </summary>
        public string FileName => Path.GetFileName(SourceRelativePath);

        /// <summary>
        /// The video mime content type, used to set content type with the mantaray manifest
        /// </summary>
        public string MimeContentType => Path.GetExtension(FileName) switch
        {
            ".ts" => "video/MP2T",
            _ => throw new NotSupportedException()
        };

        public string SourceRelativePath { get; } = sourceRelativePath;

        /// <summary>
        /// Absolute swarm hash. Used to link internal mantaray path to resource.
        /// </summary>
        public SwarmReference SwarmReference { get; } = swarmReference;
        
        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not VideoManifestVideoSourceAdditionalFile other) return false;
            return GetType() == other.GetType() &&
                   string.Equals(FileName, other.FileName, StringComparison.Ordinal) &&
                   string.Equals(MimeContentType, other.MimeContentType, StringComparison.Ordinal) &&
                   string.Equals(SourceRelativePath, other.SourceRelativePath, StringComparison.Ordinal) &&
                   SwarmReference.Equals(other.SwarmReference);
        }

        public override int GetHashCode() =>
            string.GetHashCode(FileName, StringComparison.Ordinal) ^
            string.GetHashCode(MimeContentType, StringComparison.Ordinal) ^
            string.GetHashCode(SourceRelativePath, StringComparison.Ordinal) ^
            SwarmReference.GetHashCode();
    }
}