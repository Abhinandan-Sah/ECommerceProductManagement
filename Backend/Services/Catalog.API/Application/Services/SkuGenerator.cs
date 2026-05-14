using System.Text.RegularExpressions;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;

namespace Catalog.API.Application.Services
{
    /// <summary>
    /// Generates readable, unique SKUs from product brand and name.
    /// </summary>
    public class SkuGenerator : ISkuGenerator
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<SkuGenerator> _logger;

        /// <summary>
        /// Creates the SKU generator with product lookup and logging dependencies.
        /// </summary>
        public SkuGenerator(
            IProductRepository productRepository,
            ILogger<SkuGenerator> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> GenerateSkuAsync(string brand, string productName)
        {
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Brand cannot be null or empty", nameof(brand));
            
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be null or empty", nameof(productName));

            // Normalize the readable parts first so the SKU stays predictable for support and operations teams.
            var normalizedBrand = NormalizeComponent(brand);
            var normalizedName = NormalizeComponent(productName);
            
            var basePrefix = $"{normalizedBrand}-{normalizedName}";
            
            // The numeric suffix keeps duplicate brand/product combinations unique without losing readability.
            var nextSequence = await GetNextSequenceNumberAsync(basePrefix);
            
            var sku = $"{basePrefix}-{nextSequence:D3}";
            
            _logger.LogInformation(
                "Generated SKU {Sku} for brand {Brand} and product {ProductName}",
                sku, brand, productName);
            
            return sku;
        }

        /// <summary>
        /// Normalizes one brand or product-name segment into the SKU-safe text format.
        /// </summary>
        private string NormalizeComponent(string component)
        {
            // Keep only stable, URL-safe characters. This avoids surprises when SKUs are used in exports or URLs.
            var normalized = component.ToUpperInvariant();
            
            normalized = Regex.Replace(normalized, @"[^A-Z0-9]+", "-");
            
            normalized = Regex.Replace(normalized, @"-+", "-");
            
            normalized = normalized.Trim('-');
            
            return normalized;
        }

        /// <summary>
        /// Finds the next available numeric suffix for a SKU prefix.
        /// </summary>
        private async Task<int> GetNextSequenceNumberAsync(string basePrefix)
        {
            // Pull only matching SKUs; the sequence is local to a brand/product prefix.
            var existingSkus = await _productRepository.GetSkusByPrefixAsync(basePrefix);
            
            if (!existingSkus.Any())
            {
                return 1;
            }
            
            // Ignore malformed historical SKUs instead of blocking new catalogue entries.
            var sequenceNumbers = existingSkus
                .Select(sku => ExtractSequenceNumber(sku))
                .Where(seq => seq.HasValue)
                .Select(seq => seq!.Value)
                .ToList();
            
            return sequenceNumbers.Any() ? sequenceNumbers.Max() + 1 : 1;
        }

        /// <summary>
        /// Reads the numeric suffix from an existing SKU.
        /// </summary>
        private int? ExtractSequenceNumber(string sku)
        {
            // The suffix is always the segment after the final hyphen, for example ABC-SHIRT-003.
            var lastHyphenIndex = sku.LastIndexOf('-');
            if (lastHyphenIndex == -1 || lastHyphenIndex == sku.Length - 1)
            {
                return null;
            }
            
            var sequenceStr = sku.Substring(lastHyphenIndex + 1);
            return int.TryParse(sequenceStr, out var sequence) ? sequence : null;
        }
    }
}
