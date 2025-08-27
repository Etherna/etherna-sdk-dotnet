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
using Etherna.Sdk.Index.GenClients;
using Etherna.Sdk.Users.Index.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Index.Clients
{
    public interface IEthernaUserIndexClient
    {
        /// <summary>
        /// Force new validation of video manifest.
        /// </summary>
        /// <param name="manifestReference">Hash manifest</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task AdminForceNewValidationByManifestReferenceAsync(
            SwarmReference manifestReference,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Force new validation of video manifests.
        /// </summary>
        /// <param name="videoId">Video id</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task AdminForceNewValidationByVideoIdAsync(string videoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Admin starts new full video re-index
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task AdminReindexAllVideosAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new comment on a video with current user.
        /// </summary>
        /// <param name="videoId">Video id</param>
        /// <param name="commentText">Comment text</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<Comment> CreateCommentAsync(
            string videoId,
            string commentText,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a comment owned by current user.
        /// </summary>
        /// <param name="commentId">Comment id</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task DeleteOwnedCommentAsync(string commentId, CancellationToken cancellationToken = default);

        Task<IndexedVideo[]> GetAllVideosByOwnerAsync(
            string userAddress,
            Action<IndexedVideo>? onFoundVideo = null);
        
        /// <summary>
        /// Get bulk validation info by multiple video ids.
        /// </summary>
        /// <param name="videoIds">The list of video id</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<IEnumerable<VideoValidationStatus>> GetBulkVideoValidationStatusByIdsAsync(
            IEnumerable<string> videoIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get bulk validation info by multiple manifest hashes.
        /// </summary>
        /// <param name="manifestReferences">The list of video manifest hashes</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<IEnumerable<VideoValidationStatus>> GetBulkVideoValidationStatusByManifestsAsync(
            IEnumerable<SwarmReference> manifestReferences,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current user info
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Current usre info</returns>
        Task<IndexUserInfo> GetCurrentUserInfo(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get list of last uploaded videos.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Current page on list</returns>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<PaginatedResult<VideoPreview>> GetLastPublishedVideosAsync(
            int? page = null,
            int? take = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a complete list of users.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Current page on list</returns>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<PaginatedResult<IndexUserInfo>> GetRegisteredUsersAsync(
            int? page = null,
            int? take = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get list of configuration parameters.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Configuration parameters</returns>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<IndexParameters> GetIndexParametersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user info by address.
        /// </summary>
        /// <param name="address">The user ether address</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<IndexUserInfo> GetUserInfoByAddressAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get video info by id.
        /// </summary>
        /// <param name="videoId">The video id</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<IndexedVideo> GetVideoByIdAsync(string videoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get video info by manifest hash.
        /// </summary>
        /// <param name="manifestReference">The video manifest hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<IndexedVideo> GetVideoByManifestAsync(
            SwarmReference manifestReference,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated video comments by id
        /// </summary>
        /// <param name="videoId">Video id</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Current page on list</returns>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<PaginatedResult<Comment>> GetVideoCommentsAsync(
            string videoId,
            int? page = null,
            int? take = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get list of videos uploaded by an user.
        /// </summary>
        /// <param name="userAddress">Address of user</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>List of user's videos</returns>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<PaginatedResult<IndexedVideo>> GetVideosByOwnerAsync(
            string userAddress,
            int? page = null,
            int? take = null,
            Action<IndexedVideo>? onFoundVideo = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get validation info by id.
        /// </summary>
        /// <param name="videoId">Video id</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<IEnumerable<VideoValidationStatus>> GetVideoValidationStatusByIdAsync(
            string videoId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get validation info by manifest hash.
        /// </summary>
        /// <param name="manifestReference">The video hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<VideoValidationStatus> GetVideoValidationStatusByManifestAsync(
            SwarmReference manifestReference,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Moderate comment as unsuitable for the index
        /// </summary>
        /// <param name="commentId">Id of the comment</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task ModerateCommentAsync(string commentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Moderate video as unsuitable for the index
        /// </summary>
        /// <param name="videoId">Id of the video</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task ModerateVideoAsync(string videoId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete a video from index. Only author is authorized
        /// </summary>
        /// <param name="videoId">Id of the video</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task OwnerRemoveVideoAsync(string videoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new video with current user.
        /// </summary>
        /// <param name="manifestReference">Manifest hash of the new video</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>New video id</returns>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<string> PublishNewVideoAsync(
            SwarmReference manifestReference,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Report a video content with current user.
        /// </summary>
        /// <param name="videoId">Video id</param>
        /// <param name="manifestReference">Hash manifest</param>
        /// <param name="description">Report description</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task ReportUnsuitableVideoAsync(
            string videoId,
            SwarmReference manifestReference,
            string description,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Search videos.
        /// </summary>
        /// <param name="query">Query to search in title and description</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Videos</returns>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task<PaginatedResult<VideoPreview>> SearchVideosAsync(
            string? query = null,
            int? page = null,
            int? take = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Edit a video comment with current author user.
        /// </summary>
        /// <param name="commentId">Comment id</param>
        /// <param name="newCommentText">Comment text</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task UpdateOwnedVideoCommentAsync(
            string commentId,
            string newCommentText,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update video manifest.
        /// </summary>
        /// <param name="videoId">The video id</param>
        /// <param name="newManifestReference">The new video manifest hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task UpdateVideoManifestAsync(
            string videoId,
            SwarmReference newManifestReference,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Vote a video content with current user.
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="value">Vote value</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaIndexApiException">A server side error occurred.</exception>
        Task VotesVideoAsync(
            string id,
            VoteValue value,
            CancellationToken cancellationToken = default);
    }
}