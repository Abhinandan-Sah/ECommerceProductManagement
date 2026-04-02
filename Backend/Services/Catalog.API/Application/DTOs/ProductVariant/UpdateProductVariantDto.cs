namespace Catalog.API.Application.DTOs.ProductVariant
{
    public class UpdateProductVariantDto
    {
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
    }
}
