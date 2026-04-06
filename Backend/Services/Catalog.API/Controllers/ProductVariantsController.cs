using Catalog.API.Application.DTOs.ProductVariant;
using Catalog.API.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/variants")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantService _service;

        public ProductVariantsController(IProductVariantService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVariantResponseDto>>> GetVariantsByProductAsync(Guid productId)
        {
            var response = await _service.GetVariantsByProductAsync(productId);
            // Since our old implementation checked if the product existed first inside the controller,
            // we should probably do a check or return empty if no variants, but old one returned 404 if product not found.
            // Our Service doesn't check for Product existence on GET all variants to avoid an extra DB call, it just returns empty list if no variants.
            return Ok(response);
        }

        [HttpGet("{id}", Name = "GetVariantById")]
        public async Task<ActionResult<ProductVariantResponseDto>> GetVariantByIdAsync(Guid productId, Guid id)
        {
            var response = await _service.GetVariantByIdAsync(productId, id);
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<ActionResult<ProductVariantResponseDto>> AddVariantAsync(Guid productId, [FromBody] CreateProductVariantDto dto)
        {
            var response = await _service.AddVariantAsync(productId, dto);
            return CreatedAtRoute("GetVariantById", new { productId, id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateVariantAsync(Guid productId, Guid id, [FromBody] UpdateProductVariantDto dto)
        {
            try
            {
                await _service.UpdateVariantAsync(productId, id, dto);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteVariantAsync(Guid productId, Guid id)
        {
            try
            {
                await _service.DeleteVariantAsync(productId, id);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
}
