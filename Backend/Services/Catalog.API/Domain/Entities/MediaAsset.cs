namespace Catalog.API.Domain.Entities
{
    public class MediaAsset : BaseEntity
    {
            public Guid Id { get; set; } = Guid.NewGuid();

            public Guid ProductId { get; set; }
            public Product? Product { get; set; }

            public string Url { get; set; } = string.Empty;
            public int SortOrder { get; set; }
            public string AltText { get; set; } = string.Empty;
        
    }
}

