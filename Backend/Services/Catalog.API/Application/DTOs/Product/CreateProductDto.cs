namespace Catalog.API.Application.DTOs.Product
{
    public class CreateProductDto
    {
        public string Name { get; set; } = String.Empty;
        public string Brand { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public string SKU { get; set; } = String.Empty;
        public Guid CategoryId { get; set; }
    }
}
