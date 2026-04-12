using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.MediaAsset
{
    public class CreateMediaAssetDto
    {
        [Required(ErrorMessage = "URL is required")]
        [MaxLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string Url { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be non-negative")]
        public int SortOrder { get; set; }

        [MaxLength(200, ErrorMessage = "Alt text cannot exceed 200 characters")]
        public string AltText { get; set; } = string.Empty;
    }
}
