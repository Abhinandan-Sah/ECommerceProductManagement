namespace Catalog.API.Application.Interfaces.Services
{
    /// <summary>
    /// Service responsible for generating unique SKU values for products
    /// </summary>
    public interface ISkuGenerator
    {
        /// <summary>
        /// Generates a unique SKU for a product based on brand and name
        /// </summary>
        /// <param name="brand">Product brand</param>
        /// <param name="productName">Product name</param>
        /// <returns>Generated unique SKU in format BRAND-PRODUCTNAME-###</returns>
        Task<string> GenerateSkuAsync(string brand, string productName);
    }
}
