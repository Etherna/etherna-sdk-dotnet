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

using Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest2;
using Etherna.Sdk.Tools.Video.Serialization.Dtos.PersonalData1;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization
{
    /// <summary>
    /// Source-generated JSON metadata of the documents the library writes: the preview and the details manifests,
    /// and the personal data. Members are named in camel case, and null ones are left out.
    /// </summary>
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(Manifest2DetailsDto))]
    [JsonSerializable(typeof(Manifest2PreviewDto))]
    [JsonSerializable(typeof(ManifestPersonalDataDto))]
    internal sealed partial class ManifestJsonSerializerContext : JsonSerializerContext;
}
