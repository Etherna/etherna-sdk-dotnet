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
using System.Linq;

namespace Etherna.Sdk.Tools.Video.Models
{
    public class VideoManifestImage(
        float aspectRatio,
        string blurhash,
        IEnumerable<VideoManifestImageSource> sources)
    {
        // Properties.
        public float AspectRatio { get; } = aspectRatio;
        public string Blurhash { get; } = blurhash;
        public IEnumerable<(SwarmUri Uri, VideoManifestImageSource Metadata)> Sources { get; } = sources.Select(
            s => (new SwarmUri($"thumb/{s.FileName}", UriKind.Relative), s));
        
        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not VideoManifestImage other) return false;
            return GetType() == other.GetType() &&
                   AspectRatio.Equals(other.AspectRatio) &&
                   string.Equals(Blurhash, other.Blurhash, StringComparison.Ordinal) &&
                   Sources.SequenceEqual(other.Sources);
        }

        public override int GetHashCode() =>
            AspectRatio.GetHashCode() ^
            string.GetHashCode(Blurhash, StringComparison.Ordinal) ^
            Sources.GetHashCode();
    }
}