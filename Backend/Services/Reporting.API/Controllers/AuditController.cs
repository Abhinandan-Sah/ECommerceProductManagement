using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reporting.API.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace Reporting.API.Controllers
{
    /// <summary>
    /// Exposes audit history endpoints for administrators.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        /// <summary>
        /// Creates the audit controller with audit read operations.
        /// </summary>
        /// <param name="auditService">Service that reads audit history.</param>
        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Gets paged audit logs across all tracked entities.
        /// </summary>
        /// <param name="pageNumber">One-based page number to return.</param>
        /// <param name="pageSize">Maximum number of audit logs to return.</param>
        /// <returns>Paged audit logs ordered by most recent first.</returns>
        /// <response code="200">Audit logs were returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to view audit history.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetAllAuditLogsAsync(pageNumber, pageSize);
            return Ok(logs);
        }

        /// <summary>
        /// Gets paged audit logs for a product.
        /// </summary>
        /// <param name="productId">Product identifier whose audit trail should be returned.</param>
        /// <param name="pageNumber">One-based page number to return.</param>
        /// <param name="pageSize">Maximum number of audit logs to return.</param>
        /// <returns>Paged audit logs for the requested product.</returns>
        /// <response code="200">Product audit logs were returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to view audit history.</response>
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductAuditTrail(Guid productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetProductAuditLogsAsync(productId, pageNumber, pageSize);
            return Ok(logs);
        }

        /// <summary>
        /// Gets paged audit logs created by a user.
        /// </summary>
        /// <param name="userId">User identifier whose audit activity should be returned.</param>
        /// <param name="pageNumber">One-based page number to return.</param>
        /// <param name="pageSize">Maximum number of audit logs to return.</param>
        /// <returns>Paged audit logs for the requested user.</returns>
        /// <response code="200">User audit logs were returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to view audit history.</response>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAuditTrail(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetUserAuditLogsAsync(userId, pageNumber, pageSize);
            return Ok(logs);
        }
    }
}
