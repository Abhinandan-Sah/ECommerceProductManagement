using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reporting.API.Application.DTOs.Common;
using Reporting.API.Application.DTOs.Reports;
using Reporting.API.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace Reporting.API.Controllers
{
    /// <summary>
    /// Exposes reporting endpoints for dashboard metrics, product report rows, exports, and snapshot history.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,ProductManager")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        /// <summary>
        /// Creates the reports controller with reporting business operations.
        /// </summary>
        /// <param name="reportingService">Service that reads reporting projections and exports.</param>
        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        /// <summary>
        /// Gets the latest dashboard KPI snapshot.
        /// </summary>
        /// <returns>The latest dashboard snapshot, or not found when no snapshot has been created.</returns>
        /// <response code="200">Dashboard snapshot was found and returned.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to view reports.</response>
        /// <response code="404">No dashboard snapshot exists.</response>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardKpi()
        {
            var snapshot = await _reportingService.GetDashboardKpiAsync();
            if (snapshot == null) return NotFound("Dashboard snapshot not found.");
            return Ok(snapshot);
        }

        /// <summary>
        /// Gets paged product report rows using optional category and status filters.
        /// </summary>
        /// <param name="filter">Filtering and paging options supplied from the query string.</param>
        /// <returns>Matching product report rows with paging metadata.</returns>
        /// <response code="200">Product report rows were returned successfully.</response>
        /// <response code="400">Filtering or paging values failed validation.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to view reports.</response>
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductReportFilterDto filter)
        {
            var reports = await _reportingService.GetProductReportsAsync(filter);
            return Ok(reports);
        }

        /// <summary>
        /// Exports product report rows as a CSV file.
        /// </summary>
        /// <returns>A CSV file containing all product report rows.</returns>
        /// <response code="200">CSV export was generated successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to export reports.</response>
        [HttpGet("products/export")]
        public async Task<IActionResult> ExportProductsCsv()
        {
            var csvBytes = await _reportingService.ExportProductsToCsvAsync();
            // Keep the export endpoint thin; CSV formatting stays in the service for easier testing.
            return File(csvBytes, "text/csv", "products_report.csv");
        }

        /// <summary>
        /// Gets historical dashboard snapshots for trend views.
        /// </summary>
        /// <returns>Dashboard snapshots ordered by snapshot date.</returns>
        /// <response code="200">Dashboard snapshots were returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to view reports.</response>
        [HttpGet("snapshots")]
        public async Task<IActionResult> GetHistoricalSnapshots()
        {
            var snapshots = await _reportingService.GetSnapshotsAsync();
            return Ok(snapshots);
        }
    }
}
