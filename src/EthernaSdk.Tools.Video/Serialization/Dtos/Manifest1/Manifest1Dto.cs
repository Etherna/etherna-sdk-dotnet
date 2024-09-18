﻿// Copyright 2020-present Etherna SA
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

using Etherna.BeeNet;
using Etherna.BeeNet.Models;
using Etherna.Sdk.Tools.Video.Exceptions;
using Etherna.Sdk.Tools.Video.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        private static readonly VideoManifestImage defaultThumbnail = new(
            1.8f,
            "UcGkx38v?CKhoej[j[jtM|bHs:jZjaj[j@ay",
            [new VideoManifestImageSource("thumb.jpg", ImageType.Jpeg, 100, SwarmHash.Zero, null)]);
        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true,
        };
        
        private string? _personalData;

        // Constructors.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [JsonConstructor]
        private Manifest1Dto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        //from v1.0
        public string Title { get; set; }
        public string Description { get; set; }
        public string OriginalQuality { get; set; }
        public string OwnerAddress { get; set; }
        public long Duration { get; set; }
        public Manifest1ThumbnailDto? Thumbnail { get; set; }
        public IEnumerable<Manifest1VideoSourceDto> Sources { get; set; }
        
        //from v1.1
        public long? CreatedAt { get; set; }
        public long? UpdatedAt { get; set; }
        public string? BatchId { get; set; }
        
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
        
        // Static methods.
        public static async Task<VideoManifest> DeserializeVideoManifestAsync(
            JsonElement manifestJsonElement,
            IBeeClient beeClient)
        {
            // Get manifest.
            var manifestDto = manifestJsonElement.Deserialize<Manifest1Dto>(jsonSerializerOptions)
                ?? throw new VideoManifestValidationException([new ValidationError(ValidationErrorType.JsonConvert, "Empty json")]);
            
            // Validate manifest.
            var validationErrors = manifestDto.GetValidationErrors();
            if (validationErrors.Length > 0)
                throw new VideoManifestValidationException(validationErrors);

            // Build manifest.
            //video sources
            List<VideoManifestVideoSource> videoSources = [];
            foreach (var videoSourceDto in manifestDto.Sources)
            {
                var videoSourceAddress = SwarmAddress.FromString(videoSourceDto.Reference);
                var videoSourceChunkRef = await beeClient.ResolveAddressToChunkReferenceAsync(videoSourceAddress).ConfigureAwait(false);
                var videoSourceHash = videoSourceChunkRef.Hash;
                
                videoSources.Add(new VideoManifestVideoSource(
                    videoSourceDto.Quality + ".mp4",
                    VideoType.Mp4,
                    videoSourceDto.Quality,
                    videoSourceDto.Size ?? 100,
                    [],
                    videoSourceHash,
                    videoSourceAddress));
            }
            
            //thumbnail
            var thumbnail = defaultThumbnail;
            if (manifestDto.Thumbnail is not null)
            {
                List<VideoManifestImageSource> imgSources = [];
                foreach (var imgSourceDto in manifestDto.Thumbnail.Sources)
                {
                    var imgSourceAddress = SwarmAddress.FromString(imgSourceDto.Value);
                    var imgSourceChunkRef = await beeClient.ResolveAddressToChunkReferenceAsync(imgSourceAddress).ConfigureAwait(false);
                    var imgSourceHash = imgSourceChunkRef.Hash;

                    imgSources.Add(new VideoManifestImageSource(
                        imgSourceDto.Key.TrimEnd('w') + ".jpg",
                        ImageType.Jpeg,
                        int.Parse(imgSourceDto.Key.TrimEnd('w'), CultureInfo.InvariantCulture),
                        imgSourceHash,
                        imgSourceAddress));
                }

                thumbnail = new VideoManifestImage(
                    manifestDto.Thumbnail.AspectRatio,
                    manifestDto.Thumbnail.Blurhash,
                    imgSources);
            }
            
            //manifest
            return new VideoManifest(
                manifestDto.Thumbnail?.AspectRatio ?? 1,
                manifestDto.BatchId is null ? (PostageBatchId?)null : PostageBatchId.FromString(manifestDto.BatchId),
                DateTimeOffset.FromUnixTimeMilliseconds(manifestDto.CreatedAt ?? 0),
                manifestDto.Description,
                TimeSpan.FromSeconds(manifestDto.Duration),
                manifestDto.Title,
                manifestDto.OwnerAddress,
                manifestDto.PersonalData,
                videoSources,
                thumbnail,
                [],
                manifestDto.UpdatedAt.HasValue ?
                    DateTimeOffset.FromUnixTimeMilliseconds(manifestDto.UpdatedAt.Value) :
                    null);
        }
    }
}
