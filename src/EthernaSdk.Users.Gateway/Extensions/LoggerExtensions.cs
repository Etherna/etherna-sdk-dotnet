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

namespace Etherna.Sdk.Users.Gateway.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 9
     */
    internal static class LoggerExtensions
    {
        // Fields.
        //*** DEBUG LOGS ***
        private static readonly Action<ILogger, string, Exception> _postageBatchNotCreatedYet =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(0, nameof(PostageBatchNotCreatedYet)),
                "Postage batch with reference {BatchReferenceId} is not created yet");

        private static readonly Action<ILogger, PostageBatchId, Exception> _postageBatchNotUsableYet =
            LoggerMessage.Define<PostageBatchId>(
                LogLevel.Debug,
                new EventId(1, nameof(PostageBatchNotUsableYet)),
                "Postage batch {BatchId} is not usable yet");

        private static readonly Action<ILogger, PostageBatchId, Exception> _postageBatchReadFailed =
            LoggerMessage.Define<PostageBatchId>(
                LogLevel.Debug,
                new EventId(2, nameof(PostageBatchReadFailed)),
                "Read of postage batch {BatchId} failed while waiting for it to be usable");

        //*** INFORMATION LOGS ***
        private static readonly Action<ILogger, int, BzzValue, Exception> _dryModeSkippedPostageBatchPurchase =
            LoggerMessage.Define<int, BzzValue>(
                LogLevel.Information,
                new EventId(3, nameof(DryModeSkippedPostageBatchPurchase)),
                "Dry mode skipped the purchase of a postage batch with depth {Depth} and amount {Amount}");

        private static readonly Action<ILogger, SwarmReference, Exception> _dryModeSkippedResourceDownloadDefunding =
            LoggerMessage.Define<SwarmReference>(
                LogLevel.Information,
                new EventId(4, nameof(DryModeSkippedResourceDownloadDefunding)),
                "Dry mode skipped the download defunding of resource {Reference}");

        private static readonly Action<ILogger, SwarmReference, Exception> _dryModeSkippedResourceDownloadFunding =
            LoggerMessage.Define<SwarmReference>(
                LogLevel.Information,
                new EventId(5, nameof(DryModeSkippedResourceDownloadFunding)),
                "Dry mode skipped the download funding of resource {Reference}");

        private static readonly Action<ILogger, PostageBatchId, int, BzzValue, Exception> _postageBatchCreated =
            LoggerMessage.Define<PostageBatchId, int, BzzValue>(
                LogLevel.Information,
                new EventId(6, nameof(PostageBatchCreated)),
                "Postage batch {BatchId} created with depth {Depth} and amount {Amount}");

        private static readonly Action<ILogger, string, int, BzzValue, Exception> _postageBatchPurchaseRequested =
            LoggerMessage.Define<string, int, BzzValue>(
                LogLevel.Information,
                new EventId(7, nameof(PostageBatchPurchaseRequested)),
                "Postage batch purchase requested to the gateway with reference {BatchReferenceId}, depth {Depth} and amount {Amount}");

        private static readonly Action<ILogger, PostageBatchId, Exception> _postageBatchUsable =
            LoggerMessage.Define<PostageBatchId>(
                LogLevel.Information,
                new EventId(8, nameof(PostageBatchUsable)),
                "Postage batch {BatchId} is usable");

        //*** WARNING LOGS ***
        private static readonly Action<ILogger, string, int, Exception> _postageBatchReferenceReadFailed =
            LoggerMessage.Define<string, int>(
                LogLevel.Warning,
                new EventId(9, nameof(PostageBatchReferenceReadFailed)),
                "Read of postage batch reference {BatchReferenceId} failed with status {StatusCode}: the batch is taken as not created yet");

        // Methods.
        public static void DryModeSkippedPostageBatchPurchase(this ILogger logger, int depth, BzzValue amount) =>
            _dryModeSkippedPostageBatchPurchase(logger, depth, amount, null!);

        public static void DryModeSkippedResourceDownloadDefunding(this ILogger logger, SwarmReference reference) =>
            _dryModeSkippedResourceDownloadDefunding(logger, reference, null!);

        public static void DryModeSkippedResourceDownloadFunding(this ILogger logger, SwarmReference reference) =>
            _dryModeSkippedResourceDownloadFunding(logger, reference, null!);

        public static void PostageBatchCreated(this ILogger logger, PostageBatchId batchId, int depth, BzzValue amount) =>
            _postageBatchCreated(logger, batchId, depth, amount, null!);

        public static void PostageBatchNotCreatedYet(this ILogger logger, string batchReferenceId) =>
            _postageBatchNotCreatedYet(logger, batchReferenceId, null!);

        public static void PostageBatchNotUsableYet(this ILogger logger, PostageBatchId batchId) =>
            _postageBatchNotUsableYet(logger, batchId, null!);

        public static void PostageBatchPurchaseRequested(this ILogger logger, string batchReferenceId, int depth, BzzValue amount) =>
            _postageBatchPurchaseRequested(logger, batchReferenceId, depth, amount, null!);

        public static void PostageBatchReadFailed(this ILogger logger, PostageBatchId batchId, Exception exception) =>
            _postageBatchReadFailed(logger, batchId, exception);

        public static void PostageBatchReferenceReadFailed(this ILogger logger, string batchReferenceId, int statusCode, Exception exception) =>
            _postageBatchReferenceReadFailed(logger, batchReferenceId, statusCode, exception);

        public static void PostageBatchUsable(this ILogger logger, PostageBatchId batchId) =>
            _postageBatchUsable(logger, batchId, null!);
    }
}
