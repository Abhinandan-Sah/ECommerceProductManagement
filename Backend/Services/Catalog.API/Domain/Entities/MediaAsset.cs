namespace Catalog.API.Domain.Entities
{
    /// <summary>
    /// Represents a media item attached to a product.
    /// </summary>
    public class MediaAsset : BaseEntity
    {
        /// <summary>
        /// Unique media asset identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Product identifier that owns the media asset.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Product navigation property.
        /// </summary>
        public Product? Product { get; set; }

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

