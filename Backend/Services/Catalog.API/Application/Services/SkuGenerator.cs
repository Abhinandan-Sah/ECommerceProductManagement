using System.Text.RegularExpressions;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;

namespace Catalog.API.Application.Services
{
    public class SkuGenerator : ISkuGenerator
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<SkuGenerator> _logger;

        public SkuGenerator(
            IProductRepository productRepository,
            ILogger<SkuGenerator> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<string> GenerateSkuAsync(string brand, string productName)
        {
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Brand cannot be null or empty", nameof(brand));
            
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be null or empty", nameof(productName));

            // 1. Normalize brand and product name
            var normalizedBrand = NormalizeComponent(brand);
            var normalizedName = NormalizeComponent(productName);
            
            // 2. Create base SKU prefix
            var basePrefix = $"{normalizedBrand}-{normalizedName}";
            
            // 3. Query database for existing SKUs with this prefix
            var nextSequence = await GetNextSequenceNumberAsync(basePrefix);
            
            // 4. Generate final SKU
            var sku = $"{basePrefix}-{nextSequence:D3}";
            
            _logger.LogInformation(
                "Generated SKU {Sku} for brand {Brand} and product {ProductName}",
                sku, brand, productName);
            
            return sku;
        }

        private string NormalizeComponent(string component)
        {
            // Convert to uppercase
            var normalized = component.ToUpperInvariant();
            
            // Replace non-alphanumeric characters with hyphens
            normalized = Regex.Replace(normalized, @"[^A-Z0-9]+", "-");
            
            // Remove consecutive hyphens
            normalized = Regex.Replace(normalized, @"-+", "-");
            
            // Trim hyphens from start and end
            normalized = normalized.Trim('-');
            
            return normalized;
        }

        private async Task<int> GetNextSequenceNumberAsync(string basePrefix)
        {
            // Query for all SKUs starting with the base prefix
            var existingSkus = await _productRepository.GetSkusByPrefixAsync(basePrefix);
            
            if (!existingSkus.Any())
            {
                return 1;
            }
            
            // Extract sequence numbers from existing SKUs
            var sequenceNumbers = existingSkus
                .Select(sku => ExtractSequenceNumber(sku))
                .Where(seq => seq.HasValue)
                .Select(seq => seq!.Value)
                .ToList();
            
            // Return max + 1, or 1 if no valid sequences found
            return sequenceNumbers.Any() ? sequenceNumbers.Max() + 1 : 1;
        }

        private int? ExtractSequenceNumber(string sku)
        {
            // Extract the last segment after the final hyphen
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
