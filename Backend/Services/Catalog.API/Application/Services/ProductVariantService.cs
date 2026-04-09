using Catalog.API.Application.DTOs.ProductVariant;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Exceptions;

namespace Catalog.API.Application.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly ILogger<ProductVariantService> _logger;

        public ProductVariantService(IProductVariantRepository variantRepository, ILogger<ProductVariantService> logger)
        {
            _variantRepository = variantRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductVariantResponseDto>> GetVariantsByProductAsync(Guid productId)
        {
            _logger.LogInformation("Fetching variants for product {ProductId}", productId);

            var variants = await _variantRepository.GetVariantsByProductIdAsync(productId);
            return variants.Select(v => new ProductVariantResponseDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Color = v.Color,
                Size = v.Size,
                Barcode = v.Barcode
            });
        }

        public async Task<ProductVariantResponseDto?> GetVariantByIdAsync(Guid productId, Guid id)
        {
            _logger.LogInformation("Fetching variant {VariantId} for product {ProductId}", id, productId);

            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null || variant.ProductId != productId) return null;

            return new ProductVariantResponseDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Color = variant.Color,
                Size = variant.Size,
                Barcode = variant.Barcode
            };
        }

        public async Task<ProductVariantResponseDto> AddVariantAsync(Guid productId, CreateProductVariantDto dto)
        {
            _logger.LogInformation("Adding variant for product {ProductId}", productId);

            var variantEntity = new ProductVariant
            {
                ProductId = productId,
                Color = dto.Color,
                Size = dto.Size,
                Barcode = dto.Barcode
            };

            var saved = await _variantRepository.AddVariantAsync(variantEntity);

            _logger.LogInformation("Variant {VariantId} created for product {ProductId}", saved.Id, productId);

            return new ProductVariantResponseDto
            {
                Id = saved.Id,
                ProductId = saved.ProductId,
                Color = saved.Color,
                Size = saved.Size,
                Barcode = saved.Barcode
            };
        }

        public async Task UpdateVariantAsync(Guid productId, Guid id, UpdateProductVariantDto dto)
        {
            _logger.LogInformation("Updating variant {VariantId} for product {ProductId}", id, productId);

            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null || variant.ProductId != productId) throw new NotFoundException("ProductVariant", id);

            variant.Color = dto.Color;
            variant.Size = dto.Size;
            variant.Barcode = dto.Barcode;
            variant.UpdatedAt = DateTime.UtcNow;

            await _variantRepository.UpdateVariantAsync(variant);

            _logger.LogInformation("Variant {VariantId} updated successfully", id);
        }

        public async Task DeleteVariantAsync(Guid productId, Guid id)
        {
            _logger.LogInformation("Deleting variant {VariantId} for product {ProductId}", id, productId);

            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null || variant.ProductId != productId) throw new NotFoundException("ProductVariant", id);

            await _variantRepository.DeleteVariantAsync(variant);

            _logger.LogInformation("Variant {VariantId} deleted successfully", id);
        }
    }
}
