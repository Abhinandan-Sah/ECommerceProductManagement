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
    /// <summary>
    /// Reads and writes reporting projections and dashboard snapshots from the reporting database.
    /// </summary>
    public class ReportingRepository : IReportingRepository
    {
        private readonly ReportingDbContext _context;

        /// <summary>
        /// Creates the reporting repository for the current reporting database context.
        /// </summary>
        public ReportingRepository(ReportingDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<DashboardSnapshot?> GetLatestDashboardSnapshotAsync()
        {
            return await _context.DashboardSnapshots
                .OrderByDescending(s => s.SnapshotDate)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        /// <remarks>Builds the EF Core query incrementally so only requested filters are sent to SQL.</remarks>
        public async Task<PagedResult<ProductReport>> GetProductReportsAsync(ProductReportFilterDto filter)
        {
            // Start with IQueryable so filters, count, and paging are all executed by SQL Server.
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

        /// <inheritdoc />
        public async Task<IEnumerable<ProductReport>> GetAllProductReportsAsync()
        {
            return await _context.ProductReports.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DashboardSnapshot>> GetHistoricalSnapshotsAsync()
        {
            return await _context.DashboardSnapshots.OrderByDescending(s => s.SnapshotDate).ToListAsync();
        }

        /// <inheritdoc />
        /// <remarks>Upserts make product projection handling idempotent for redelivered messages.</remarks>
        public async Task UpsertProductReportAsync(ProductReport report)
        {
            var existing = await _context.ProductReports.FirstOrDefaultAsync(p => p.ProductId == report.ProductId);

            if (existing == null)
            {
                // First event for a product creates the Reporting read model.
                await _context.ProductReports.AddAsync(report);
            }
            else
            {
                // Later events refresh the read model in place so report URLs remain stable.
                existing.ProductName = report.ProductName;
                existing.SKU = report.SKU;
                existing.Status = report.Status;
                existing.CategoryName = report.CategoryName;
                existing.PublishedAt = report.PublishedAt;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteProductReportAsync(Guid productId)
        {
            var existing = await _context.ProductReports.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (existing == null) return;

            _context.ProductReports.Remove(existing);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UpdateProductStatusAsync(Guid productId, string status)
        {
            var existing = await _context.ProductReports.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (existing == null) return;

            existing.Status = status;
            // PublishedAt should reflect the first time a product becomes visible to customers.
            existing.PublishedAt = status == "Published" ? DateTime.UtcNow : existing.PublishedAt;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        /// <remarks>Dashboard snapshots are appended to preserve simple trend history.</remarks>
        public async Task RefreshDashboardSnapshotAsync(int userDelta = 0)
        {
            var latest = await GetLatestDashboardSnapshotAsync();
            var products = await _context.ProductReports.ToListAsync();

            // Keep snapshots append-only; it gives Reporting a simple trend history without a separate job.
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
