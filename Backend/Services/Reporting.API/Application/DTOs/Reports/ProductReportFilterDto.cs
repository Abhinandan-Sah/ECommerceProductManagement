using System.ComponentModel.DataAnnotations;

namespace Reporting.API.Application.DTOs.Reports
{
    public class ProductReportFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        [MaxLength(200, ErrorMessage = "Category cannot exceed 200 characters")]
        public string? Category { get; set; }

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }
    }
}
