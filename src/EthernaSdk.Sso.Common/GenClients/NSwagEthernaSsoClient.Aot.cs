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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Sso.GenClients
{
    // Native AOT support for the NSwag-generated clients of NSwagEthernaSsoClient.cs.
    //
    // NSwag doesn't emit a System.Text.Json source-generation context, so the generated clients would fall
    // back to reflection-based serialization, which is not trim/AOT safe. This context provides the JsonTypeInfo
    // metadata of every request body and response type the clients exchange, and the UpdateJsonSerializerSettings
    // partial hook of each client adds it to the client's JsonSerializerOptions. The generated (de)serialization
    // calls are switched by hand to the JsonTypeInfo-based overloads, which resolve through this context.
    //
    // Only the root types are listed: the source generator pulls in nested DTOs and enums transitively. Keep the
    // list in sync when the client is regenerated, and re-apply the edits of the generated file (doc/NativeAot.md).
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(PrivateUserDto))]
    [JsonSerializable(typeof(UserContactInfoDto))]
    [JsonSerializable(typeof(UserDto))]
    internal sealed partial class SsoJsonSerializerContext : JsonSerializerContext;

    internal partial class IdentityClient
    {
        // Static methods.
        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
            settings.TypeInfoResolverChain.Add(SsoJsonSerializerContext.Default);
    }

    internal partial class ServiceInteractClient
    {
        // Static methods.
        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
            settings.TypeInfoResolverChain.Add(SsoJsonSerializerContext.Default);
    }
}
