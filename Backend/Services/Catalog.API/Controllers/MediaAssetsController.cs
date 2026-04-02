using Catalog.API.Application.DTOs;
using Catalog.API.Application.DTOs.MediaAsset;
using Catalog.API.Application.Interfaces;
using Catalog.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/media")]
    public class MediaAssetsController : ControllerBase
    {
        private readonly IMediaAssetRepository _mediaRepository;
        private readonly IProductRepository _productRepository;

        public MediaAssetsController(
            IMediaAssetRepository mediaRepository,
            IProductRepository productRepository)
        {
            _mediaRepository = mediaRepository;
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaAssetResponseDto>>> GetMediaByProductAsync(Guid productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return NotFound($"Product {productId} not found.");

            var media = await _mediaRepository.GetMediaByProductIdAsync(productId);

            var response = media.Select(m => new MediaAssetResponseDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                Url = m.Url,
                SortOrder = m.SortOrder,
                AltText = m.AltText
            });

            return Ok(response);
        }

        [HttpGet("{id}", Name = "GetMediaById")]
        public async Task<ActionResult<MediaAssetResponseDto>> GetMediaByIdAsync(Guid productId, Guid id)
        {
            var media = await _mediaRepository.GetMediaByIdAsync(id);

            if (media == null || media.ProductId != productId) return NotFound();

            var response = new MediaAssetResponseDto
            {
                Id = media.Id,
                ProductId = media.ProductId,
                Url = media.Url,
                SortOrder = media.SortOrder,
                AltText = media.AltText
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ContentExecutive")]
        public async Task<ActionResult<MediaAssetResponseDto>> AddMediaAsync(Guid productId, [FromBody] CreateMediaAssetDto dto)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return NotFound($"Product {productId} not found.");

            var mediaEntity = new MediaAsset
            {
                ProductId = productId,
                Url = dto.Url,
                SortOrder = dto.SortOrder,
                AltText = dto.AltText
            };

            var saved = await _mediaRepository.AddMediaAsync(mediaEntity);

            var response = new MediaAssetResponseDto
            {
                Id = saved.Id,
                ProductId = saved.ProductId,
                Url = saved.Url,
                SortOrder = saved.SortOrder,
                AltText = saved.AltText
            };

            return CreatedAtRoute("GetMediaById", new { productId, id = response.Id }, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ContentExecutive")]
        public async Task<ActionResult> DeleteMediaAsync(Guid productId, Guid id)
        {
            var media = await _mediaRepository.GetMediaByIdAsync(id);

            if (media == null || media.ProductId != productId) return NotFound();

            await _mediaRepository.DeleteMediaAsync(media);
            return NoContent();
        }
    }
}