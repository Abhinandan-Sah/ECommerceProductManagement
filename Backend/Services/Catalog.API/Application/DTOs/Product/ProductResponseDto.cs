using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.DTOs.Product
{
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PublishStatus PublishStatus { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}
