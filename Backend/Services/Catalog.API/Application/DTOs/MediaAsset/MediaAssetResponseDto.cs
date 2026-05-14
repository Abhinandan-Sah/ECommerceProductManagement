namespace Catalog.API.Application.DTOs.MediaAsset
{
    /// <summary>
    /// Represents media asset data returned by the API.
    /// </summary>
    public class MediaAssetResponseDto
    {
        /// <summary>
        /// Unique media asset identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product identifier that owns the media asset.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Public URL for the media asset.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Display order for the media asset within the product gallery.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Alternative text used for accessibility and fallback display.
        /// </summary>
        public string AltText { get; set; } = string.Empty;
    }
}
