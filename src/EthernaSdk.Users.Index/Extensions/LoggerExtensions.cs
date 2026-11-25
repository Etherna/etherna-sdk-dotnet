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

using Microsoft.Extensions.Logging;
using System;

namespace Etherna.Sdk.Users.Index.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 0
     */
    internal static class LoggerExtensions
    {
        // Fields.
        //*** WARNING LOGS ***
        private static readonly Action<ILogger, string?, Exception> _videoPreviewSkipped =
            LoggerMessage.Define<string?>(
                LogLevel.Warning,
                new EventId(0, nameof(VideoPreviewSkipped)),
                "Video {VideoId} skipped from the previews of the last published videos: the index returned it with incomplete data");

        // Methods.
        public static void VideoPreviewSkipped(this ILogger logger, string? videoId, Exception exception) =>
            _videoPreviewSkipped(logger, videoId, exception);
    }
}
