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

using Etherna.BeeNet;
using Etherna.BeeNet.Exceptions;
using Etherna.BeeNet.Models;
using Etherna.BeeNet.Stores;
using Etherna.Sdk.Index.GenClients;
using Etherna.Sdk.Tools.Video.Models;
using Etherna.Sdk.Users.Index.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VoteValue = Etherna.Sdk.Users.Index.Models.VoteValue;

namespace Etherna.Sdk.Users.Index.Clients
{
    public class EthernaUserIndexClient(
        Uri baseUrl,
        ISwarmClient swarmClient,
        HttpClient httpClient)
        : IEthernaUserIndexClient
    {
        // Fields.
        private readonly CommentsClient generatedCommentsClient = new(baseUrl.AbsoluteUri, httpClient);
        private readonly ModerationClient generatedModerationClient = new(baseUrl.AbsoluteUri, httpClient);
        private readonly SearchClient generatedSearchClient = new(baseUrl.AbsoluteUri, httpClient);
        private readonly SystemClient generatedSystemClient = new(baseUrl.AbsoluteUri, httpClient);
        private readonly UsersClient generatedUsersClient = new(baseUrl.AbsoluteUri, httpClient);
        private readonly VideosClient generatedVideosClient = new(baseUrl.AbsoluteUri, httpClient);

        // Methods.
        public Task AdminForceNewValidationByManifestReferenceAsync(
            SwarmReference manifestReference,
            CancellationToken cancellationToken = default) =>
            generatedSystemClient.ManifestAsync(manifestReference.ToString(), cancellationToken);

        public Task AdminForceNewValidationByVideoIdAsync(
            string videoId,
            CancellationToken cancellationToken = default) =>
            generatedSystemClient.VideoAsync(videoId, cancellationToken);

        public Task AdminReindexAllVideosAsync(CancellationToken cancellationToken = default) =>
            generatedSearchClient.RebuildAsync(cancellationToken);

        public async Task<Comment> CreateCommentAsync(
            string videoId,
            string commentText,
            CancellationToken cancellationToken = default)
        {
            var response = await generatedVideosClient.Comments2PostAsync(
                videoId,
                commentText,
                cancellationToken).ConfigureAwait(false);
            return new Comment(
                id: response.Id!,
                creationDateTime: response.CreationDateTime,
                isEditable: response.IsEditable,
                isFrozen: response.IsFrozen,
                ownerAddress: response.OwnerAddress,
                textHistory: response.TextHistory ?? new Dictionary<string, string>(),
                videoId: response.VideoId!);
        }

        public Task DeleteOwnedCommentAsync(string commentId, CancellationToken cancellationToken = default) =>
            generatedCommentsClient.CommentsAsync(commentId, cancellationToken);
        
        public async Task<IndexedVideo[]> GetAllVideosByOwnerAsync(
            string userAddress,
            Action<IndexedVideo>? onFoundVideo = null)
        {
            var videos = new List<IndexedVideo>();
            const int maxForPage = 100;

            PaginatedResult<IndexedVideo>? page = null;
            do
            {
                page = await GetVideosByOwnerAsync(
                    userAddress,
                    page is null ? 0 : page.CurrentPage + 1,
                    maxForPage,
                    onFoundVideo).ConfigureAwait(false);
                videos.AddRange(page.Elements);
            } while (page.Elements.Any());

            return videos.ToArray();
        }

        public async Task<IEnumerable<VideoValidationStatus>> GetBulkVideoValidationStatusByIdsAsync(
            IEnumerable<string> videoIds,
            CancellationToken cancellationToken = default) =>
            (await generatedVideosClient.BulkValidation2Async(videoIds, cancellationToken).ConfigureAwait(false)).Select(s => new VideoValidationStatus(
                errorDetails: s.ErrorDetails.Select(e => new VideoValidationErrorDetail(
                    errorMessage: e.ErrorMessage,
                    errorType: Enum.Parse<VideoValidationErrorDetail.ErrorTypes>(e.ErrorType.ToString()))),
                reference: s.Hash,
                isValid: s.IsValid,
                validationTime: s.ValidationTime,
                videoId: s.VideoId));

        public async Task<IEnumerable<VideoValidationStatus>> GetBulkVideoValidationStatusByManifestsAsync(
            IEnumerable<SwarmReference> manifestReferences,
            CancellationToken cancellationToken = default) =>
            (await generatedVideosClient.BulkValidationPut2Async(
                manifestReferences.Select(h => h.ToString()), cancellationToken).ConfigureAwait(false)).Select(s => new VideoValidationStatus(
                    errorDetails: s.ErrorDetails.Select(e => new VideoValidationErrorDetail(
                        errorMessage: e.ErrorMessage,
                        errorType: Enum.Parse<VideoValidationErrorDetail.ErrorTypes>(e.ErrorType.ToString()))),
                    reference: s.Hash,
                    isValid: s.IsValid,
                    validationTime: s.ValidationTime,
                    videoId: s.VideoId));

        public async Task<IndexUserInfo> GetCurrentUserInfo(CancellationToken cancellationToken = default)
        {
            var response = await generatedUsersClient.CurrentAsync(cancellationToken).ConfigureAwait(false);
            return new IndexUserInfo(
                response.Id!,
                response.Address,
                response.CreationDateTime,
                response.IsSuperModerator);
        }

        public async Task<IndexParameters> GetIndexParametersAsync(CancellationToken cancellationToken = default)
        {
            var response = await generatedSystemClient.ParametersAsync(cancellationToken).ConfigureAwait(false);
            return new IndexParameters(
                commentMaxLength: response.CommentMaxLength,
                videoDescriptionMaxLength: response.VideoDescriptionMaxLength,
                videoPersonalDataMaxLength: response.VideoPersonalDataMaxLength,
                videoTitleMaxLength: response.VideoTitleMaxLength);
        }

        public async Task<PaginatedResult<VideoPreview>> GetLastPublishedVideosAsync(int? page = null, int? take = null, CancellationToken cancellationToken = default)
        {
            var chunkStore = new SwarmClientChunkStore(swarmClient);
            
            // Get API result.
            var result = await generatedVideosClient.Latest3Async(page, take, cancellationToken).ConfigureAwait(false);

            // Build response.
            List<VideoPreview> videoPreviews = [];
            foreach (var v in (result.Elements ?? []).Where(v => v.Hash is not null))
            {
                try
                {
                    List<VideoManifestImageSource> thumbnailSources = [];
                    foreach (var thumbSourceDto in v.Thumbnail!.Sources)
                    {
                        if (!Enum.TryParse<ImageType>(thumbSourceDto.Type, true, out var imageType))
                            imageType = ImageType.Jpeg; //only used jpeg at first

                        var fileName = thumbSourceDto.Path.Split(SwarmAddress.Separator).Last();
                        if (!Path.HasExtension(fileName))
                            fileName += imageType switch
                            {
                                ImageType.Avif => ".avif",
                                ImageType.Jpeg => ".jpeg",
                                ImageType.Png => ".png",
                                ImageType.Webp => ".webp",
                                _ => throw new InvalidOperationException()
                            };

                        var swarmUri = new SwarmUri(thumbSourceDto.Path, UriKind.RelativeOrAbsolute);

                        var thumbAddress = swarmUri.ToSwarmAddress(v.Hash!);
                        var thumbChunkRef = await SwarmReference.ResolveFromAddressAsync(thumbAddress, chunkStore).ConfigureAwait(false);
                        
                        var thumbSource = new VideoManifestImageSource(
                            fileName,
                            imageType,
                            thumbSourceDto.Width,
                            thumbChunkRef.Hash);
                        
                        thumbnailSources.Add(thumbSource);
                    }
                
                    videoPreviews.Add(new VideoPreview(
                        reference: v.Hash is not null ?
                            v.Hash : (SwarmReference?)null,
                        id: v.Id!,
                        createdAt: v.CreatedAt,
                        duration: v.Duration,
                        ownerAddress: v.OwnerAddress,
                        thumbnail: new VideoManifestImage(
                            aspectRatio: v.Thumbnail.AspectRatio,
                            blurhash: v.Thumbnail.Blurhash,
                            sources: thumbnailSources),
                        title: v.Title,
                        updatedAt: v.UpdatedAt));
                }
                catch (NullReferenceException)
                {
                }
            }
            
            return new PaginatedResult<VideoPreview>(
                videoPreviews,
                result.TotalElements,
                result.PageSize,
                result.CurrentPage,
                result.MaxPage);
        }

        public async Task<PaginatedResult<IndexUserInfo>> GetRegisteredUsersAsync(int? page = null, int? take = null, CancellationToken cancellationToken = default)
        {
            var result = await generatedUsersClient.List2Async(page, take, cancellationToken).ConfigureAwait(false);
            return new PaginatedResult<IndexUserInfo>(
                (result.Elements ?? []).Select(u => new IndexUserInfo(
                    id: u.Id!,
                    address: u.Address,
                    creationDateTime: u.CreationDateTime,
                    null)),
                result.TotalElements,
                result.PageSize,
                result.CurrentPage,
                result.MaxPage);
        }

        public async Task<IndexUserInfo> GetUserInfoByAddressAsync(
            string address,
            CancellationToken cancellationToken = default)
        {
            var response = await generatedUsersClient.UsersGetAsync(address, cancellationToken).ConfigureAwait(false);
            return new IndexUserInfo(
                id: response.Id!,
                address: response.Address,
                creationDateTime: response.CreationDateTime,
                null);
        }

        public async Task<IndexedVideo> GetVideoByIdAsync(
            string videoId,
            CancellationToken cancellationToken = default)
        {
            var response = await generatedVideosClient.Find2Async(videoId, cancellationToken).ConfigureAwait(false);
            return BuildIndexedVideo(response);
        }

        public async Task<IndexedVideo> GetVideoByManifestAsync(
            SwarmReference manifestReference,
            CancellationToken cancellationToken = default)
        {
            var response = await generatedVideosClient.Manifest2Async(manifestReference.ToString(), cancellationToken).ConfigureAwait(false);
            return BuildIndexedVideo(response);
        }

        public async Task<PaginatedResult<Comment>> GetVideoCommentsAsync(string videoId, int? page = null, int? take = null,
            CancellationToken cancellationToken = default)
        {
            var result = await generatedVideosClient.Comments3Async(videoId, page, take, cancellationToken).ConfigureAwait(false);
            return new PaginatedResult<Comment>(
                (result.Elements ?? []).Select(c => new Comment(
                    id: c.Id!,
                    creationDateTime: c.CreationDateTime,
                    isEditable: c.IsEditable,
                    isFrozen: c.IsFrozen,
                    ownerAddress: c.OwnerAddress,
                    textHistory: c.TextHistory ?? new Dictionary<string, string>(),
                    videoId: c.VideoId!)),
                result.TotalElements,
                result.PageSize,
                result.CurrentPage,
                result.MaxPage);
        }

        public async Task<PaginatedResult<IndexedVideo>> GetVideosByOwnerAsync(
            string userAddress,
            int? page = null,
            int? take = null,
            Action<IndexedVideo>? onFoundVideo = null,
            CancellationToken cancellationToken = default)
        {
            var result = await generatedUsersClient.Videos3Async(userAddress, page, take, cancellationToken).ConfigureAwait(false);

            List<IndexedVideo> indexedVideos = [];
            foreach (var videoDto in result.Elements ?? [])
                try
                {
                    var indexedVideo = BuildIndexedVideo(videoDto);
                    indexedVideos.Add(indexedVideo);

                    onFoundVideo?.Invoke(indexedVideo);
                }
                catch (BeeNetApiException e) when(e.StatusCode == 404)
                { }
            
            return new PaginatedResult<IndexedVideo>(
                indexedVideos,
                result.TotalElements,
                result.PageSize,
                result.CurrentPage,
                result.MaxPage);
        }

        public async Task<IEnumerable<VideoValidationStatus>> GetVideoValidationStatusByIdAsync(string videoId, CancellationToken cancellationToken = default) =>
            (await generatedVideosClient.Validation2Async(videoId, cancellationToken).ConfigureAwait(false)).Select(s => new VideoValidationStatus(
                errorDetails: s.ErrorDetails.Select(e => new VideoValidationErrorDetail(
                    errorMessage: e.ErrorMessage,
                    errorType: Enum.Parse<VideoValidationErrorDetail.ErrorTypes>(e.ErrorType.ToString()))),
                reference: s.Hash,
                isValid: s.IsValid,
                validationTime: s.ValidationTime,
                videoId: s.VideoId));

        public async Task<VideoValidationStatus> GetVideoValidationStatusByManifestAsync(
            SwarmReference manifestReference,
            CancellationToken cancellationToken = default)
        {
            var response = await generatedVideosClient.ValidationGet2Async(
                manifestReference.ToString(),
                cancellationToken).ConfigureAwait(false);
            return new VideoValidationStatus(
                errorDetails: response.ErrorDetails.Select(e => new VideoValidationErrorDetail(
                    errorMessage: e.ErrorMessage,
                    errorType: Enum.Parse<VideoValidationErrorDetail.ErrorTypes>(e.ErrorType.ToString()))),
                reference: response.Hash,
                isValid: response.IsValid,
                validationTime: response.ValidationTime,
                videoId: response.VideoId);
        }

        public Task ModerateCommentAsync(string commentId, CancellationToken cancellationToken = default) =>
            generatedModerationClient.CommentsAsync(commentId, cancellationToken);

        public Task ModerateVideoAsync(string videoId, CancellationToken cancellationToken = default) =>
            generatedModerationClient.VideosAsync(videoId, cancellationToken);

        public Task OwnerRemoveVideoAsync(string videoId, CancellationToken cancellationToken = default) =>
            generatedVideosClient.VideosDeleteAsync(videoId, cancellationToken);

        public Task<string> PublishNewVideoAsync(SwarmReference manifestReference, CancellationToken cancellationToken = default) =>
            generatedVideosClient.VideosPostAsync(new VideoCreateInput { ManifestHash = manifestReference.ToString() }, cancellationToken);

        public Task ReportUnsuitableVideoAsync(string videoId, SwarmReference manifestReference, string description, CancellationToken cancellationToken = default) =>
            generatedVideosClient.ReportsAsync(videoId, manifestReference.ToString(), description, cancellationToken);

        public Task<PaginatedResult<VideoPreview>> SearchVideosAsync(string? query = null, int? page = null, int? take = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOwnedVideoCommentAsync(string commentId, string newCommentText, CancellationToken cancellationToken = default) =>
            generatedVideosClient.CommentsPutAsync(commentId, newCommentText, cancellationToken);

        public Task UpdateVideoManifestAsync(string videoId, SwarmReference newManifestReference, CancellationToken cancellationToken = default) =>
            generatedVideosClient.Update2Async(videoId, newManifestReference.ToString(), cancellationToken);

        public Task VotesVideoAsync(string id, VoteValue value, CancellationToken cancellationToken = default) =>
            generatedVideosClient.VotesAsync(id, Enum.Parse<Sdk.Index.GenClients.VoteValue>(value.ToString()), cancellationToken);
        
        // Helpers.
        private static IndexedVideo BuildIndexedVideo(Video2Dto videoDto)
        {
            ArgumentNullException.ThrowIfNull(videoDto);

            // Build published video manifest.
            SwarmReference? lastValidManifestReference = null;
            if (videoDto.LastValidManifest is not null)
                lastValidManifestReference = SwarmReference.FromString(videoDto.LastValidManifest.Hash);
            
            // Build indexed video.
            VideoManifestPersonalData.TryDeserialize(videoDto.LastValidManifest?.PersonalData, out var personalData);
            return new IndexedVideo(
                id: videoDto.Id!,
                creationDateTime: videoDto.CreationDateTime,
                currentVoteValue: videoDto.CurrentVoteValue.HasValue ?
                    Enum.Parse<VoteValue>(videoDto.CurrentVoteValue.Value.ToString()) : null,
                description: videoDto.LastValidManifest?.Description,
                lastValidManifestReference: lastValidManifestReference,
                ownerAddress: videoDto.OwnerAddress,
                personalData: personalData,
                title: videoDto.LastValidManifest?.Title,
                totDownvotes: videoDto.TotDownvotes,
                totUpvotes: videoDto.TotUpvotes);
        }
    }
}
