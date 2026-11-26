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

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Etherna.Sdk.Index.GenClients
{
    // Native AOT support for the NSwag-generated clients of NSwagEthernaIndexClient.cs.
    //
    // NSwag doesn't emit a System.Text.Json source-generation context, so the generated clients would fall
    // back to reflection-based serialization, which is not trim/AOT safe. This context provides the JsonTypeInfo
    // metadata of every request body and response type the clients exchange, and the UpdateJsonSerializerSettings
    // partial hook of each client adds it to the client's JsonSerializerOptions. The generated (de)serialization
    // calls are switched by hand to the JsonTypeInfo-based overloads, which resolve through this context.
    //
    // Only the root types are listed: the source generator pulls in nested DTOs and enums transitively. Keep the
    // list in sync when the client is regenerated, and re-apply the edits of the generated file (doc/NativeAot.md).
    [JsonSerializable(typeof(Comment2Dto))]
    [JsonSerializable(typeof(CommentDto))]
    [JsonSerializable(typeof(CurrentUserDto))]
    [JsonSerializable(typeof(ICollection<CommentDto>))]
    [JsonSerializable(typeof(ICollection<UserDto>))]
    [JsonSerializable(typeof(ICollection<VideoDto>))]
    [JsonSerializable(typeof(ICollection<VideoManifestStatusDto>))]
    [JsonSerializable(typeof(ICollection<VideoStatusDto>))]
    [JsonSerializable(typeof(IEnumerable<object>))]
    [JsonSerializable(typeof(IEnumerable<string>))]
    [JsonSerializable(typeof(PaginatedEnumerableDtoOfComment2Dto))]
    [JsonSerializable(typeof(PaginatedEnumerableDtoOfCommentDto))]
    [JsonSerializable(typeof(PaginatedEnumerableDtoOfUserDto))]
    [JsonSerializable(typeof(PaginatedEnumerableDtoOfVideo2Dto))]
    [JsonSerializable(typeof(PaginatedEnumerableDtoOfVideoDto))]
    [JsonSerializable(typeof(PaginatedEnumerableDtoOfVideoPreviewDto))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(SystemParametersDto))]
    [JsonSerializable(typeof(UserDto))]
    [JsonSerializable(typeof(Video2Dto))]
    [JsonSerializable(typeof(VideoCreateInput))]
    [JsonSerializable(typeof(VideoCreateInput2))]
    [JsonSerializable(typeof(VideoDto))]
    [JsonSerializable(typeof(VideoManifest2Dto))]
    [JsonSerializable(typeof(VideoManifestDto))]
    [JsonSerializable(typeof(VideoManifestStatusDto))]
    [JsonSerializable(typeof(VideoStatusDto))]
    internal sealed partial class IndexJsonSerializerContext : JsonSerializerContext;

    internal partial class CommentsClient
    {
        // Static methods.
        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
            settings.TypeInfoResolverChain.Add(IndexJsonSerializerContext.Default);
    }

    internal partial class ModerationClient
    {
        // Static methods.
        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
            settings.TypeInfoResolverChain.Add(IndexJsonSerializerContext.Default);
    }

    internal partial class SearchClient
    {
        // Static methods.
        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
            settings.TypeInfoResolverChain.Add(IndexJsonSerializerContext.Default);
    }

    internal partial class SystemClient
    {
        // Static methods.
        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
            settings.TypeInfoResolverChain.Add(IndexJsonSerializerContext.Default);
    }

    internal partial class UsersClient
    {
        // Static methods.
        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
            settings.TypeInfoResolverChain.Add(IndexJsonSerializerContext.Default);
    }

    internal partial class VideosClient
    {
        // Static methods.
        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
            settings.TypeInfoResolverChain.Add(IndexJsonSerializerContext.Default);

        // Helpers.
        // Serialize a request body through the source-generated context. The JsonTypeInfo comes from the static
        // type of the body, so a null body serializes as "null" like the generated call did.
        private byte[] SerializeBody<T>(T body) =>
            JsonSerializer.SerializeToUtf8Bytes(body, (JsonTypeInfo<T>)JsonSerializerSettings.GetTypeInfo(typeof(T)));
    }
}
