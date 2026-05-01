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
    [ApiController]
    [Route("api/products/{productId}/media")]
    public class MediaAssetsController : ControllerBase
    {
        private readonly IMediaAssetService _service;
        private readonly IPublishEndpoint _publishEndpoint;

        public MediaAssetsController(IMediaAssetService service, IPublishEndpoint publishEndpoint)
        {
            _service = service;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaAssetResponseDto>>> GetMediaByProductAsync(Guid productId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var response = await _service.GetMediaByProductAsync(productId, role);
            return Ok(response);
        }

        [HttpGet("{id}", Name = "GetMediaById")]
        public async Task<ActionResult<MediaAssetResponseDto>> GetMediaByIdAsync(Guid productId, Guid id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var response = await _service.GetMediaByIdAsync(productId, id, role);
            if (response == null) return NotFound();
            return Ok(response);
        }

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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<ActionResult> DeleteMediaAsync(Guid productId, Guid id)
        {
            await _service.DeleteMediaAsync(productId, id);
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
