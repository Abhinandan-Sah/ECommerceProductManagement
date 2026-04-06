using Catalog.API.Application.DTOs;
using Catalog.API.Application.DTOs.MediaAsset;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Services
{
    public class MediaAssetService : IMediaAssetService
    {
        private readonly IMediaAssetRepository _mediaRepository;

        public MediaAssetService(IMediaAssetRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        }

        public async Task<IEnumerable<MediaAssetResponseDto>> GetMediaByProductAsync(Guid productId)
        {
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
            var mediaEntity = new MediaAsset
            {
                ProductId = productId,
                Url = dto.Url,
                SortOrder = dto.SortOrder,
                AltText = dto.AltText
            };

            var saved = await _mediaRepository.AddMediaAsync(mediaEntity);

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
            var media = await _mediaRepository.GetMediaByIdAsync(id);
            if (media == null || media.ProductId != productId) throw new InvalidOperationException("Media not found.");

            await _mediaRepository.DeleteMediaAsync(media);
        }
    }
}
