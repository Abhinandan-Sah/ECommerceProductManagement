using System.ComponentModel.DataAnnotations;

namespace Workflow.API.Application.DTOs
{
    public class UpdateInventoryRequestDto
    {
        [Required(AllowEmptyStrings = false)]
        public string WarehouseLocation { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Available quantity cannot be negative.")]
        public int AvailableQty { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }
    }
}