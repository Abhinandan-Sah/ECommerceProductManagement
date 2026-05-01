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

        public async Task UpsertProductReportAsync(ProductReport report)
        {
            var existing = await _context.ProductReports.FirstOrDefaultAsync(p => p.ProductId == report.ProductId);

            if (existing == null)
            {
                await _context.ProductReports.AddAsync(report);
            }
            else
            {
                existing.ProductName = report.ProductName;
                existing.SKU = report.SKU;
                existing.Status = report.Status;
                existing.CategoryName = report.CategoryName;
                existing.PublishedAt = report.PublishedAt;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductReportAsync(Guid productId)
        {
            var existing = await _context.ProductReports.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (existing == null) return;

            _context.ProductReports.Remove(existing);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductStatusAsync(Guid productId, string status)
        {
            var existing = await _context.ProductReports.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (existing == null) return;

            existing.Status = status;
            existing.PublishedAt = status == "Published" ? DateTime.UtcNow : existing.PublishedAt;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task RefreshDashboardSnapshotAsync(int userDelta = 0)
        {
            var latest = await GetLatestDashboardSnapshotAsync();
            var products = await _context.ProductReports.ToListAsync();

            var snapshot = new DashboardSnapshot
            {
                SnapshotDate = DateTime.UtcNow,
                TotalProducts = products.Count,
                PublishedProducts = products.Count(p => p.Status == "Published"),
                PendingApprovals = products.Count(p => p.Status == "ReadyForReview"),
                RejectedProducts = products.Count(p => p.Status == "Rejected"),
                TotalUsers = Math.Max(0, (latest?.TotalUsers ?? 0) + userDelta)
            };

            await _context.DashboardSnapshots.AddAsync(snapshot);
            await _context.SaveChangesAsync();
        }
    }
}
