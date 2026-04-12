using System.ComponentModel.DataAnnotations;

namespace Workflow.API.Application.DTOs
{
    public class UpdateInventoryRequestDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Warehouse location is required")]
        [MaxLength(200, ErrorMessage = "Warehouse location cannot exceed 200 characters")]
        public string WarehouseLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Available quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Available quantity cannot be negative.")]
        public int AvailableQty { get; set; }

        [Required(ErrorMessage = "Reorder level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
        public int ReorderLevel { get; set; }
    }
}