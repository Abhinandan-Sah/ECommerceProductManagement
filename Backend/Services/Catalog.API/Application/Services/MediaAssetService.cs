using Catalog.API.Application.DTOs;
using Catalog.API.Application.DTOs.MediaAsset;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Exceptions;

namespace Catalog.API.Application.Services
{
    public class MediaAssetService : IMediaAssetService
    {
        private readonly IMediaAssetRepository _mediaRepository;
        private readonly ILogger<MediaAssetService> _logger;

        public MediaAssetService(IMediaAssetRepository mediaRepository, ILogger<MediaAssetService> logger)
        {
            _mediaRepository = mediaRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<MediaAssetResponseDto>> GetMediaByProductAsync(Guid productId)
        {
            _logger.LogInformation("Fetching media assets for product {ProductId}", productId);

            var media = await _mediaRepository.GetMediaByProductIdAsync(productId);
            return media.Select(m => new MediaAssetResponseDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                Url = m.Url,
                SortOrder = m.SortOrder,
                AltText = m.AltText
            });
        }

        public async Task<MediaAssetResponseDto?> GetMediaByIdAsync(Guid productId, Guid id)
        {
            _logger.LogInformation("Fetching media asset {MediaId} for product {ProductId}", id, productId);

            var media = await _mediaRepository.GetMediaByIdAsync(id);
            if (media == null || media.ProductId != productId) return null;

            return new MediaAssetResponseDto
            {
                Id = media.Id,
                ProductId = media.ProductId,
                Url = media.Url,
                SortOrder = media.SortOrder,
                AltText = media.AltText
            };
        }

        public async Task<MediaAssetResponseDto> AddMediaAsync(Guid productId, CreateMediaAssetDto dto)
        {
            _logger.LogInformation("Adding media asset for product {ProductId}", productId);

            var mediaEntity = new MediaAsset
            {
                ProductId = productId,
                Url = dto.Url,
                SortOrder = dto.SortOrder,
                AltText = dto.AltText
            };

            var saved = await _mediaRepository.AddMediaAsync(mediaEntity);

            _logger.LogInformation("Media asset {MediaId} created for product {ProductId}", saved.Id, productId);

            return new MediaAssetResponseDto
            {
                Id = saved.Id,
                ProductId = saved.ProductId,
                Url = saved.Url,
                SortOrder = saved.SortOrder,
                AltText = saved.AltText
            };
        }

        public async Task DeleteMediaAsync(Guid productId, Guid id)
        {
            _logger.LogInformation("Deleting media asset {MediaId} for product {ProductId}", id, productId);

            var media = await _mediaRepository.GetMediaByIdAsync(id);
            if (media == null || media.ProductId != productId) throw new NotFoundException("MediaAsset", id);

            await _mediaRepository.DeleteMediaAsync(media);

            _logger.LogInformation("Media asset {MediaId} deleted successfully", id);
        }
    }
}
