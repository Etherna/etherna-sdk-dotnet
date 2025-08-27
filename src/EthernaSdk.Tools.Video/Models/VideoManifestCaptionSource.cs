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
using System.Diagnostics.CodeAnalysis;

namespace Etherna.Sdk.Tools.Video.Models
{
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public class VideoManifestCaptionSource(
        string label,
        string languageCode,
        string fileName,
        SwarmReference directContentReference)
    {
        // Properties.
        public SwarmReference ContentSwarmReference { get; } = directContentReference;
        public string FileName { get; } = fileName;
        public string Label { get; } = label;
        public string LanguageCode { get; } = languageCode;
        public string MimeContentType => "text/vtt";

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not VideoManifestCaptionSource other) return false;
            return GetType() == other.GetType() &&
                   ContentSwarmReference.Equals(other.ContentSwarmReference) &&
                   string.Equals(FileName, other.FileName, StringComparison.Ordinal) &&
                   string.Equals(Label, other.Label, StringComparison.Ordinal) &&
                   string.Equals(LanguageCode, other.LanguageCode, StringComparison.Ordinal) &&
                   string.Equals(MimeContentType, other.MimeContentType, StringComparison.Ordinal);
        }
        
        public override int GetHashCode() =>
            ContentSwarmReference.GetHashCode() ^
            string.GetHashCode(FileName, StringComparison.Ordinal) ^
            string.GetHashCode(Label, StringComparison.Ordinal) ^
            string.GetHashCode(LanguageCode, StringComparison.Ordinal) ^
            string.GetHashCode(MimeContentType, StringComparison.Ordinal);
    }
}