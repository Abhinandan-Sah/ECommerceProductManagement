using Catalog.API.Application.DTOs.Category;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAllCategoriesAsync()
        {
            var response = await _service.GetAllCategoriesAsync();
            return Ok(response);
        }

        [HttpGet("{id}", Name = "GetCategoryById")]
        public async Task<ActionResult<CategoryResponseDto>> GetCategoryByIdAsync(Guid id)
        {
            var response = await _service.GetCategoryByIdAsync(id);
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult<CategoryResponseDto>> AddCategoryAsync([FromBody] CreateCategoryDto newCategoryDto)
        {
            var response = await _service.AddCategoryAsync(newCategoryDto);
            return CreatedAtRoute("GetCategoryById", new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<ActionResult> UpdateCategoryAsync(Guid id, [FromBody] Category newCategory)
        {
            try
            {
                await _service.UpdateCategoryAsync(id, newCategory);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategoryAsync(Guid id)
        {
            try
            {
                await _service.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
}