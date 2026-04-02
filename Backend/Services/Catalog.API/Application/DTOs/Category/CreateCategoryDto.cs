namespace Catalog.API.Application.DTOs.Category
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;

        // Nullable because top-level categories won't have a parent
        public Guid? ParentCategoryId { get; set; }
    }
}
