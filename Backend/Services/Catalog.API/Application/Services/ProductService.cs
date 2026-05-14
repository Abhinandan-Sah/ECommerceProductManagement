using Catalog.API.Application.DTOs.Product;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Domain.Exceptions;
using MassTransit;
using Shared.Messaging;

namespace Catalog.API.Application.Services
{
    /// <summary>
    /// Applies catalog product rules before product data is returned, created, updated, or deleted.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISkuGenerator _skuGenerator;
        private readonly ILogger<ProductService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Creates the product service with persistence, SKU generation, logging, and messaging dependencies.
        /// </summary>
        public ProductService(
            IProductRepository repository, 
            ICategoryRepository categoryRepository, 
            ISkuGenerator skuGenerator,
            ILogger<ProductService> logger,
            IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _skuGenerator = skuGenerator;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null, bool canViewUnpublished = false)
        {
            _logger.LogInformation("Fetching all products (CategoryId: {CategoryId}, Status: {Status})", categoryId, status);

            // Public users should only see products that have passed review or are live in the catalogue.
            // Internal roles can still filter and work with drafts during the merchandising flow.
            var publicStatuses = new[] { PublishStatus.Approved, PublishStatus.Published };

            if (!canViewUnpublished && status.HasValue && !publicStatuses.Contains(status.Value))
                return Enumerable.Empty<ProductResponseDto>();

            var products = await _repository.GetAllProductsAsync(categoryId, status);
            if (!canViewUnpublished && status == null)
                products = products.Where(p => publicStatuses.Contains(p.PublishStatus));

            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Brand = p.Brand,
                Description = p.Description,
                PublishStatus = p.PublishStatus,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized"
            });
        }

        /// <inheritdoc />
        public async Task<ProductResponseDto?> GetProductByIdAsync(Guid id, bool canViewUnpublished = false)
        {
            _logger.LogInformation("Fetching product {ProductId}", id);

            var product = await _repository.GetProductByIdAsync(id);
            if (product == null) return null;

            if (!canViewUnpublished && product.PublishStatus != PublishStatus.Approved && product.PublishStatus != PublishStatus.Published)
                return null;

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Brand = product.Brand,
                Description = product.Description,
                PublishStatus = product.PublishStatus,
                CategoryId = product.CategoryId,
                CategoryName = product.Category != null ? product.Category.Name : "Uncategorized"
            };
        }

        /// <inheritdoc />
        public async Task<ProductResponseDto> AddProductAsync(CreateProductDto dto)
        {
            _logger.LogInformation("Adding new product: {ProductName}", dto.Name);

            // Keep product creation strict here so we do not end up with orphaned catalogue data.
            var category = await _categoryRepository.GetCategoryByIdAsync(dto.CategoryId);
            if (category == null)
            {
                _logger.LogWarning("Cannot create product - Category {CategoryId} does not exist", dto.CategoryId);
                throw new BadRequestException($"Category with ID {dto.CategoryId} does not exist.");
            }

            // SKU is generated once from the commercial identity of the product and stays stable afterwards.
            var generatedSku = await _skuGenerator.GenerateSkuAsync(dto.Brand, dto.Name);

            var productEntity = new Product
            {
                Name = dto.Name,
                SKU = generatedSku,
                Brand = dto.Brand,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                PublishStatus = PublishStatus.Draft
            };

            var savedProduct = await _repository.AddProductAsync(productEntity);
            await PublishProductReportAsync(savedProduct, category.Name);

            _logger.LogInformation("Product {ProductId} created successfully with SKU {Sku}", savedProduct.Id, savedProduct.SKU);

            return new ProductResponseDto
            {
                Id = savedProduct.Id,
                Name = savedProduct.Name,
                SKU = savedProduct.SKU,
                Brand = savedProduct.Brand,
                Description = savedProduct.Description,
                PublishStatus = savedProduct.PublishStatus,
                CategoryId = savedProduct.CategoryId,
                CategoryName = savedProduct.Category?.Name ?? "Uncategorized"
            };
        }

        /// <inheritdoc />
        public async Task UpdateProductAsync(Guid id, UpdateProductDto dto, string callerRole)
        {
            _logger.LogInformation("Updating product {ProductId} by role {Role}", id, callerRole);

            var existingProduct = await _repository.GetProductByIdAsync(id);
            if (existingProduct == null) throw new NotFoundException("Product", id);

            // Product managers can prepare catalogue content, but final publish decisions stay with Admin
            // or the approval workflow. This keeps accidental direct publishing out of day-to-day edits.
            var adminOnlyStatuses = new[]
            {
                PublishStatus.Approved,
                PublishStatus.Published,
                PublishStatus.Rejected,
                PublishStatus.Archived
            };

            if (callerRole == "ProductManager" && adminOnlyStatuses.Contains(dto.PublishStatus))
            {
                _logger.LogWarning(
                    "ProductManager attempted to set restricted status {Status} on product {ProductId}",
                    dto.PublishStatus, id);
                throw new ForbiddenException(
                    $"ProductManager cannot set PublishStatus to '{dto.PublishStatus}'. " +
                    "Only Admin can set Approved, Published, Rejected, or Archived. " +
                    "Use the Workflow approval process instead.");
            }

            // Category is rechecked on update because the client may have stale dropdown data.
            var category = await _categoryRepository.GetCategoryByIdAsync(dto.CategoryId);
            if (category == null)
            {
                _logger.LogWarning("Cannot update product - Category {CategoryId} does not exist", dto.CategoryId);
                throw new BadRequestException($"Category with ID {dto.CategoryId} does not exist.");
            }

            existingProduct.Name = dto.Name;
            // SKU is intentionally not changed after creation; downstream reports and integrations treat it as stable.
            existingProduct.Brand = dto.Brand;
            existingProduct.Description = dto.Description;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.PublishStatus = dto.PublishStatus;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateProductAsync(existingProduct);
            await PublishProductReportAsync(existingProduct, category.Name);

            _logger.LogInformation("Product {ProductId} updated successfully", id);
        }

        /// <inheritdoc />
        public async Task DeleteProductAsync(Guid id)
        {
            _logger.LogInformation("Deleting product {ProductId}", id);

            var product = await _repository.GetProductByIdAsync(id);
            if (product == null) throw new NotFoundException("Product", id);

            await _repository.DeleteProductAsync(product);
            await _publishEndpoint.Publish(new ProductReportChangedEvent
            {
                ProductId = product.Id,
                IsDeleted = true
            });

            _logger.LogInformation("Product {ProductId} deleted successfully", id);
        }

        /// <summary>
        /// Publishes the product fields needed by Reporting.API to maintain its product read model.
        /// </summary>
        private Task PublishProductReportAsync(Product product, string categoryName)
        {
            // Reporting owns its read model, so Catalog only publishes the facts needed to rebuild it.
            return _publishEndpoint.Publish(new ProductReportChangedEvent
            {
                ProductId = product.Id,
                ProductName = product.Name,
                SKU = product.SKU,
                Status = product.PublishStatus.ToString(),
                CategoryName = categoryName,
                CreatedByUserId = Guid.Empty
            });
        }
    }
}
