using Catalog.API.Application.DTOs.ProductVariant;

namespace Catalog.API.Application.Interfaces.Services
{
    /// <summary>
    /// Defines product variant business operations.
    /// </summary>
    public interface IProductVariantService
    {
        /// <summary>
        /// Gets all variants that belong to a product.
        /// </summary>
        /// <param name="productId">Product identifier whose variants should be returned.</param>
        /// <returns>Variants attached to the product.</returns>
        Task<IEnumerable<ProductVariantResponseDto>> GetVariantsByProductAsync(Guid productId);

        /// <summary>
        /// Gets one variant only when it belongs to the requested product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the variant.</param>
        /// <param name="id">Variant identifier to load.</param>
        /// <returns>The matching variant, or null when it is missing or belongs to another product.</returns>
        Task<ProductVariantResponseDto?> GetVariantByIdAsync(Guid productId, Guid id);

        /// <summary>
        /// Creates a variant under an existing product.
        /// </summary>
        /// <param name="productId">Product identifier that will own the variant.</param>
        /// <param name="dto">Variant details supplied by the caller.</param>
        /// <returns>The created variant response.</returns>
        Task<ProductVariantResponseDto> AddVariantAsync(Guid productId, CreateProductVariantDto dto);

        /// <summary>
        /// Updates a variant that belongs to a product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the variant.</param>
        /// <param name="id">Variant identifier to update.</param>
        /// <param name="dto">New variant values.</param>
        Task UpdateVariantAsync(Guid productId, Guid id, UpdateProductVariantDto dto);

        /// <summary>
        /// Deletes a variant that belongs to a product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the variant.</param>
        /// <param name="id">Variant identifier to delete.</param>
        Task DeleteVariantAsync(Guid productId, Guid id);
    }
}
