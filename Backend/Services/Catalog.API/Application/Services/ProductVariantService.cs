using Catalog.API.Application.DTOs.ProductVariant;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;

        public ProductVariantService(IProductVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task<IEnumerable<ProductVariantResponseDto>> GetVariantsByProductAsync(Guid productId)
        {
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
            var variantEntity = new ProductVariant
            {
                ProductId = productId,
                Color = dto.Color,
                Size = dto.Size,
                Barcode = dto.Barcode
            };

            var saved = await _variantRepository.AddVariantAsync(variantEntity);

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
            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null || variant.ProductId != productId) throw new InvalidOperationException("Variant not found.");

            variant.Color = dto.Color;
            variant.Size = dto.Size;
            variant.Barcode = dto.Barcode;
            variant.UpdatedAt = DateTime.UtcNow;

            await _variantRepository.UpdateVariantAsync(variant);
        }

        public async Task DeleteVariantAsync(Guid productId, Guid id)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);
            if (variant == null || variant.ProductId != productId) throw new InvalidOperationException("Variant not found.");

            await _variantRepository.DeleteVariantAsync(variant);
        }
    }
}
