using System.ComponentModel.DataAnnotations;

namespace Workflow.API.Application.DTOs
{
    /// <summary>
    /// Captures inventory values accepted by the inventory update endpoint.
    /// </summary>
    public class UpdateInventoryRequestDto
    {
        /// <summary>
        /// Warehouse or storage location for the product stock.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Warehouse location is required")]
        [MaxLength(200, ErrorMessage = "Warehouse location cannot exceed 200 characters")]
        public string WarehouseLocation { get; set; } = string.Empty;

        /// <summary>
        /// Quantity currently available for sale or allocation.
        /// </summary>
        [Required(ErrorMessage = "Available quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Available quantity cannot be negative.")]
        public int AvailableQty { get; set; }

        /// <summary>
        /// Stock level at which the product should be reordered.
        /// </summary>
        [Required(ErrorMessage = "Reorder level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
        public int ReorderLevel { get; set; }
    }
}
