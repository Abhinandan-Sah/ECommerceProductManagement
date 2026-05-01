using Catalog.API.Application.DTOs.ProductVariant;
using Catalog.API.Application.Interfaces.Services;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging;
using System.Security.Claims;
using System.Text.Json;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/variants")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantService _service;
        private readonly IPublishEndpoint _publishEndpoint;

        public ProductVariantsController(IProductVariantService service, IPublishEndpoint publishEndpoint)
        {
            _service = service;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVariantResponseDto>>> GetVariantsByProductAsync(Guid productId)
        {
            var response = await _service.GetVariantsByProductAsync(productId);
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
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var response = await _service.AddVariantAsync(productId, dto);
            await PublishAuditLogAsync(
                productId,
                "Created",
                userId.Value,
                null,
                JsonSerializer.Serialize(new
                {
                    VariantId = response.Id,
                    response.ProductId,
                    response.Color,
                    response.Size,
                    response.Barcode
                }));

            return CreatedAtRoute("GetVariantById", new { productId, id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateVariantAsync(Guid productId, Guid id, [FromBody] UpdateProductVariantDto dto)
        {
            await _service.UpdateVariantAsync(productId, id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteVariantAsync(Guid productId, Guid id)
        {
            await _service.DeleteVariantAsync(productId, id);
            return NoContent();
        }

        private Guid? GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        private Task PublishAuditLogAsync(Guid productId, string action, Guid byUserId, string? oldValues, string? newValues)
        {
            return _publishEndpoint.Publish(new AuditLogCreatedEvent
            {
                EntityName = "ProductVariant",
                EntityId = productId,
                Action = action,
                ByUserId = byUserId,
                OldValues = oldValues,
                NewValues = newValues
            });
        }
    }
}
