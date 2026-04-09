using Catalog.API.Application.DTOs.Product;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Domain.Exceptions;

namespace Catalog.API.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repository, ILogger<ProductService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null)
        {
            _logger.LogInformation("Fetching all products (CategoryId: {CategoryId}, Status: {Status})", categoryId, status);

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
            _logger.LogInformation("Fetching product {ProductId}", id);

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
            _logger.LogInformation("Adding new product: {ProductName}", dto.Name);

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

            _logger.LogInformation("Product {ProductId} created successfully", savedProduct.Id);

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
            _logger.LogInformation("Updating product {ProductId}", id);

            var existingProduct = await _repository.GetProductByIdAsync(id);
            if (existingProduct == null) throw new NotFoundException("Product", id);

            existingProduct.Name = dto.Name;
            existingProduct.SKU = dto.SKU;
            existingProduct.Brand = dto.Brand;
            existingProduct.Description = dto.Description;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.PublishStatus = dto.PublishStatus;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateProductAsync(existingProduct);

            _logger.LogInformation("Product {ProductId} updated successfully", id);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            _logger.LogInformation("Deleting product {ProductId}", id);

            var product = await _repository.GetProductByIdAsync(id);
            if (product == null) throw new NotFoundException("Product", id);

            await _repository.DeleteProductAsync(product);

            _logger.LogInformation("Product {ProductId} deleted successfully", id);
        }
    }
}
