using Catalog.API.Application.DTOs.Product;
using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.Interfaces.Services
{
    /// <summary>
    /// Defines product business operations used by the Catalog API.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Gets products visible to the caller, optionally filtered by category and publish status.
        /// </summary>
        /// <param name="categoryId">Optional category filter.</param>
        /// <param name="status">Optional publish status filter.</param>
        /// <param name="canViewUnpublished">True when the caller can access draft or internal product states.</param>
        /// <returns>The matching product response records.</returns>
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null, bool canViewUnpublished = false);

        /// <summary>
        /// Gets a single product when it exists and is visible to the caller.
        /// </summary>
        /// <param name="id">Product identifier.</param>
        /// <param name="canViewUnpublished">True when the caller can access draft or internal product states.</param>
        /// <returns>The product response, or null when it is missing or hidden from the caller.</returns>
        Task<ProductResponseDto?> GetProductByIdAsync(Guid id, bool canViewUnpublished = false);

        /// <summary>
        /// Creates a product in draft state.
        /// </summary>
        /// <param name="dto">Product details supplied by the client.</param>
        /// <returns>The created product with generated identifiers and SKU.</returns>
        Task<ProductResponseDto> AddProductAsync(CreateProductDto dto);

        /// <summary>
        /// Updates product details while enforcing role-based publish status rules.
        /// </summary>
        /// <param name="id">Product identifier.</param>
        /// <param name="dto">Updated product values.</param>
        /// <param name="callerRole">Role of the authenticated caller.</param>
        Task UpdateProductAsync(Guid id, UpdateProductDto dto, string callerRole);

        /// <summary>
        /// Deletes a product and notifies reporting projections.
        /// </summary>
        /// <param name="id">Product identifier.</param>
        Task DeleteProductAsync(Guid id);
    }
}
