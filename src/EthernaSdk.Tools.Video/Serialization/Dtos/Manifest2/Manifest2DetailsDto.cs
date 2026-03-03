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
using Etherna.Sdk.Tools.Video.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest2
{
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    internal sealed class Manifest2DetailsDto
    {
        // Consts.
        public const int DescriptionMaxLength = 5000;
        public const int PersonalDataMaxLength = 200;

        // Fields.
        private string? _personalData;

        // Constructors.
        public Manifest2DetailsDto(
            string description,
            float aspectRatio,
            string? personalData,
            IEnumerable<Manifest2CaptionSourceDto> captions,
            IEnumerable<Manifest2VideoSourceDto> sources)
        {
            Description = description;
            AspectRatio = aspectRatio;
            PersonalData = personalData;
            Captions = captions;
            Sources = sources;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Manifest2DetailsDto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        //from v2.0
        public string Description { get; set; }
        public float AspectRatio { get; set; }
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
        public IEnumerable<Manifest2VideoSourceDto> Sources { get; set; }
        
        //from v2.1
        public IEnumerable<Manifest2CaptionSourceDto>? Captions { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
        
        // Methods.
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
        public ValidationError[] GetValidationErrors()
        {
            var errors = new List<ValidationError>();

            if (AspectRatio <= 0)
                errors.Add(new ValidationError(ValidationErrorType.InvalidAspectRatio));

            foreach (var caption in Captions ?? [])
                errors.AddRange(caption.GetValidationErrors());
            
            if (Description is null)
                errors.Add(new ValidationError(ValidationErrorType.MissingDescription));
            else if (Description.Length > DescriptionMaxLength)
                errors.Add(new ValidationError(ValidationErrorType.InvalidDescription, "Description is too long"));
            
            if (Sources is null || !Sources.Any())
                errors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Missing sources"));
            foreach (var source in Sources ?? [])
                errors.AddRange(source.GetValidationErrors());
            if ((Sources ?? []).Count(s => s.Size == 0) > 1)
                errors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "More than one video source has 0 size"));
            
            if (PersonalData is not null &&
                PersonalData.Length > PersonalDataMaxLength)
                errors.Add(new ValidationError(ValidationErrorType.InvalidPersonalData, "Personal data is too long"));
            
            return errors.ToArray();
        }
    }
}