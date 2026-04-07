namespace Reporting.API.Application.DTOs.Reports
{
    public class ProductReportFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Category { get; set; }
        public string? Status { get; set; }
    }
}
