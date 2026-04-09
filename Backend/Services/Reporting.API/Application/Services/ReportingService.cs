using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Reporting.API.Application.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IReportingRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(IReportingRepository repository, IMemoryCache cache, ILogger<ReportingService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<DashboardSnapshot?> GetDashboardKpiAsync()
        {
            _logger.LogInformation("Fetching dashboard KPI snapshot");

            var cacheKey = "DashboardKpi";
            if (!_cache.TryGetValue(cacheKey, out DashboardSnapshot? snapshot))
            {
                snapshot = await _repository.GetLatestDashboardSnapshotAsync();
                if (snapshot != null)
                {
                    _cache.Set(cacheKey, snapshot, TimeSpan.FromMinutes(10));
                    _logger.LogInformation("Dashboard snapshot cached for 10 minutes");
                }
            }
            else
            {
                _logger.LogInformation("Dashboard snapshot served from cache");
            }
            return snapshot;
        }

        public async Task<PagedResult<ProductReport>> GetProductReportsAsync(ProductReportFilterDto filter)
        {
            _logger.LogInformation("Fetching product reports");

            return await _repository.GetProductReportsAsync(filter);
        }

        public async Task<IEnumerable<DashboardSnapshot>> GetSnapshotsAsync()
        {
            _logger.LogInformation("Fetching historical snapshots");

            return await _repository.GetHistoricalSnapshotsAsync();
        }

        public async Task<byte[]> ExportProductsToCsvAsync()
        {
            _logger.LogInformation("Exporting products to CSV");

            var products = await _repository.GetAllProductReportsAsync();
            var builder = new StringBuilder();
            builder.AppendLine("Id,ProductId,ProductName,SKU,Status,PublishedAt,CreatedByUserId,CategoryName,CreatedAt");

            foreach (var p in products)
            {
                // Simple escaping for basic string properties
                var name = p.ProductName?.Replace("\"", "\"\"") ?? "";
                if (name.Contains(',') || name.Contains('"')) name = $"\"{name}\"";

                var category = p.CategoryName?.Replace("\"", "\"\"") ?? "";
                if (category.Contains(',') || category.Contains('"')) category = $"\"{category}\"";

                builder.AppendLine($"{p.Id},{p.ProductId},{name},{p.SKU},{p.Status},{p.PublishedAt},{p.CreatedByUserId},{category},{p.CreatedAt}");
            }

            _logger.LogInformation("CSV export completed with {Count} records", products.Count());
            return Encoding.UTF8.GetBytes(builder.ToString());
        }
    }
}
