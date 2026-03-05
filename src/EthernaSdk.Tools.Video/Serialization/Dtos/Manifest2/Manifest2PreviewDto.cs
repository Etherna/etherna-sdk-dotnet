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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest2
{
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    internal sealed class Manifest2PreviewDto
    {
        // Consts.
        public const int TitleMaxLength = 200;

        // Constructors.
        public Manifest2PreviewDto(
            string title,
            long createdAt,
            long? updatedAt,
            string ownerEthAddress,
            long duration,
            Manifest2ThumbnailDto? thumbnail)
        {
            Title = title;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            OwnerAddress = ownerEthAddress;
            Duration = duration;
            Thumbnail = thumbnail;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Manifest2PreviewDto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string V => "2.1";
        public string Title { get; set; }
        public long CreatedAt { get; set; }
        public long? UpdatedAt { get; set; }
        public string OwnerAddress { get; set; }
        public long Duration { get; set; }
        public Manifest2ThumbnailDto? Thumbnail { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
        
        // Methods.
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
        public ValidationError[] GetValidationErrors()
        {
            var errors = new List<ValidationError>();
            
            if (CreatedAt <= 0)
                errors.Add(new ValidationError(ValidationErrorType.MissingManifestCreationTime));
            
            if (Duration == 0)
                errors.Add(new ValidationError(ValidationErrorType.MissingDuration));

            if (string.IsNullOrWhiteSpace(Title))
                errors.Add(new ValidationError(ValidationErrorType.MissingTitle));
            else if (Title.Length > TitleMaxLength)
                errors.Add(new ValidationError(ValidationErrorType.InvalidTitle, "Title is too long"));
            
            if (Thumbnail is not null)
                errors.AddRange(Thumbnail.GetValidationErrors());
            
            return errors.ToArray();
        }
    }
}