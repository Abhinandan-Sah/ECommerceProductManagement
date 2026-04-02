namespace Catalog.API.Application.DTOs.Category
{
    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }

        public string ParentCategoryName { get; set; } = string.Empty;
    }
}
