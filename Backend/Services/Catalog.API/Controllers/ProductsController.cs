using Catalog.API.Application.DTOs;
using Catalog.API.Application.Interfaces;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAllProductsAsync()
        {
            var products = await _repository.GetAllProductsAsync();

            // Map the List of Entities to a List of DTOs
            var response = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Brand = p.Brand,
                Description = p.Description,
                PublishStatus = p.PublishStatus,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized"
            });

            return Ok(response); // Returns HTTP 200
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto?>> GetProductByIdAsync(Guid id)
        {
            var product = await _repository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(); // Returns HTTP 404 if the ID doesn't exist
            }

            // Map single Entity to DTO
            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Brand = product.Brand,
                Description = product.Description,
                PublishStatus = product.PublishStatus,
                CategoryName = product.Category != null ? product.Category.Name : "Uncategorized"
            };

            return Ok(response); // Returns HTTP 200
        }

        [HttpPost]
        [Authorize(Roles ="Admin,ProductManager,ContentExecutive")]
        public async Task<ActionResult<ProductResponseDto>> AddProductAsync(CreateProductDto newProductDto)
        {
            // 1. Map Request DTO -> Domain Entity
            var productEntity = new Product
            {
                Name = newProductDto.Name,
                SKU = newProductDto.SKU,
                Brand = newProductDto.Brand,
                Description = newProductDto.Description,
                CategoryId = newProductDto.CategoryId,
                PublishStatus = PublishStatus.Draft // Enforce Draft status on creation
            };

            // 2. Save via Repository
            var savedProduct = await _repository.AddProductAsync(productEntity);

            // 3. Map Saved Entity -> Response DTO
            var response = new ProductResponseDto
            {
                Id = savedProduct.Id,
                Name = savedProduct.Name,
                SKU = savedProduct.SKU,
                Brand = savedProduct.Brand,
                Description = savedProduct.Description,
                PublishStatus = savedProduct.PublishStatus,
                CategoryName = savedProduct.Category?.Name ?? "Uncategorized" // Repository insert might not instantly load the Category object
            };

            // Returns HTTP 201 Created and points to the GET endpoint to view the new item
            return CreatedAtAction(nameof(GetProductByIdAsync), new { id = response.Id }, response);
        }
    }
}
