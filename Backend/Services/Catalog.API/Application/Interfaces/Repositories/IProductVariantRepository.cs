using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines tracked product variant persistence operations for the catalog database.
    /// </summary>
    public interface IProductVariantRepository
    {
        /// <summary>
        /// Reads variants attached to a product.
        /// </summary>
        /// <param name="productId">Product identifier whose variants should be returned.</param>
        /// <returns>Tracked variants for the product, or an empty collection when none match.</returns>
        Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId);

        /// <summary>
        /// Finds one variant by identifier, including product navigation data.
        /// </summary>
        /// <param name="id">Variant identifier to load.</param>
        /// <returns>The tracked matching variant, or null when it does not exist.</returns>
        Task<ProductVariant?> GetVariantByIdAsync(Guid id);

        /// <summary>
        /// Adds a new product variant and saves the database change immediately.
        /// </summary>
        /// <param name="variant">Variant entity to add.</param>
        /// <returns>The tracked saved variant entity.</returns>
        Task<ProductVariant> AddVariantAsync(ProductVariant variant);

        /// <summary>
        /// Updates a product variant and saves the database change immediately.
        /// </summary>
        /// <param name="variant">Variant entity with updated values.</param>
        Task UpdateVariantAsync(ProductVariant variant);

        /// <summary>
        /// Removes a product variant and saves the database change immediately.
        /// </summary>
        /// <param name="variant">Variant entity to remove.</param>
        Task DeleteVariantAsync(ProductVariant variant);
    }
}
