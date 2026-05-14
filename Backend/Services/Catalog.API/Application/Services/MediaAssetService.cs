using Catalog.API.Application.DTOs;
using Catalog.API.Application.DTOs.MediaAsset;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Domain.Exceptions;

namespace Catalog.API.Application.Services
{
    /// <summary>
    /// Applies media visibility rules and shapes product media data for API responses.
    /// </summary>
    public class MediaAssetService : IMediaAssetService
    {
        private readonly IMediaAssetRepository _mediaRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<MediaAssetService> _logger;

        /// <summary>
        /// Creates the media asset service with media, product, and logging dependencies.
        /// </summary>
        public MediaAssetService(
            IMediaAssetRepository mediaRepository,
            IProductRepository productRepository,
            ILogger<MediaAssetService> logger)
        {
            _mediaRepository = mediaRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MediaAssetResponseDto>> GetMediaByProductAsync(Guid productId, string? role)
        {
            _logger.LogInformation("Fetching media assets for product {ProductId}", productId);

            // Media follows the same visibility rule as products. A draft image should not leak before
            // the product itself is ready for customers.
            var canViewUnpublished = CanViewUnpublishedProducts(role);

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return Enumerable.Empty<MediaAssetResponseDto>();

            if (!canViewUnpublished && product.PublishStatus != PublishStatus.Approved && product.PublishStatus != PublishStatus.Published)
                return Enumerable.Empty<MediaAssetResponseDto>();

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

        /// <inheritdoc />
        public async Task<MediaAssetResponseDto?> GetMediaByIdAsync(Guid productId, Guid id, string? role)
        {
            _logger.LogInformation("Fetching media asset {MediaId} for product {ProductId}", id, productId);

            // Checking the product first prevents direct media URLs from exposing unpublished catalogue items.
            var canViewUnpublished = CanViewUnpublishedProducts(role);

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return null;

            if (!canViewUnpublished && product.PublishStatus != PublishStatus.Approved && product.PublishStatus != PublishStatus.Published)
                return null;

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

        /// <summary>
        /// Checks whether a role can see media attached to unpublished products.
        /// </summary>
        private static bool CanViewUnpublishedProducts(string? role)
        {
            // Keep this list close to the media visibility checks so role changes are easy to audit.
            if (string.IsNullOrWhiteSpace(role)) return false;

            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || role.Equals("ProductManager", StringComparison.OrdinalIgnoreCase)
                || role.Equals("ContentExecutive", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
