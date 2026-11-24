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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest1
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
    internal sealed class Manifest1ThumbnailDto
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Manifest1ThumbnailDto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        
        // Properties.
        public float AspectRatio { get; set; }
        public string Blurhash { get; set; }
        public IDictionary<string, string> Sources { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }

        // Methods.
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
        public ValidationError[] GetValidationErrors()
        {
            var errors = new List<ValidationError>();

            if (Sources is null || Sources.Count == 0)
                errors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, "Thumbnail has missing sources"));

            foreach (var source in Sources ?? new Dictionary<string, string>())
            {
                if (string.IsNullOrWhiteSpace(source.Key))
                    errors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, $"Thumbnail has source with missing width"));
                if (!int.TryParse(source.Key.Replace("w", "", StringComparison.OrdinalIgnoreCase), out var width) ||
                    width <= 0)
                    errors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, $"Thumbnail has wrong width"));
            }

            return errors.ToArray();
        }
    }
}
