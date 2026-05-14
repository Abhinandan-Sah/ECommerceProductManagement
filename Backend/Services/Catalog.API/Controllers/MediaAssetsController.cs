using Catalog.API.Application.DTOs;
using Catalog.API.Application.DTOs.MediaAsset;
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
    /// Manages product media assets such as images and supporting catalog visuals.
    /// </summary>
    [ApiController]
    [Route("api/products/{productId}/media")]
    public class MediaAssetsController : ControllerBase
    {
        private readonly IMediaAssetService _service;
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Creates the media assets controller with media services and audit publishing.
        /// </summary>
        /// <param name="service">Service that handles media asset rules and persistence work.</param>
        /// <param name="publishEndpoint">Message publisher used to send audit events.</param>
        public MediaAssetsController(IMediaAssetService service, IPublishEndpoint publishEndpoint)
        {
            _service = service;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Gets media assets for a product, filtered by what the caller is allowed to see.
        /// </summary>
        /// <param name="productId">Product identifier whose media should be returned.</param>
        /// <returns>The media assets visible to the caller.</returns>
        /// <response code="200">Media assets were returned successfully.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaAssetResponseDto>>> GetMediaByProductAsync(Guid productId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var response = await _service.GetMediaByProductAsync(productId, role);
            return Ok(response);
        }

        /// <summary>
        /// Gets one media asset that belongs to a product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the media asset.</param>
        /// <param name="id">Media asset identifier to load.</param>
        /// <returns>The requested media asset, or not found when it does not exist or is hidden.</returns>
        /// <response code="200">Media asset was found and returned.</response>
        /// <response code="404">No visible media asset exists for the supplied identifiers.</response>
        [HttpGet("{id}", Name = "GetMediaById")]
        public async Task<ActionResult<MediaAssetResponseDto>> GetMediaByIdAsync(Guid productId, Guid id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var response = await _service.GetMediaByIdAsync(productId, id, role);
            if (response == null) return NotFound();
            return Ok(response);
        }

        /// <summary>
        /// Adds a media asset to a product and records the change for audit history.
        /// </summary>
        /// <param name="productId">Product identifier that will own the media asset.</param>
        /// <param name="dto">Media details supplied by the catalog team.</param>
        /// <returns>The created media asset.</returns>
        /// <response code="201">Media asset was created successfully.</response>
        /// <response code="400">Media details failed validation.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to add media assets.</response>
        [HttpPost]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<ActionResult<MediaAssetResponseDto>> AddMediaAsync(Guid productId, [FromBody] CreateMediaAssetDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var response = await _service.AddMediaAsync(productId, dto);
            await PublishAuditLogAsync(
                productId,
                "Created",
                userId.Value,
                null,
                JsonSerializer.Serialize(new
                {
                    MediaAssetId = response.Id,
                    response.ProductId,
                    response.Url,
                    response.SortOrder,
                    response.AltText
                }));

            return CreatedAtRoute("GetMediaById", new { productId, id = response.Id }, response);
        }

        /// <summary>
        /// Deletes a media asset from a product.
        /// </summary>
        /// <param name="productId">Product identifier that owns the media asset.</param>
        /// <param name="id">Media asset identifier to delete.</param>
        /// <returns>No content when the media asset has been removed.</returns>
        /// <response code="204">Media asset was deleted successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to delete media assets.</response>
        /// <response code="404">No media asset exists for the supplied identifiers.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<ActionResult> DeleteMediaAsync(Guid productId, Guid id)
        {
            await _service.DeleteMediaAsync(productId, id);
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
        /// Publishes an audit event for media changes made through this API.
        /// </summary>
        /// <param name="productId">Product identifier connected to the media change.</param>
        /// <param name="action">Action that was performed.</param>
        /// <param name="byUserId">Identifier of the user who performed the action.</param>
        /// <param name="oldValues">Serialized values before the change, when available.</param>
        /// <param name="newValues">Serialized values after the change, when available.</param>
        /// <returns>A task that completes when the audit event has been published.</returns>
        private Task PublishAuditLogAsync(Guid productId, string action, Guid byUserId, string? oldValues, string? newValues)
        {
            return _publishEndpoint.Publish(new AuditLogCreatedEvent
            {
                EntityName = "MediaAsset",
                EntityId = productId,
                Action = action,
                ByUserId = byUserId,
                OldValues = oldValues,
                NewValues = newValues
            });
        }
    }
}
