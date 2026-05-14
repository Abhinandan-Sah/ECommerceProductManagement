namespace Catalog.API.Domain.Entities
{
    /// <summary>
    /// Represents a sellable product option such as color and size.
    /// </summary>
    public class ProductVariant : BaseEntity
    {
        /// <summary>
        /// Unique variant identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Product identifier that owns the variant.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Product navigation property.
        /// </summary>
        public Product? Product { get; set; } 

        /// <summary>
        /// Color option for the variant.
        /// </summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// Size option for the variant.
        /// </summary>
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Scannable barcode value for the variant.
        /// </summary>
        public string Barcode { get; set; } = string.Empty;
    }
}
