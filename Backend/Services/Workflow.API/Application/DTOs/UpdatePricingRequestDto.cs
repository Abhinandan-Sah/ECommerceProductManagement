using System.ComponentModel.DataAnnotations;

namespace Workflow.API.Application.DTOs
{
    /// <summary>
    /// Captures pricing values accepted by the pricing update endpoint.
    /// </summary>
    public class UpdatePricingRequestDto
    {
        /// <summary>
        /// Maximum retail price for the product.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "MRP must be greater than zero.")]
        public decimal MRP { get; set; }

        /// <summary>
        /// Sale price charged to customers.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Sale Price must be greater than zero.")]
        public decimal SalePrice { get; set; }

        /// <summary>
        /// GST percentage applied to the product price.
        /// </summary>
        [Required]
        [Range(0.01, 100, ErrorMessage = "GST must be greater than zero and no more than 100.")]
        public decimal GSTPercent { get; set; }
    }
}
