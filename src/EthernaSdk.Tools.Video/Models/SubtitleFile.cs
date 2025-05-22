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
using Etherna.Sdk.Tools.UniversalFiles;
using System;
using System.Threading.Tasks;

namespace Etherna.Sdk.Tools.Video.Models
{
    public class SubtitleFile : FileBase
    {
        // Constructor.
        private SubtitleFile(
            long byteSize,
            string fileName,
            string label,
            string languageCode,
            UFile universalFile,
            SwarmHash? swarmHash)
            : base(byteSize, fileName, universalFile, swarmHash)
        {
            Label = label;
            LanguageCode = languageCode;
        }

        // Static builders.
        public static async Task<SubtitleFile> BuildNewAsync(
            BasicUFile uFile,
            string label,
            string langaugeCode,
            SwarmHash? swarmHash = null)
        {
            ArgumentNullException.ThrowIfNull(uFile, nameof(uFile));

            // Get image info.
            var byteSize = await uFile.GetByteSizeAsync().ConfigureAwait(false);
            var fileName = await uFile.TryGetFileNameAsync().ConfigureAwait(false) ??
                           throw new InvalidOperationException($"Can't get file name from {uFile.FileUri.OriginalUri}");

            return new SubtitleFile(
                byteSize,
                fileName,
                label,
                langaugeCode,
                uFile,
                swarmHash);
        }
        
        // Properties.
        public string Label { get; }
        public string LanguageCode { get; }
    }
}