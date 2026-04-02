using Catalog.API.Application.Interfaces;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Repositories
{
    public class MediaAssetRepository : IMediaAssetRepository
    {
        private readonly CatalogDbContext _context;

        public MediaAssetRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MediaAsset>> GetMediaByProductIdAsync(Guid productId)
        {
            return await _context.MediaAssets
                .Where(m => m.ProductId == productId)
                .OrderBy(m => m.SortOrder) // Keep images in the correct order
                .ToListAsync();
        }

        public async Task<MediaAsset?> GetMediaByIdAsync(Guid id)
        {
            return await _context.MediaAssets.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MediaAsset> AddMediaAsync(MediaAsset media)
        {
            await _context.MediaAssets.AddAsync(media);
            await _context.SaveChangesAsync();
            return media;
        }

        public async Task DeleteMediaAsync(MediaAsset media)
        {
            _context.MediaAssets.Remove(media);
            await _context.SaveChangesAsync();
        }
    }
}