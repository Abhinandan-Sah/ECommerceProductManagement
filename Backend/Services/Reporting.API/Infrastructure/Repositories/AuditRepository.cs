using System;
using System.Threading.Tasks;
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
    /// Reads and writes audit log records from the reporting database.
    /// </summary>
    public class AuditRepository : IAuditRepository
    {
        private readonly ReportingDbContext _context;

        /// <summary>
        /// Creates the audit repository for the current reporting database context.
        /// </summary>
        public AuditRepository(ReportingDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<PagedResult<AuditLog>> GetAllAuditLogsAsync(int pageNumber, int pageSize)
        {
            var query = _context.AuditLogs.AsQueryable();
            int totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AuditLog> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        /// <inheritdoc />
        public async Task<PagedResult<AuditLog>> GetProductAuditLogsAsync(Guid productId, int pageNumber, int pageSize)
        {
            var query = _context.AuditLogs.Where(a => a.EntityId == productId);
            
            int totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AuditLog> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        /// <inheritdoc />
        public async Task<PagedResult<AuditLog>> GetUserAuditLogsAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _context.AuditLogs.Where(a => a.ByUserId == userId);
            
            int totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AuditLog> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        /// <inheritdoc />
        public async Task AddAuditLogAsync(AuditLog log)
        {
            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
