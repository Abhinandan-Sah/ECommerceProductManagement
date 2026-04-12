using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reporting.API.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace Reporting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetAllAuditLogsAsync(pageNumber, pageSize);
            return Ok(logs);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductAuditTrail(Guid productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetProductAuditLogsAsync(productId, pageNumber, pageSize);
            return Ok(logs);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAuditTrail(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetUserAuditLogsAsync(userId, pageNumber, pageSize);
            return Ok(logs);
        }
    }
}
