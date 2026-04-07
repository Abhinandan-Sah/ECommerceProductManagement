using System;
using System.Threading.Tasks;
using Reporting.API.Domain.Entities;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Application.Interfaces.Services;

namespace Reporting.API.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repository;

        public AuditService(IAuditRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<AuditLog>> GetAllAuditLogsAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetAllAuditLogsAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<AuditLog>> GetProductAuditLogsAsync(Guid productId, int pageNumber, int pageSize)
        {
            return await _repository.GetProductAuditLogsAsync(productId, pageNumber, pageSize);
        }

        public async Task<PagedResult<AuditLog>> GetUserAuditLogsAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _repository.GetUserAuditLogsAsync(userId, pageNumber, pageSize);
        }
    }
}
