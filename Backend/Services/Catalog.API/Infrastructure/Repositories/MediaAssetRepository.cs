using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Repositories
{
    /// <summary>
    /// Reads and writes product media asset records from the catalog database.
    /// </summary>
    public class MediaAssetRepository : IMediaAssetRepository
    {
        private readonly CatalogDbContext _context;

        /// <summary>
        /// Creates the media asset repository for the current catalog database context.
        /// </summary>
        public MediaAssetRepository(CatalogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <remarks>Orders media by <see cref="MediaAsset.SortOrder"/> so callers receive gallery-ready results.</remarks>
        public async Task<IEnumerable<MediaAsset>> GetMediaByProductIdAsync(Guid productId)
        {
            return await _context.MediaAssets
                .Where(m => m.ProductId == productId)
                .OrderBy(m => m.SortOrder) // Keep images in the correct order
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<MediaAsset?> GetMediaByIdAsync(Guid id)
        {
            return await _context.MediaAssets.FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <inheritdoc />
        public async Task<MediaAsset> AddMediaAsync(MediaAsset media)
        {
            await _context.MediaAssets.AddAsync(media);
            await _context.SaveChangesAsync();
            return media;
        }

        /// <inheritdoc />
        public async Task DeleteMediaAsync(MediaAsset media)
        {
            _context.MediaAssets.Remove(media);
            await _context.SaveChangesAsync();
        }
    }
}
