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

using Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest1;
using Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest2;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization
{
    /// <summary>
    /// Source-generated JSON metadata of the published manifests the library reads, of every schema version.
    /// Member names are matched whatever their case.
    /// </summary>
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(JsonElement))]
    [JsonSerializable(typeof(Manifest1Dto))]
    [JsonSerializable(typeof(Manifest2DetailsDto))]
    [JsonSerializable(typeof(Manifest2PreviewDto))]
    internal sealed partial class PublishedManifestJsonSerializerContext : JsonSerializerContext;
}
