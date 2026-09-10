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

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Models
{
    public class VideoManifestPersonalData(
        string clientName,
        string clientVersion,
        string sourceProviderName,
        string sourceVideoId)
    {
        // Fields.
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Properties.
        public string ClientName { get; } = clientName;
        public string ClientVersion { get; } = clientVersion;
        public string SourceProviderName { get; } = sourceProviderName;
        public string SourceVideoId { get; } = sourceVideoId;

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not VideoManifestPersonalData other) return false;
            return GetType() == other.GetType() &&
                   string.Equals(ClientName, other.ClientName, StringComparison.Ordinal) &&
                   string.Equals(ClientVersion, other.ClientVersion, StringComparison.Ordinal) &&
                   string.Equals(SourceProviderName, other.SourceProviderName, StringComparison.Ordinal) &&
                   string.Equals(SourceVideoId, other.SourceVideoId, StringComparison.Ordinal);
        }

        public override int GetHashCode() =>
            string.GetHashCode(ClientName, StringComparison.Ordinal) ^
            string.GetHashCode(ClientVersion, StringComparison.Ordinal) ^
            string.GetHashCode(SourceProviderName, StringComparison.Ordinal) ^
            string.GetHashCode(SourceVideoId, StringComparison.Ordinal);
        
        public string Serialize()
        {
            var dto = new Serialization.Dtos.PersonalData1.ManifestPersonalDataDto(
                ClientName,
                ClientVersion,
                SourceProviderName,
                SourceVideoId);
            return JsonSerializer.Serialize(dto, JsonSerializerOptions);
        }
        
        // Static methods.
        public static bool TryDeserialize(string? rawPersonalData, out VideoManifestPersonalData personalData)
        {
            personalData = null!;

            if (rawPersonalData is null)
                return false;
            
            try
            {
                var dto = JsonSerializer.Deserialize<Serialization.Dtos.PersonalData1.ManifestPersonalDataDto>(rawPersonalData, JsonSerializerOptions);
                if (dto is null)
                    return false;
                
                personalData = new VideoManifestPersonalData(
                    dto.CliName,
                    dto.CliV,
                    dto.SrcName,
                    dto.SrcVId);
                return true;
            }
            catch (Exception e) when (e is InvalidOperationException
                                        or JsonException)
            {
                return false;
            }
        }
    }
}