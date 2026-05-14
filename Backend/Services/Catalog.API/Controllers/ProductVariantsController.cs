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
    /// <summary>
    /// Manages purchasable variants that belong to a product.
    /// </summary>
    [ApiController]
    [Route("api/products/{productId}/variants")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantService _service;
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Creates the product variants controller with variant services and audit publishing.
        /// </summary>
        /// <param name="service">Service that handles product variant rules and persistence work.</param>
        /// <param name="publishEndpoint">Message publisher used to send audit events.</param>
        public ProductVariantsController(IProductVariantService service, IPublishEndpoint publishEndpoint)
        {
            _service = service;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Gets all variants attached to a product.
        /// </summary>
        /// <param name="productId">Product identifier whose variants should be returned.</param>
        /// <returns>The variants for the product.</returns>
        /// <response code="200">Variants were returned successfully.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVariantResponseDto>>> GetVariantsByProductAsync(Guid productId)
        {
            var response = await _service.GetVariantsByProductAsync(productId);
            return Ok(response);
        }

        /// <summary>
        /// Gets one variant that belongs to the requested product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the variant.</param>
        /// <param name="id">Variant identifier to load.</param>
        /// <returns>The requested variant, or not found when it does not exist for the product.</returns>
        /// <response code="200">Variant was found and returned.</response>
        /// <response code="404">No variant exists for the supplied product and variant identifiers.</response>
        [HttpGet("{id}", Name = "GetVariantById")]
        public async Task<ActionResult<ProductVariantResponseDto>> GetVariantByIdAsync(Guid productId, Guid id)
        {
            var response = await _service.GetVariantByIdAsync(productId, id);
            if (response == null) return NotFound();
            return Ok(response);
        }

        /// <summary>
        /// Creates a variant under a product and records the change for audit history.
        /// </summary>
        /// <param name="productId">Product identifier that will own the variant.</param>
        /// <param name="dto">Variant details supplied by the catalog team.</param>
        /// <returns>The created variant.</returns>
        /// <response code="201">Variant was created successfully.</response>
        /// <response code="400">Variant details failed validation or referenced an invalid product.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to create variants.</response>
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

        /// <summary>
        /// Updates a variant that belongs to a product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the variant.</param>
        /// <param name="id">Variant identifier to update.</param>
        /// <param name="dto">New variant values to save.</param>
        /// <returns>No content when the variant has been updated.</returns>
        /// <response code="204">Variant was updated successfully.</response>
        /// <response code="400">Variant details failed validation.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to update variants.</response>
        /// <response code="404">No variant exists for the supplied product and variant identifiers.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateVariantAsync(Guid productId, Guid id, [FromBody] UpdateProductVariantDto dto)
        {
            await _service.UpdateVariantAsync(productId, id, dto);
            return NoContent();
        }

        /// <summary>
        /// Deletes a variant from a product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the variant.</param>
        /// <param name="id">Variant identifier to delete.</param>
        /// <returns>No content when the variant has been removed.</returns>
        /// <response code="204">Variant was deleted successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to delete variants.</response>
        /// <response code="404">No variant exists for the supplied product and variant identifiers.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteVariantAsync(Guid productId, Guid id)
        {
            await _service.DeleteVariantAsync(productId, id);
            return NoContent();
        }

        /// <summary>
        /// Reads the authenticated user's identifier from the current claims.
        /// </summary>
        /// <returns>The user identifier, or null when the claim is missing or invalid.</returns>
        private Guid? GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        /// <summary>
        /// Publishes an audit event for variant changes made through this API.
        /// </summary>
        /// <param name="productId">Product identifier connected to the variant change.</param>
        /// <param name="action">Action that was performed.</param>
        /// <param name="byUserId">Identifier of the user who performed the action.</param>
        /// <param name="oldValues">Serialized values before the change, when available.</param>
        /// <param name="newValues">Serialized values after the change, when available.</param>
        /// <returns>A task that completes when the audit event has been published.</returns>
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
