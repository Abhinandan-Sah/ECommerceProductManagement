using System.Threading.Tasks;
using System.Collections.Generic;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;

namespace Reporting.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines persistence operations for reporting projections and dashboard snapshots.
    /// </summary>
    public interface IReportingRepository
    {
        /// <summary>
        /// Reads the most recent dashboard snapshot.
        /// </summary>
        /// <returns>The latest dashboard snapshot, or null when none exist.</returns>
        Task<DashboardSnapshot?> GetLatestDashboardSnapshotAsync();

        /// <summary>
        /// Reads paged product report rows using the supplied filter.
        /// </summary>
        /// <param name="filter">Filtering and paging options.</param>
        /// <returns>Paged product report data.</returns>
        Task<PagedResult<ProductReport>> GetProductReportsAsync(ProductReportFilterDto filter);

        /// <summary>
        /// Reads all product report rows for export operations.
        /// </summary>
        /// <returns>Product report rows ordered by most recent first.</returns>
        Task<IEnumerable<ProductReport>> GetAllProductReportsAsync();

        /// <summary>
        /// Reads historical dashboard snapshots.
        /// </summary>
        /// <returns>Dashboard snapshots ordered by snapshot date.</returns>
        Task<IEnumerable<DashboardSnapshot>> GetHistoricalSnapshotsAsync();

        /// <summary>
        /// Creates or updates the reporting read model for a catalog product.
        /// </summary>
        /// <param name="report">Product report values projected from catalog events.</param>
        Task UpsertProductReportAsync(ProductReport report);

        /// <summary>
        /// Updates the status of an existing product report row.
        /// </summary>
        /// <param name="productId">Catalog product identifier to update.</param>
        /// <param name="status">New workflow status.</param>
        Task UpdateProductStatusAsync(Guid productId, string status);

        /// <summary>
        /// Deletes the reporting read model row for a catalog product.
        /// </summary>
        /// <param name="productId">Catalog product identifier to remove.</param>
        Task DeleteProductReportAsync(Guid productId);

        /// <summary>
        /// Recalculates dashboard KPI values and appends a new snapshot.
        /// </summary>
        /// <param name="userDelta">Change in total user count supplied by identity events.</param>
        Task RefreshDashboardSnapshotAsync(int userDelta = 0);
    }
}
