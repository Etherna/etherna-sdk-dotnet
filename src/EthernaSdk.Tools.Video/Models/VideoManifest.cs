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
using Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest2;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Models
{
    /// <summary>
    /// Video Manifest utility class, able to serialize/deserialize to/from json
    /// </summary>
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
    public class VideoManifest(
        float aspectRatio,
        PostageBatchId? batchId,
        DateTimeOffset createdAt,
        string description,
        TimeSpan duration,
        string title,
        string ownerEthAddress,
        string? personalData,
        IEnumerable<VideoManifestVideoSource> videoSources,
        VideoManifestImage thumbnail,
        IEnumerable<VideoManifestCaptionSource> captionSources,
        DateTimeOffset? updatedAt = null)
    {
        // Fields.
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Properties.
        public float AspectRatio { get; } = aspectRatio;
        public PostageBatchId BatchId { get; set; } = batchId ?? PostageBatchId.Zero; //can be updated later
        public IEnumerable<(SwarmUri Uri, VideoManifestCaptionSource Source)> CaptionSources { get; }
            = captionSources.Select(s => (new SwarmUri($"captions/{s.FileName}", UriKind.Relative), s));
        public DateTimeOffset CreatedAt { get; } = createdAt;
        public string Description { get; } = description;
        public TimeSpan Duration { get; } = duration;
        public string Title { get; } = title;
        public string OwnerEthAddress { get; } = ownerEthAddress;
        public VideoManifestPersonalData? PersonalData { get; } = TryParsePersonalData(personalData);
        public string? PersonalDataRaw { get; } = personalData;
        public VideoManifestImage Thumbnail { get; } = thumbnail;
        public DateTimeOffset? UpdatedAt { get; set; } = updatedAt;
        public IEnumerable<(SwarmUri Uri, VideoManifestVideoSource Metadata)> VideoSources { get; }
            = videoSources.Select(s => (new SwarmUri(
                VideoManifestVideoSource.GetManifestVideoSourceBaseDirectory(s.VideoType) + s.SourceRelativePath,
                UriKind.Relative), s));
        
        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not VideoManifest other) return false;
            return GetType() == other.GetType() &&
                   AspectRatio.Equals(other.AspectRatio) &&
                   BatchId.Equals(other.BatchId) &&
                   CaptionSources.SequenceEqual(other.CaptionSources) &&
                   DateTimeOffset.Equals(CreatedAt, other.CreatedAt) &&
                   string.Equals(Description, other.Description, StringComparison.Ordinal) &&
                   Duration.Equals(other.Duration) &&
                   string.Equals(Title, other.Title, StringComparison.Ordinal) &&
                   string.Equals(OwnerEthAddress, other.OwnerEthAddress, StringComparison.Ordinal) &&
                   EqualityComparer<VideoManifestPersonalData>.Default.Equals(PersonalData, other.PersonalData) &&
                   string.Equals(PersonalDataRaw, other.PersonalDataRaw, StringComparison.Ordinal) &&
                   Thumbnail.Equals(other.Thumbnail) &&
                   EqualityComparer<DateTimeOffset?>.Default.Equals(UpdatedAt, other.UpdatedAt) &&
                   VideoSources.SequenceEqual(other.VideoSources);
        }

        public override int GetHashCode() =>
            AspectRatio.GetHashCode() ^
            CaptionSources.GetHashCode() ^
            CreatedAt.GetHashCode() ^
            string.GetHashCode(Description, StringComparison.Ordinal) ^
            Duration.GetHashCode() ^
            string.GetHashCode(Title, StringComparison.Ordinal) ^
            string.GetHashCode(OwnerEthAddress, StringComparison.Ordinal) ^
            PersonalData?.GetHashCode() ?? 0 ^
            string.GetHashCode(PersonalDataRaw, StringComparison.Ordinal) ^
            Thumbnail.GetHashCode() ^
            VideoSources.GetHashCode();
        
        public string SerializeDetailsManifest()
        {
            var manifestDetails = new Manifest2DetailsDto(
                description: Description,
                aspectRatio: AspectRatio,
                batchId: BatchId,
                personalData: PersonalDataRaw,
                captions: CaptionSources.Select(s => new Manifest2CaptionSourceDto(
                    s.Source.Label,
                    s.Source.LanguageCode,
                    path: s.Uri)),
                sources: VideoSources.Select(s => new Manifest2VideoSourceDto(
                    type: s.Metadata.VideoType,
                    quality: s.Metadata.Quality,
                    path: s.Uri,
                    size: s.Metadata.TotalSourceSize)));
            return JsonSerializer.Serialize(manifestDetails, jsonSerializerOptions);
        }

        public string SerializePreviewManifest()
        {
            var manifestPreview = new Manifest2PreviewDto(
                title: Title,
                createdAt: CreatedAt.ToUnixTimeSeconds(),
                updatedAt: UpdatedAt?.ToUnixTimeSeconds(),
                ownerEthAddress: OwnerEthAddress,
                duration: (long)Duration.TotalSeconds,
                thumbnail: new Manifest2ThumbnailDto(
                    aspectRatio: Thumbnail.AspectRatio,
                    blurhash: Thumbnail.Blurhash,
                    sources: Thumbnail.Sources.Select(s => new Manifest2ThumbnailSourceDto(
                        width: s.Metadata.Width,
                        type: s.Metadata.ImageType,
                        path: s.Uri))));
            return JsonSerializer.Serialize(manifestPreview, jsonSerializerOptions);
        }
        
        // Helpers.
        private static VideoManifestPersonalData? TryParsePersonalData(string? personalDataRaw)
        {
            if (personalDataRaw is null) return null;
            return VideoManifestPersonalData.TryDeserialize(personalDataRaw, out var personalData)
                ? personalData : null;
        }
    }
}