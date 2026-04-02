using Catalog.API.Application.DTOs.ProductVariant;
using Catalog.API.Application.Interfaces;
using Catalog.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/variants")]  // Nested under /api/products/{productId}
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;

        public ProductVariantsController(
            IProductVariantRepository variantRepository,
            IProductRepository productRepository)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
        }

        // GET: /api/products/{productId}/variants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVariantResponseDto>>> GetVariantsByProductAsync(Guid productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = $"Product {productId} not found." });

            var variants = await _variantRepository.GetVariantsByProductIdAsync(productId);

            var response = variants.Select(v => new ProductVariantResponseDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Color = v.Color,
                Size = v.Size,
                Barcode = v.Barcode
            });

            return Ok(response);
        }

        // GET: /api/products/{productId}/variants/{id}
        [HttpGet("{id}", Name = "GetVariantById")]
        public async Task<ActionResult<ProductVariantResponseDto>> GetVariantByIdAsync(Guid productId, Guid id)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);

            if (variant == null || variant.ProductId != productId)
                return NotFound();

            var response = new ProductVariantResponseDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Color = variant.Color,
                Size = variant.Size,
                Barcode = variant.Barcode
            };

            return Ok(response);
        }

        // POST: /api/products/{productId}/variants
        [HttpPost]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<ActionResult<ProductVariantResponseDto>> AddVariantAsync(Guid productId, [FromBody] CreateProductVariantDto dto)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = $"Product {productId} not found." });

            var variantEntity = new ProductVariant
            {
                ProductId = productId,
                Color = dto.Color,
                Size = dto.Size,
                Barcode = dto.Barcode
            };

            var saved = await _variantRepository.AddVariantAsync(variantEntity);

            var response = new ProductVariantResponseDto
            {
                Id = saved.Id,
                ProductId = saved.ProductId,
                Color = saved.Color,
                Size = saved.Size,
                Barcode = saved.Barcode
            };

            return CreatedAtRoute("GetVariantById", new { productId, id = response.Id }, response);
        }

        // PUT: /api/products/{productId}/variants/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateVariantAsync(Guid productId, Guid id, [FromBody] UpdateProductVariantDto dto)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);

            if (variant == null || variant.ProductId != productId)
                return NotFound();

            variant.Color = dto.Color;
            variant.Size = dto.Size;
            variant.Barcode = dto.Barcode;
            variant.UpdatedAt = DateTime.UtcNow;

            await _variantRepository.UpdateVariantAsync(variant);
            return NoContent();
        }

        // DELETE: /api/products/{productId}/variants/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteVariantAsync(Guid productId, Guid id)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(id);

            if (variant == null || variant.ProductId != productId)
                return NotFound();

            await _variantRepository.DeleteVariantAsync(variant);
            return NoContent();
        }
    }
}
