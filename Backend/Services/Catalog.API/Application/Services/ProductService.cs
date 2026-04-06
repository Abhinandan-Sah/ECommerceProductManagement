using Catalog.API.Application.DTOs.Product;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null)
        {
            var products = await _repository.GetAllProductsAsync(categoryId, status);
            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Brand = p.Brand,
                Description = p.Description,
                PublishStatus = p.PublishStatus,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized"
            });
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(Guid id)
        {
            var product = await _repository.GetProductByIdAsync(id);
            if (product == null) return null;

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Brand = product.Brand,
                Description = product.Description,
                PublishStatus = product.PublishStatus,
                CategoryName = product.Category != null ? product.Category.Name : "Uncategorized"
            };
        }

        public async Task<ProductResponseDto> AddProductAsync(CreateProductDto dto)
        {
            var productEntity = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                Brand = dto.Brand,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                PublishStatus = PublishStatus.Draft
            };

            var savedProduct = await _repository.AddProductAsync(productEntity);

            return new ProductResponseDto
            {
                Id = savedProduct.Id,
                Name = savedProduct.Name,
                SKU = savedProduct.SKU,
                Brand = savedProduct.Brand,
                Description = savedProduct.Description,
                PublishStatus = savedProduct.PublishStatus,
                CategoryName = savedProduct.Category?.Name ?? "Uncategorized"
            };
        }

        public async Task UpdateProductAsync(Guid id, UpdateProductDto dto)
        {
            var existingProduct = await _repository.GetProductByIdAsync(id);
            if (existingProduct == null) throw new InvalidOperationException("Product not found.");

            existingProduct.Name = dto.Name;
            existingProduct.SKU = dto.SKU;
            existingProduct.Brand = dto.Brand;
            existingProduct.Description = dto.Description;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.PublishStatus = dto.PublishStatus;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateProductAsync(existingProduct);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _repository.GetProductByIdAsync(id);
            if (product == null) throw new InvalidOperationException("Product not found.");

            await _repository.DeleteProductAsync(product);
        }
    }
}
