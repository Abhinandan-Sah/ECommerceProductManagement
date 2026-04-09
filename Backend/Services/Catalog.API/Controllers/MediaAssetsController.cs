using Catalog.API.Application.DTOs;
using Catalog.API.Application.DTOs.MediaAsset;
using Catalog.API.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/media")]
    public class MediaAssetsController : ControllerBase
    {
        private readonly IMediaAssetService _service;

        public MediaAssetsController(IMediaAssetService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaAssetResponseDto>>> GetMediaByProductAsync(Guid productId)
        {
            var response = await _service.GetMediaByProductAsync(productId);
            return Ok(response);
        }

        [HttpGet("{id}", Name = "GetMediaById")]
        public async Task<ActionResult<MediaAssetResponseDto>> GetMediaByIdAsync(Guid productId, Guid id)
        {
            var response = await _service.GetMediaByIdAsync(productId, id);
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ContentExecutive")]
        public async Task<ActionResult<MediaAssetResponseDto>> AddMediaAsync(Guid productId, [FromBody] CreateMediaAssetDto dto)
        {
            var response = await _service.AddMediaAsync(productId, dto);
            return CreatedAtRoute("GetMediaById", new { productId, id = response.Id }, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ContentExecutive")]
        public async Task<ActionResult> DeleteMediaAsync(Guid productId, Guid id)
        {
            await _service.DeleteMediaAsync(productId, id);
            return NoContent();
        }
    }
}