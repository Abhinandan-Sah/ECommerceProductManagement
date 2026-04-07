using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Reporting.API.Application.Interfaces.Services;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;

namespace Reporting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardKpi()
        {
            var snapshot = await _reportingService.GetDashboardKpiAsync();
            if (snapshot == null) return NotFound("Dashboard snapshot not found.");
            return Ok(snapshot);
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductReportFilterDto filter)
        {
            var reports = await _reportingService.GetProductReportsAsync(filter);
            return Ok(reports);
        }

        [HttpGet("products/export")]
        public async Task<IActionResult> ExportProductsCsv()
        {
            var csvBytes = await _reportingService.ExportProductsToCsvAsync();
            return File(csvBytes, "text/csv", "products_report.csv");
        }

        [HttpGet("snapshots")]
        public async Task<IActionResult> GetHistoricalSnapshots()
        {
            var snapshots = await _reportingService.GetSnapshotsAsync();
            return Ok(snapshots);
        }
    }
}
