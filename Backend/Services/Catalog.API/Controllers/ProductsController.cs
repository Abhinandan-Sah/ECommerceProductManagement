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
    /// <summary>
    /// Exposes product browsing and product-management endpoints for the catalog.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Creates the products controller with product services and audit publishing.
        /// </summary>
        /// <param name="service">Service that handles product rules and persistence work.</param>
        /// <param name="publishEndpoint">Message publisher used to send audit events.</param>
        public ProductsController(IProductService service, IPublishEndpoint publishEndpoint)
        {
            _service = service;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Checks whether the caller can see products that are not yet public.
        /// </summary>
        /// <returns>True when the caller works in a role that can view draft or review products.</returns>
        private bool CanViewUnpublishedProducts()
        {
            // These roles work before the public catalogue stage, so they can see drafts and review states.
            return User.IsInRole("Admin") || User.IsInRole("ProductManager") || User.IsInRole("ContentExecutive");
        }

        /// <summary>
        /// Gets products visible to the caller, optionally filtered by category and publish status.
        /// </summary>
        /// <param name="categoryId">Optional category identifier used to narrow the product list.</param>
        /// <param name="status">Optional publish status used to narrow the product list.</param>
        /// <returns>The products the caller is allowed to see.</returns>
        /// <response code="200">Products were returned successfully.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAllProductsAsync([FromQuery] Guid? categoryId, [FromQuery] PublishStatus? status)
        {
            var response = await _service.GetAllProductsAsync(categoryId, status, CanViewUnpublishedProducts());
            return Ok(response);
        }

        /// <summary>
        /// Gets one product when it exists and is visible to the caller.
        /// </summary>
        /// <param name="id">Product identifier to load.</param>
        /// <returns>The requested product, or not found when it is missing or hidden.</returns>
        /// <response code="200">Product was found and returned.</response>
        /// <response code="404">No visible product exists for the supplied identifier.</response>
        [HttpGet("{id}", Name = "GetProductById")]
        public async Task<ActionResult<ProductResponseDto?>> GetProductByIdAsync(Guid id)
        {
            var response = await _service.GetProductByIdAsync(id, CanViewUnpublishedProducts());
            if (response == null) return NotFound();
            return Ok(response);
        }

        /// <summary>
        /// Creates a product and records who created it for audit history.
        /// </summary>
        /// <param name="newProductDto">Product details supplied by the catalog team.</param>
        /// <returns>The created product with its generated identifier and SKU.</returns>
        /// <response code="201">Product was created successfully.</response>
        /// <response code="400">Product details failed validation or referenced an invalid category.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to create products.</response>
        [HttpPost]
        [Authorize(Roles ="Admin,ProductManager,ContentExecutive")]
        public async Task<ActionResult<ProductResponseDto>> AddProductAsync(CreateProductDto newProductDto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var response = await _service.AddProductAsync(newProductDto);
            // Product creation is audited from the API layer because this is where the authenticated user
            // is available; the service keeps the catalogue write itself focused on product rules.
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

        /// <summary>
        /// Updates product details while using the caller's role for catalog workflow rules.
        /// </summary>
        /// <param name="id">Product identifier to update.</param>
        /// <param name="updateDto">New product values to save.</param>
        /// <returns>No content when the product has been updated.</returns>
        /// <response code="204">Product was updated successfully.</response>
        /// <response code="400">Product details failed validation or referenced an invalid category.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to update the requested product state.</response>
        /// <response code="404">No product exists for the supplied identifier.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateProductAsync(Guid id, [FromBody] UpdateProductDto updateDto)
        {
            var callerRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            await _service.UpdateProductAsync(id, updateDto, callerRole);
            return NoContent();
        }

        /// <summary>
        /// Deletes a product from the catalog.
        /// </summary>
        /// <param name="id">Product identifier to delete.</param>
        /// <returns>No content when the product has been removed.</returns>
        /// <response code="204">Product was deleted successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to delete products.</response>
        /// <response code="404">No product exists for the supplied identifier.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult> DeleteProductAsync(Guid id)
        {
            await _service.DeleteProductAsync(id);
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
        /// Publishes an audit event for product changes made through this API.
        /// </summary>
        /// <param name="entityName">Name of the entity being audited.</param>
        /// <param name="entityId">Identifier of the entity being audited.</param>
        /// <param name="action">Action that was performed.</param>
        /// <param name="byUserId">Identifier of the user who performed the action.</param>
        /// <param name="oldValues">Serialized values before the change, when available.</param>
        /// <param name="newValues">Serialized values after the change, when available.</param>
        /// <returns>A task that completes when the audit event has been published.</returns>
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
