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
using System.Net;

namespace Etherna.Sdk.Tools.UniversalFiles.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 4
     */
    internal static class LoggerExtensions
    {
        // Fields.
        //*** DEBUG LOGS ***
        private static readonly Action<ILogger, string, HttpStatusCode, Exception> _onlineFileRequestRefused =
            LoggerMessage.Define<string, HttpStatusCode>(
                LogLevel.Debug,
                new EventId(0, nameof(OnlineFileRequestRefused)),
                "Request of online file {FileUri} was answered with status {StatusCode}: the file is taken as not retrievable");

        private static readonly Action<ILogger, string, string, Exception> _uFileCopiedToLocalFile =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(1, nameof(UFileCopiedToLocalFile)),
                "File {FileUri} copied to local file {LocalFilePath}");

        //*** WARNING LOGS ***
        private static readonly Action<ILogger, string, Exception> _onlineFileHeadRequestFailed =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(2, nameof(OnlineFileHeadRequestFailed)),
                "Head request of online file {FileUri} failed: its byte size is taken as unknown");

        private static readonly Action<ILogger, string, Exception> _onlineFileRequestFailed =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(3, nameof(OnlineFileRequestFailed)),
                "Request of online file {FileUri} failed: the file is taken as not retrievable");

        private static readonly Action<ILogger, string, Exception> _swarmFileHeadersRequestFailed =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(4, nameof(SwarmFileHeadersRequestFailed)),
                "Headers request of Swarm file {FileUri} failed: the file is taken as not existing");

        // Methods.
        public static void OnlineFileHeadRequestFailed(this ILogger logger, string fileUri, Exception exception) =>
            _onlineFileHeadRequestFailed(logger, fileUri.RemoveUriSecrets(), exception);

        public static void OnlineFileRequestFailed(this ILogger logger, string fileUri, Exception exception) =>
            _onlineFileRequestFailed(logger, fileUri.RemoveUriSecrets(), exception);

        public static void OnlineFileRequestRefused(this ILogger logger, string fileUri, HttpStatusCode statusCode) =>
            _onlineFileRequestRefused(logger, fileUri.RemoveUriSecrets(), statusCode, null!);

        public static void SwarmFileHeadersRequestFailed(this ILogger logger, string fileUri, Exception exception) =>
            _swarmFileHeadersRequestFailed(logger, fileUri.RemoveUriSecrets(), exception);

        public static void UFileCopiedToLocalFile(this ILogger logger, string fileUri, string localFilePath) =>
            _uFileCopiedToLocalFile(logger, fileUri.RemoveUriSecrets(), localFilePath, null!);
    }
}
