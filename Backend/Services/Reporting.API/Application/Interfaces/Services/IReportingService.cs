using System.Threading.Tasks;
using System.Collections.Generic;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;

namespace Reporting.API.Application.Interfaces.Services
{
    public interface IReportingService
    {
        Task<DashboardSnapshot?> GetDashboardKpiAsync();
        Task<PagedResult<ProductReport>> GetProductReportsAsync(ProductReportFilterDto filter);
        Task<IEnumerable<DashboardSnapshot>> GetSnapshotsAsync();
        Task<byte[]> ExportProductsToCsvAsync();
    }
}
