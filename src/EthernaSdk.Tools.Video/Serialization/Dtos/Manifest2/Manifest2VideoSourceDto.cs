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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest2
{
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    internal sealed class Manifest2VideoSourceDto
    {
        // Constructors.
        public Manifest2VideoSourceDto(
            VideoType type,
            string? quality,
            SwarmUri path,
            long size)
        {
            Quality = quality;
            Path = path.ToString();
            Size = size;
            Type = type.ToString().ToLowerInvariant();
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Manifest2VideoSourceDto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string Type { get; set; }
        public string? Quality { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }

        // Methods.
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
        public ValidationError[] GetValidationErrors()
        {
            var errors = new List<ValidationError>();

            if (Path is null ||
                SwarmUri.FromString(Path) is { UriKind: UriKind.Relative, HasPath: false })
                errors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has empty path"));

            if (Quality is not null &&
                string.IsNullOrWhiteSpace(Quality))
                errors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has empty quality"));

            if (string.IsNullOrWhiteSpace(Type))
                errors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has empty type"));

            return errors.ToArray();
        }
    }
}
