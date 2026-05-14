namespace Catalog.API.Application.DTOs.Category
{
    /// <summary>
    /// Represents category data returned by the API.
    /// </summary>
    public class CategoryResponseDto
    {
        /// <summary>
        /// Unique category identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Category display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Parent category identifier when this category is nested.
        /// </summary>
        public Guid? ParentCategoryId { get; set; }

        /// <summary>
        /// Parent category display name, or a fallback label for top-level categories.
        /// </summary>
        public string ParentCategoryName { get; set; } = string.Empty;
    }
}
