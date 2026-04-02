namespace Catalog.API.Domain.Entities
{
    public class Category : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = String.Empty;
        public Guid? ParentCategoryId { get; set; }

        public Category? ParentCategory { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
