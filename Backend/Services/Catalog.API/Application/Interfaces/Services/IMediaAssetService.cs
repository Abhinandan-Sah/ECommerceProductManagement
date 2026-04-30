using Catalog.API.Application.DTOs;
using Catalog.API.Application.DTOs.MediaAsset;

namespace Catalog.API.Application.Interfaces.Services
{
    public interface IMediaAssetService
    {
        Task<IEnumerable<MediaAssetResponseDto>> GetMediaByProductAsync(Guid productId, string? role);
        Task<MediaAssetResponseDto?> GetMediaByIdAsync(Guid productId, Guid id, string? role);
        Task<MediaAssetResponseDto> AddMediaAsync(Guid productId, CreateMediaAssetDto dto);
        Task DeleteMediaAsync(Guid productId, Guid id);
    }
}
