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
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest1
{
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    [SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
    internal sealed class Manifest1Dto
    {
        // Consts.
        public const int DescriptionMaxLength = 5000;
        public const int PersonalDataMaxLength = 200;
        public const int TitleMaxLength = 200;

        // Fields.
        private string? _personalData;

        // Constructors.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Manifest1Dto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        //from v1.0
        public string Title { get; set; }
        public string Description { get; set; }
        public string OriginalQuality { get; set; }
        public EthAddress OwnerAddress { get; set; }
        public long Duration { get; set; }
        public Manifest1ThumbnailDto? Thumbnail { get; set; }
        public IEnumerable<Manifest1VideoSourceDto> Sources { get; set; }
        
        //from v1.1
        public long? CreatedAt { get; set; }
        public long? UpdatedAt { get; set; }
        
        //from v1.2
        public string? PersonalData
        {
            get => _personalData;
            set
            {
                if (value is not null && value.Length > PersonalDataMaxLength)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _personalData = value;
            }
        }
        public string V => "1.2";

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
        
        // Methods.
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
        public ValidationError[] GetValidationErrors()
        {
            var errors = new List<ValidationError>();
            
            if (Description is null)
                errors.Add(new ValidationError(ValidationErrorType.MissingDescription));
            else if (Description.Length > DescriptionMaxLength)
                errors.Add(new ValidationError(ValidationErrorType.InvalidDescription, "Description is too long"));
            
            if (Duration == 0)
                errors.Add(new ValidationError(ValidationErrorType.MissingDuration));
            
            if (Sources is null || !Sources.Any())
                errors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Missing sources"));
            foreach (var source in Sources ?? [])
                errors.AddRange(source.GetValidationErrors());

            if (Thumbnail is not null)
                errors.AddRange(Thumbnail.GetValidationErrors());
            
            if (string.IsNullOrWhiteSpace(Title))
                errors.Add(new ValidationError(ValidationErrorType.MissingTitle));
            else if (Title.Length > TitleMaxLength)
                errors.Add(new ValidationError(ValidationErrorType.InvalidTitle, "Title is too long"));

            if (PersonalData is not null &&
                PersonalData.Length > PersonalDataMaxLength)
                errors.Add(new ValidationError(ValidationErrorType.InvalidPersonalData, "Personal data is too long"));
            
            return errors.ToArray();
        }
    }
}
