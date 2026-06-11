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
using System.Globalization;
using System.IO;
using System.Linq;

namespace Etherna.Sdk.Tools.Video.Models
{
    public class VideoManifestVideoSource(
        string sourceRelativePath,
        VideoType videoType,
        string? quality,
        long totalSourceSize,
        VideoManifestVideoSourceAdditionalFile[] additionalFiles,
        SwarmReference directContentReference)
    {
        // Properties.
        /// <summary>
        /// Content direct swarm hash. Used to link internal mantaray path to resource.
        /// </summary>
        public SwarmReference ContentSwarmReference { get; } = directContentReference;
        
        /// <summary>
        /// The file name, used to set the download file name in mantaray
        /// </summary>
        public string FileName => Path.GetFileName(SourceRelativePath);

        /// <summary>
        /// The video mime content type, used to set content type with the mantaray manifest
        /// </summary>
        public string MimeContentType => VideoType switch
        {
            VideoType.Dash => "application/dash+xml",
            VideoType.Hls => "application/x-mpegURL",
            VideoType.Mp4 => "video/mp4",
            _ => throw new NotSupportedException()
        };

        /// <summary>
        /// The video stream quality
        /// </summary>
        public string? Quality { get; } = quality;

        /// <summary>
        /// Relative path inside the source directory
        /// </summary>
        public string SourceRelativePath { get; } = sourceRelativePath;

        /// <summary>
        /// The video stream byte size
        /// </summary>
        public long TotalSourceSize { get; } = totalSourceSize;

        /// <summary>
        /// The video type, used to derive mime conten type
        /// </summary>
        public VideoType VideoType { get; } = videoType;

        public IEnumerable<(SwarmUri Uri, VideoManifestVideoSourceAdditionalFile File)> AdditionalFiles =>
            additionalFiles.Select(f => (new SwarmUri(
                GetManifestVideoSourceBaseDirectory(VideoType) + f.SourceRelativePath,
                UriKind.Relative), f));
        
        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not VideoManifestVideoSource other) return false;
            return GetType() == other.GetType() &&
                   ContentSwarmReference.Equals(other.ContentSwarmReference) &&
                   string.Equals(FileName, other.FileName, StringComparison.Ordinal) &&
                   string.Equals(MimeContentType, other.MimeContentType, StringComparison.Ordinal) &&
                   string.Equals(Quality, other.Quality, StringComparison.Ordinal) &&
                   string.Equals(SourceRelativePath, other.SourceRelativePath, StringComparison.Ordinal) &&
                   TotalSourceSize.Equals(other.TotalSourceSize) &&
                   VideoType.Equals(other.VideoType) &&
                   AdditionalFiles.SequenceEqual(other.AdditionalFiles);
        }

        public override int GetHashCode() =>
            ContentSwarmReference.GetHashCode() ^
            string.GetHashCode(FileName, StringComparison.Ordinal) ^
            string.GetHashCode(MimeContentType, StringComparison.Ordinal) ^
            string.GetHashCode(Quality, StringComparison.Ordinal) ^
            string.GetHashCode(SourceRelativePath, StringComparison.Ordinal) ^
            TotalSourceSize.GetHashCode() ^
            VideoType.GetHashCode() ^
            AdditionalFiles.GetHashCode();
        
        // Static methods.
        public static string GetManifestVideoSourceBaseDirectory(VideoType videoType) =>
            $"sources/{videoType.ToString().ToLowerInvariant()}/";
    }
}