namespace Catalog.API.Domain.Entities
{
    /// <summary>
    /// Represents a product category, including optional hierarchy through a parent category.
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Unique category identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Category display name.
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Parent category identifier when this category is nested.
        /// </summary>
        public Guid? ParentCategoryId { get; set; }

        /// <summary>
        /// Parent category navigation property.
        /// </summary>
        public Category? ParentCategory { get; set; }

        /// <summary>
        /// Products assigned to this category.
        /// </summary>
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
