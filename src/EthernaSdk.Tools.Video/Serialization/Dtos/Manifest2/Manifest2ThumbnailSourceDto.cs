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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest2
{
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
    internal sealed class Manifest2ThumbnailSourceDto
    {
        // Constructors.
        public Manifest2ThumbnailSourceDto(
            int width,
            ImageType type,
            SwarmUri path)
        {
            Width = width;
            Type = type.ToString().ToLowerInvariant();
            Path = path.ToString();
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Manifest2ThumbnailSourceDto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public int Width { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
        
        // Methods.
        public ValidationError[] GetValidationErrors()
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(Type))
                errors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, $"Thumbnail has empty type"));
            
            if (Width <= 0)
                errors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, $"Thumbnail has wrong width"));
            
            return errors.ToArray();
        }
    }
}