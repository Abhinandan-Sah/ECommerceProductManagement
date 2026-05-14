using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines tracked product persistence operations for the catalog database.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Reads products with optional category and publish status filters, including category navigation data.
        /// </summary>
        /// <param name="categoryId">Optional category identifier filter.</param>
        /// <param name="status">Optional publish status filter.</param>
        /// <returns>Tracked products matching the requested filters, or an empty collection when none match.</returns>
        Task<IEnumerable<Product>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null);

        /// <summary>
        /// Finds one product with category data loaded.
        /// </summary>
        /// <param name="Id">Product identifier to load.</param>
        /// <returns>The tracked matching product, or null when it does not exist.</returns>
        Task<Product?> GetProductByIdAsync(Guid Id);

        /// <summary>
        /// Adds a new product and saves the database change immediately.
        /// </summary>
        /// <param name="product">Product entity to add.</param>
        /// <returns>The tracked saved product entity.</returns>
        Task<Product> AddProductAsync(Product product);

        /// <summary>
        /// Updates a product and saves the database change immediately.
        /// </summary>
        /// <param name="product">Product entity with updated values.</param>
        Task UpdateProductAsync(Product product);

        /// <summary>
        /// Removes a product and saves the database change immediately.
        /// </summary>
        /// <param name="product">Product entity to remove.</param>
        Task DeleteProductAsync(Product product);

        /// <summary>
        /// Retrieves product SKUs that start with the specified prefix for SKU sequence generation.
        /// </summary>
        /// <param name="prefix">The SKU prefix to search for.</param>
        /// <returns>SKU strings matching the prefix, or an empty collection when none match.</returns>
        Task<IEnumerable<string>> GetSkusByPrefixAsync(string prefix);
    }
}
