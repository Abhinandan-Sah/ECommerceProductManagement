using System;
using System.Threading.Tasks;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;

namespace Reporting.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines persistence operations for reporting audit logs.
    /// </summary>
    public interface IAuditRepository
    {
        /// <summary>
        /// Reads paged audit logs across all tracked entities.
        /// </summary>
        /// <param name="pageNumber">One-based page number to return.</param>
        /// <param name="pageSize">Maximum number of audit logs to return.</param>
        /// <returns>Paged audit logs ordered by most recent first.</returns>
        Task<PagedResult<AuditLog>> GetAllAuditLogsAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Reads paged audit logs for a product entity.
        /// </summary>
        /// <param name="productId">Product identifier whose audit trail should be returned.</param>
        /// <param name="pageNumber">One-based page number to return.</param>
        /// <param name="pageSize">Maximum number of audit logs to return.</param>
        /// <returns>Paged audit logs for the requested product.</returns>
        Task<PagedResult<AuditLog>> GetProductAuditLogsAsync(Guid productId, int pageNumber, int pageSize);

        /// <summary>
        /// Reads paged audit logs created by a user.
        /// </summary>
        /// <param name="userId">User identifier whose audit activity should be returned.</param>
        /// <param name="pageNumber">One-based page number to return.</param>
        /// <param name="pageSize">Maximum number of audit logs to return.</param>
        /// <returns>Paged audit logs for the requested user.</returns>
        Task<PagedResult<AuditLog>> GetUserAuditLogsAsync(Guid userId, int pageNumber, int pageSize);

        /// <summary>
        /// Adds a new audit log and saves the database change immediately.
        /// </summary>
        /// <param name="log">Audit log entity to persist.</param>
        Task AddAuditLogAsync(AuditLog log);
    }
}
