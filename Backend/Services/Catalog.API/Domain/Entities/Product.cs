using Catalog.API.Domain.Enums;

namespace Catalog.API.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = String.Empty;
        public string Brand { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public string SKU { get; set; } = String.Empty;
        public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();


    }
}
