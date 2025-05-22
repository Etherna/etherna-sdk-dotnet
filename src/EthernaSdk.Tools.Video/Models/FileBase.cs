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
using System.IO;
using System.Threading.Tasks;

namespace Etherna.Sdk.Tools.Video.Models
{
    public class FileBase(
        long byteSize,
        string fileName,
        UFile universalFile,
        SwarmHash? swarmHash)
    {
        // Static builders.
        public static async Task<FileBase> BuildFromUFileAsync(UFile uFile)
        {
            ArgumentNullException.ThrowIfNull(uFile, nameof(uFile));
            
            var fileSize = await uFile.GetByteSizeAsync().ConfigureAwait(false);
            var fileName = await uFile.TryGetFileNameAsync().ConfigureAwait(false) ?? throw new InvalidOperationException(
                $"Can't get file name from {uFile.FileUri.OriginalUri}");
            
            return new FileBase(fileSize, fileName,uFile, null);
        }
        
        // Properties.
        public long ByteSize { get; } = byteSize;
        public string FileName { get; } = fileName;
        public SwarmHash? SwarmHash { get; set; } = swarmHash;
        public UUri UUri => universalFile.FileUri;
        
        // Methods.
        public async Task<Stream> ReadToStreamAsync() => (await universalFile.ReadToStreamAsync().ConfigureAwait(false)).Stream;

        public Task<string> ReadToStringAsync() => universalFile.ReadToStringAsync();
    }
}
