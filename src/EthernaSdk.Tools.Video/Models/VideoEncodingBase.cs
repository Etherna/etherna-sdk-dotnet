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
using System.Collections.Generic;
using System.Linq;

namespace Etherna.Sdk.Tools.Video.Models
{
    public abstract class VideoEncodingBase
    {
        // Constructor.
        protected VideoEncodingBase(
            TimeSpan duration,
            string? encodingDirectoryPath,
            FileBase? masterFile,
            VideoVariantBase[] variants)
        {
            ArgumentNullException.ThrowIfNull(variants);
            if (variants.Length == 0)
                throw new ArgumentException("Variant list can't be empty");

            Duration = duration;
            EncodingDirectoryPath = encodingDirectoryPath;
            MasterFile = masterFile;
            Variants = variants;
        }

        // Properties.
        public VideoVariantBase BestVariant => Variants.MaxBy(v => v.Height)!;
        public TimeSpan Duration { get; }
        public string? EncodingDirectoryPath { get; }
        public FileBase? MasterFile { get; }
        public long TotalByteSize => (MasterFile?.ByteSize ?? 0) + Variants.Sum(v => v.TotalByteSize);
        public IReadOnlyCollection<VideoVariantBase> Variants { get; }
    }
}