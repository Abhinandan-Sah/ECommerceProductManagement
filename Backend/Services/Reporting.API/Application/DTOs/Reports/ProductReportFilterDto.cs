using System.ComponentModel.DataAnnotations;

namespace Reporting.API.Application.DTOs.Reports
{
    /// <summary>
    /// Captures filtering and paging options for product reporting queries.
    /// </summary>
    public class ProductReportFilterDto
    {
        /// <summary>
        /// One-based page number requested by the caller.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Maximum number of report rows to return.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Optional category display name used to narrow product report rows.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Category cannot exceed 200 characters")]
        public string? Category { get; set; }

        /// <summary>
        /// Optional product workflow status used to narrow product report rows.
        /// </summary>
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }
    }
}
