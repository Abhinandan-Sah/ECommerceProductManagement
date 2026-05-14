namespace Catalog.API.Application.Interfaces.Services
{
    /// <summary>
    /// Generates stable, unique product SKUs.
    /// </summary>
    public interface ISkuGenerator
    {
        /// <summary>
        /// Generates a unique SKU for a product based on brand and name.
        /// </summary>
        /// <param name="brand">Product brand.</param>
        /// <param name="productName">Product name.</param>
        /// <returns>A unique SKU in the format BRAND-PRODUCTNAME-###.</returns>
        Task<string> GenerateSkuAsync(string brand, string productName);
    }
}
