// Copyright 2022-present Etherna SA
// This file is part of Etherna Video Importer.
// 
// Etherna Video Importer is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Video Importer is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Video Importer.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.BeeNet.Models;
using Etherna.Sdk.Tools.Video.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.Sdk.Tools.Video.Serialization.Dtos.Manifest2
{
    internal sealed class Manifest2CaptionSourceDto
    {
        // Constructors.
        public Manifest2CaptionSourceDto(
            string label,
            string lang,
            SwarmUri path)
        {
            Label = label;
            Lang = lang;
            Path = path.ToString();
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Manifest2CaptionSourceDto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string Label { get; set; }
        public string Lang { get; set; }
        public string Path { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
        
        // Methods.
        [SuppressMessage("Performance", "CA1822:Mark members as static")]
        public ValidationError[] GetValidationErrors()
        {
            var errors = new List<ValidationError>();
            
            return errors.ToArray();
        }
    }
}