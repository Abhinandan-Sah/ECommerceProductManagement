using Catalog.API.Application.DTOs;
using Catalog.API.Application.DTOs.MediaAsset;

namespace Catalog.API.Application.Interfaces.Services
{
    /// <summary>
    /// Defines media asset business operations.
    /// </summary>
    public interface IMediaAssetService
    {
        /// <summary>
        /// Gets media assets for a product after applying product visibility rules.
        /// </summary>
        /// <param name="productId">Product identifier whose media should be returned.</param>
        /// <param name="role">Caller role used to decide whether unpublished product media is visible.</param>
        /// <returns>Media assets visible to the caller.</returns>
        Task<IEnumerable<MediaAssetResponseDto>> GetMediaByProductAsync(Guid productId, string? role);

        /// <summary>
        /// Gets one media asset for a product after applying product visibility rules.
        /// </summary>
        /// <param name="productId">Product identifier that owns the media asset.</param>
        /// <param name="id">Media asset identifier to load.</param>
        /// <param name="role">Caller role used to decide whether unpublished product media is visible.</param>
        /// <returns>The visible media asset, or null when it is missing or hidden.</returns>
        Task<MediaAssetResponseDto?> GetMediaByIdAsync(Guid productId, Guid id, string? role);

        /// <summary>
        /// Adds a media asset to a product.
        /// </summary>
        /// <param name="productId">Product identifier that will own the media asset.</param>
        /// <param name="dto">Media asset details supplied by the caller.</param>
        /// <returns>The created media asset response.</returns>
        Task<MediaAssetResponseDto> AddMediaAsync(Guid productId, CreateMediaAssetDto dto);

        /// <summary>
        /// Removes a media asset from a product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the media asset.</param>
        /// <param name="id">Media asset identifier to delete.</param>
        Task DeleteMediaAsync(Guid productId, Guid id);
    }
}
