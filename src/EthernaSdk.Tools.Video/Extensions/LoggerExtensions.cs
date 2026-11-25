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

using Etherna.SwarmSdk.Models;
using Microsoft.Extensions.Logging;
using System;

namespace Etherna.Sdk.Tools.Video.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 1
     */
    internal static class LoggerExtensions
    {
        // Fields.
        //*** DEBUG LOGS ***
        private static readonly Action<ILogger, SwarmReference, Version?, int, Exception> _publishedVideoManifestRead =
            LoggerMessage.Define<SwarmReference, Version?, int>(
                LogLevel.Debug,
                new EventId(0, nameof(PublishedVideoManifestRead)),
                "Published video manifest {ManifestReference} read with version {ManifestVersion} and {ValidationErrorsCount} validation errors");

        private static readonly Action<ILogger, SwarmReference, string, Exception> _videoManifestChunksCreated =
            LoggerMessage.Define<SwarmReference, string>(
                LogLevel.Debug,
                new EventId(1, nameof(VideoManifestChunksCreated)),
                "Chunks of video manifest {ManifestReference} created in directory {ChunksDirectory}");

        // Methods.
        public static void PublishedVideoManifestRead(this ILogger logger, SwarmReference manifestReference, Version? manifestVersion, int validationErrorsCount) =>
            _publishedVideoManifestRead(logger, manifestReference, manifestVersion, validationErrorsCount, null!);

        public static void VideoManifestChunksCreated(this ILogger logger, SwarmReference manifestReference, string chunksDirectory) =>
            _videoManifestChunksCreated(logger, manifestReference, chunksDirectory, null!);
    }
}
