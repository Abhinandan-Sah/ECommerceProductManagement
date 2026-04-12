using Catalog.API.Application.DTOs.Product;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        private bool CanViewUnpublishedProducts()
        {
            return User.IsInRole("Admin") || User.IsInRole("ProductManager") || User.IsInRole("ContentExecutive");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAllProductsAsync([FromQuery] Guid? categoryId, [FromQuery] PublishStatus? status)
        {
            var response = await _service.GetAllProductsAsync(categoryId, status, CanViewUnpublishedProducts());
            return Ok(response);
        }

        [HttpGet("{id}", Name = "GetProductById")]
        public async Task<ActionResult<ProductResponseDto?>> GetProductByIdAsync(Guid id)
        {
            var response = await _service.GetProductByIdAsync(id, CanViewUnpublishedProducts());
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles ="Admin,ProductManager,ContentExecutive")]
        public async Task<ActionResult<ProductResponseDto>> AddProductAsync(CreateProductDto newProductDto)
        {
            var response = await _service.AddProductAsync(newProductDto);
            return CreatedAtRoute("GetProductById", new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateProductAsync(Guid id, [FromBody] UpdateProductDto updateDto)
        {
            await _service.UpdateProductAsync(id, updateDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult> DeleteProductAsync(Guid id)
        {
            await _service.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
