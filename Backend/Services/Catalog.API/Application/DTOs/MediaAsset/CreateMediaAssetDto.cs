using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.MediaAsset
{
    /// <summary>
    /// Captures the data required to add a media asset to a product.
    /// </summary>
    public class CreateMediaAssetDto
    {
        /// <summary>
        /// Public URL for the media asset.
        /// </summary>
        [Required(ErrorMessage = "URL is required")]
        [MaxLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Display order for the media asset within the product gallery.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be non-negative")]
        public int SortOrder { get; set; }

        /// <summary>
        /// Alternative text used for accessibility and fallback display.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Alt text cannot exceed 200 characters")]
        public string AltText { get; set; } = string.Empty;
    }
}
