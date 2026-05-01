using System.Threading.Tasks;
using System.Collections.Generic;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;

namespace Reporting.API.Application.Interfaces.Repositories
{
    public interface IReportingRepository
    {
        Task<DashboardSnapshot?> GetLatestDashboardSnapshotAsync();
        Task<PagedResult<ProductReport>> GetProductReportsAsync(ProductReportFilterDto filter);
        Task<IEnumerable<ProductReport>> GetAllProductReportsAsync();
        Task<IEnumerable<DashboardSnapshot>> GetHistoricalSnapshotsAsync();
        Task UpsertProductReportAsync(ProductReport report);
        Task UpdateProductStatusAsync(Guid productId, string status);
        Task DeleteProductReportAsync(Guid productId);
        Task RefreshDashboardSnapshotAsync(int userDelta = 0);
    }
}
