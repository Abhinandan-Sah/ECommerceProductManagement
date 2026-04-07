using System;
using System.Threading.Tasks;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;

namespace Reporting.API.Application.Interfaces.Repositories
{
    public interface IAuditRepository
    {
        Task<PagedResult<AuditLog>> GetAllAuditLogsAsync(int pageNumber, int pageSize);
        Task<PagedResult<AuditLog>> GetProductAuditLogsAsync(Guid productId, int pageNumber, int pageSize);
        Task<PagedResult<AuditLog>> GetUserAuditLogsAsync(Guid userId, int pageNumber, int pageSize);
        Task AddAuditLogAsync(AuditLog log);
    }
}
