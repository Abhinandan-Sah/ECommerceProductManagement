using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines tracked media asset persistence operations for the catalog database.
    /// </summary>
    public interface IMediaAssetRepository
    {
        /// <summary>
        /// Reads media assets attached to a product in gallery display order.
        /// </summary>
        /// <param name="productId">Product identifier whose media should be returned.</param>
        /// <returns>Tracked media assets for the product, or an empty collection when none match.</returns>
        Task<IEnumerable<MediaAsset>> GetMediaByProductIdAsync(Guid productId);

        /// <summary>
        /// Finds one media asset by identifier.
        /// </summary>
        /// <param name="id">Media asset identifier to load.</param>
        /// <returns>The tracked matching media asset, or null when it does not exist.</returns>
        Task<MediaAsset?> GetMediaByIdAsync(Guid id);

        /// <summary>
        /// Adds a new media asset and saves the database change immediately.
        /// </summary>
        /// <param name="media">Media asset entity to add.</param>
        /// <returns>The tracked saved media asset entity.</returns>
        Task<MediaAsset> AddMediaAsync(MediaAsset media);

        /// <summary>
        /// Removes a media asset and saves the database change immediately.
        /// </summary>
        /// <param name="media">Media asset entity to remove.</param>
        Task DeleteMediaAsync(MediaAsset media);
    }
}
