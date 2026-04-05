using System.ComponentModel.DataAnnotations;

namespace Workflow.API.Application.DTOs
{
    public class UpdatePricingRequestDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "MRP must be greater than zero.")]
        public decimal MRP { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Sale Price must be greater than zero.")]
        public decimal SalePrice { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "GST must be between 0 and 100.")]
        public decimal GSTPercent { get; set; }
    }
}