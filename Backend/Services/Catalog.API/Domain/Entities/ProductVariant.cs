namespace Catalog.API.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProductId { get; set; }
        public Product? Product { get; set; } 

        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
    }
}
