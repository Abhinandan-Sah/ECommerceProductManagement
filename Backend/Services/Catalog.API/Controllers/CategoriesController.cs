using Catalog.API.Application.DTOs.Category;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    /// <summary>
    /// Exposes category lookup and administrator category management for the catalog.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        /// <summary>
        /// Creates the categories controller with the category service it delegates to.
        /// </summary>
        /// <param name="service">Service that handles category rules and persistence work.</param>
        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets every product category available in the catalog.
        /// </summary>
        /// <returns>The list of category records.</returns>
        /// <response code="200">Categories were returned successfully.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAllCategoriesAsync()
        {
            var response = await _service.GetAllCategoriesAsync();
            return Ok(response);
        }

        /// <summary>
        /// Gets one category by its identifier.
        /// </summary>
        /// <param name="id">Category identifier to load.</param>
        /// <returns>The requested category, or not found when it does not exist.</returns>
        /// <response code="200">Category was found and returned.</response>
        /// <response code="404">No category exists for the supplied identifier.</response>
        [HttpGet("{id}", Name = "GetCategoryById")]
        public async Task<ActionResult<CategoryResponseDto>> GetCategoryByIdAsync(Guid id)
        {
            var response = await _service.GetCategoryByIdAsync(id);
            if (response == null) return NotFound();
            return Ok(response);
        }

        /// <summary>
        /// Creates a category for products to be grouped under.
        /// </summary>
        /// <param name="newCategoryDto">Category details supplied by the catalog manager.</param>
        /// <returns>The created category with its generated identifier.</returns>
        /// <response code="201">Category was created successfully.</response>
        /// <response code="400">Category details failed validation.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to create categories.</response>
        [HttpPost]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult<CategoryResponseDto>> AddCategoryAsync([FromBody] CreateCategoryDto newCategoryDto)
        {
            var response = await _service.AddCategoryAsync(newCategoryDto);
            return CreatedAtRoute("GetCategoryById", new { id = response.Id }, response);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">Category identifier to update.</param>
        /// <param name="newCategory">New category values to save.</param>
        /// <returns>No content when the update is complete.</returns>
        /// <response code="204">Category was updated successfully.</response>
        /// <response code="400">Category details failed validation.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to update categories.</response>
        /// <response code="404">No category exists for the supplied identifier.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateCategoryAsync(Guid id, [FromBody] Category newCategory)
        {
            await _service.UpdateCategoryAsync(id, newCategory);
            return NoContent();
        }

        /// <summary>
        /// Deletes a category from the catalog.
        /// </summary>
        /// <param name="id">Category identifier to delete.</param>
        /// <returns>No content when the category has been removed.</returns>
        /// <response code="204">Category was deleted successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to delete categories.</response>
        /// <response code="404">No category exists for the supplied identifier.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategoryAsync(Guid id)
        {
            await _service.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
