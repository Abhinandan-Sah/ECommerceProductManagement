using System.Threading.Tasks;
using System.Collections.Generic;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;

namespace Reporting.API.Application.Interfaces.Services
{
    /// <summary>
    /// Defines reporting operations exposed by the Reporting API.
    /// </summary>
    public interface IReportingService
    {
        /// <summary>
        /// Gets the latest dashboard KPI snapshot.
        /// </summary>
        /// <returns>The latest snapshot when available; otherwise null.</returns>
        Task<DashboardSnapshot?> GetDashboardKpiAsync();

        /// <summary>
        /// Gets paged product report rows using the supplied filter.
        /// </summary>
        /// <param name="filter">Filtering and paging options.</param>
        /// <returns>Paged product report data.</returns>
        Task<PagedResult<ProductReport>> GetProductReportsAsync(ProductReportFilterDto filter);

        /// <summary>
        /// Gets historical dashboard snapshots.
        /// </summary>
        /// <returns>Dashboard snapshots ordered by snapshot date.</returns>
        Task<IEnumerable<DashboardSnapshot>> GetSnapshotsAsync();

        /// <summary>
        /// Exports product reports as a CSV document.
        /// </summary>
        /// <returns>CSV file contents as bytes.</returns>
        Task<byte[]> ExportProductsToCsvAsync();
    }
}
