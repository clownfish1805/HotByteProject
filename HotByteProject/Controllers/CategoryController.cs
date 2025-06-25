using HotByteProject.DTO;
using HotByteProject.Repository.Implementations;
using HotByteProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotByteProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Create a new category
        [Authorize(Roles = "Admin, Restaurant")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Category name is required.");

            var success = await _categoryService.CreateCategoryAsync(name);
            if (!success)
                return BadRequest("Category already exists or name is invalid.");

            return Ok("Category created successfully.");
        }

        // Get all categories with their menus
        [Authorize(Roles = "Admin, Restaurant, User")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }

        // Get all menus under a specific category
        [Authorize(Roles = "Admin, Restaurant, User")]
        [HttpGet("{name}/menus")]
        public async Task<IActionResult> GetMenusByCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Category name is required.");

            var category = await _categoryService.GetCategoryByNameAsync(name);
            if (category == null)
                return NotFound("Category not found.");

            var results = category.Menus.Select(m => new MenuDetailsDTO
            {
                MenuId = m.MenuId,
                ItemName = m.ItemName,
                Description = m.Description,
                Category = category.CategoryName,
                Price = m.Price,
                DietaryInfo = m.DietaryInfo,
                TasteInfo = m.TasteInfo,
                AvailabilityTime = m.AvailabilityTime,
                NutritionalInfo = m.NutritionalInfo,
                RestaurantId = m.RestaurantId,
                RestaurantName = m.Restaurant?.RestaurantName
            }).ToList();

            return Ok(results);
        }

        // Soft delete a category and its menus
        [Authorize(Roles = "Admin, Restaurant")]
        [HttpDelete("delete/{name}")]
        public async Task<IActionResult> DeleteCategoryByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Category name is required.");

            var success = await _categoryService.DeleteCategoryByNameAsync(name);
            if (!success)
                return NotFound("Category not found.");

            return Ok("Category and its associated menus soft-deleted successfully.");
        }
    }
}
