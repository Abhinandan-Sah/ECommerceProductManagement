using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Infrastructure.Data;

namespace Reporting.API.Infrastructure.Repositories
{
    public class ReportingRepository : IReportingRepository
    {
        private readonly ReportingDbContext _context;

        public ReportingRepository(ReportingDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSnapshot?> GetLatestDashboardSnapshotAsync()
        {
            return await _context.DashboardSnapshots
                .OrderByDescending(s => s.SnapshotDate)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<ProductReport>> GetProductReportsAsync(ProductReportFilterDto filter)
        {
            var query = _context.ProductReports.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(p => p.CategoryName == filter.Category);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(p => p.Status == filter.Status);

            int totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<ProductReport>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<IEnumerable<ProductReport>> GetAllProductReportsAsync()
        {
            return await _context.ProductReports.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<DashboardSnapshot>> GetHistoricalSnapshotsAsync()
        {
            return await _context.DashboardSnapshots.OrderByDescending(s => s.SnapshotDate).ToListAsync();
        }
    }
}
