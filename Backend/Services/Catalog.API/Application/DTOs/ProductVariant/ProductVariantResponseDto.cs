namespace Catalog.API.Application.DTOs.ProductVariant
{
    public class ProductVariantResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
    }
}
