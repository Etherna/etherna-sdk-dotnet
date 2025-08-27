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
using System;

namespace Etherna.Sdk.Tools.Video.Models
{
    public class VideoManifestImageSource(
        string fileName,
        ImageType imageType,
        int width,
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
        public string FileName { get; } = fileName;

        /// <summary>
        /// The video type, used to derive mime content type
        /// </summary>
        public ImageType ImageType { get; } = imageType;

        /// <summary>
        /// The video mime content type, used to set content type with the mantaray manifest
        /// </summary>
        public string MimeContentType => ImageType switch
        {
            ImageType.Avif => "image/avif",
            ImageType.Jpeg => "image/jpeg",
            ImageType.Png => "image/png",
            ImageType.Webp => "image/webp",
            _ => throw new NotSupportedException()
        };

        public int Width { get; } = width;

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not VideoManifestImageSource other) return false;
            return GetType() == other.GetType() &&
                   ContentSwarmReference.Equals(other.ContentSwarmReference) &&
                   string.Equals(FileName, other.FileName, StringComparison.Ordinal) &&
                   ImageType.Equals(other.ImageType) &&
                   string.Equals(MimeContentType, other.MimeContentType, StringComparison.Ordinal) &&
                   Width.Equals(other.Width);
        }
        
        public override int GetHashCode() =>
            ContentSwarmReference.GetHashCode() ^
            string.GetHashCode(FileName, StringComparison.Ordinal) ^
            ImageType.GetHashCode() ^
            string.GetHashCode(MimeContentType, StringComparison.Ordinal) ^
            Width.GetHashCode();
    }
}