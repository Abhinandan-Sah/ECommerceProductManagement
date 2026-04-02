using Catalog.API.Application.DTOs.Category;
using Catalog.API.Application.Interfaces;
using Catalog.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Resolves to: /api/categories
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repository;

        public CategoriesController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        // GET: /api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAllCategoriesAsync()
        {
            var categories = await _repository.GetAllCategoriesAsync();

            var response = categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId
            });

            return Ok(response);
        }

        // GET: /api/categories/{id}
        [HttpGet("{id}", Name = "GetCategoryById")]
        public async Task<ActionResult<CategoryResponseDto>> GetCategoryByIdAsync(Guid id)
        {
            var category = await _repository.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var response = new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory != null ? category.ParentCategory.Name : "None"
            };

            return Ok(response);
        }

        // POST: /api/categories
        [HttpPost]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult<CategoryResponseDto>> AddCategoryAsync([FromBody] CreateCategoryDto newCategoryDto)
        {
            // Map DTO -> Entity
            var categoryEntity = new Category
            {
                Name = newCategoryDto.Name,
                ParentCategoryId = newCategoryDto.ParentCategoryId
            };

            // Save via Repo
            var savedCategory = await _repository.AddCategoryAsync(categoryEntity);

            // Map Entity -> DTO
            var response = new CategoryResponseDto
            {
                Id = savedCategory.Id,
                Name = savedCategory.Name,
                ParentCategoryId = savedCategory.ParentCategoryId
            };

            return CreatedAtRoute("GetCategoryById", new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateCategoryAsync(Guid id, [FromBody]Category newCategory)
        {
            var existingCategory = await _repository.GetCategoryByIdAsync(id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = newCategory.Name;
            await _repository.UpdateCategoryAsync(existingCategory);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategoryAsync(Guid id)
        {
            var category = await _repository.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            await _repository.DeleteCategoryAsync(category);

            return NoContent();
        }
    }
}