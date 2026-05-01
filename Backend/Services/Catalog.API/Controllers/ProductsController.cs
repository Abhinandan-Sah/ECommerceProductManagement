using Catalog.API.Application.DTOs.Product;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Enums;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging;
using System.Security.Claims;
using System.Text.Json;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly IPublishEndpoint _publishEndpoint;

        public ProductsController(IProductService service, IPublishEndpoint publishEndpoint)
        {
            _service = service;
            _publishEndpoint = publishEndpoint;
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
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var response = await _service.AddProductAsync(newProductDto);
            await PublishAuditLogAsync(
                "Product",
                response.Id,
                "Created",
                userId.Value,
                null,
                JsonSerializer.Serialize(new
                {
                    response.Id,
                    response.Name,
                    response.SKU,
                    response.Brand,
                    response.Description,
                    response.CategoryId,
                    response.CategoryName,
                    PublishStatus = response.PublishStatus.ToString()
                }));

            return CreatedAtRoute("GetProductById", new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateProductAsync(Guid id, [FromBody] UpdateProductDto updateDto)
        {
            var callerRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            await _service.UpdateProductAsync(id, updateDto, callerRole);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult> DeleteProductAsync(Guid id)
        {
            await _service.DeleteProductAsync(id);
            return NoContent();
        }

        private Guid? GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        private Task PublishAuditLogAsync(
            string entityName,
            Guid entityId,
            string action,
            Guid byUserId,
            string? oldValues,
            string? newValues)
        {
            return _publishEndpoint.Publish(new AuditLogCreatedEvent
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                ByUserId = byUserId,
                OldValues = oldValues,
                NewValues = newValues
            });
        }
    }
}
