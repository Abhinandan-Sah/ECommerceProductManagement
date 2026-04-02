using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces
{
    public interface IMediaAssetRepository
    {
        Task<IEnumerable<MediaAsset>> GetMediaByProductIdAsync(Guid productId);
        Task<MediaAsset?> GetMediaByIdAsync(Guid id);
        Task<MediaAsset> AddMediaAsync(MediaAsset media);
        Task DeleteMediaAsync(MediaAsset media);
    }
}
