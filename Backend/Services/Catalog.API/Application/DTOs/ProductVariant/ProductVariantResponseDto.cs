namespace Catalog.API.Application.DTOs.ProductVariant
{
    /// <summary>
    /// Represents product variant data returned by the API.
    /// </summary>
    public class ProductVariantResponseDto
    {
        /// <summary>
        /// Unique variant identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product identifier that owns the variant.
        /// </summary>
        public Guid ProductId { get; set; }

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
