using System;
using System.Threading.Tasks;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Application.Interfaces.Services;

namespace Reporting.API.Application.Services
{
    /// <summary>
    /// Handles audit history read operations and logging around audit queries.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repository;
        private readonly ILogger<AuditService> _logger;

        /// <summary>
        /// Creates the audit service with repository access and logging.
        /// </summary>
        public AuditService(IAuditRepository repository, ILogger<AuditService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<PagedResult<AuditLog>> GetAllAuditLogsAsync(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching all audit logs (Page: {Page}, PageSize: {PageSize})", pageNumber, pageSize);

            return await _repository.GetAllAuditLogsAsync(pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<PagedResult<AuditLog>> GetProductAuditLogsAsync(Guid productId, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching audit logs for product {ProductId}", productId);

            return await _repository.GetProductAuditLogsAsync(productId, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task<PagedResult<AuditLog>> GetUserAuditLogsAsync(Guid userId, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching audit logs for user {UserId}", userId);

            return await _repository.GetUserAuditLogsAsync(userId, pageNumber, pageSize);
        }
    }
}
