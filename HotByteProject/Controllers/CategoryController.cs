using HotByteProject.DTO;
using HotByteProject.Helpers;
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

        //// Create a new category
        //[Authorize(Roles = "Admin, Restaurant")]
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] string name)
        //{
        //    if (string.IsNullOrWhiteSpace(name))
        //        return BadRequest("Category name is required.");

        //    var success = await _categoryService.CreateCategoryAsync(name);
        //    if (!success)
        //        return BadRequest("Category already exists or name is invalid.");

        //    return Ok("Category created successfully.");
        //}


        [Authorize(Roles = "Admin, Restaurant")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CategoryBasicDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Category name is required.");

            var exists = await _categoryService.CategoryExistsAsync(dto.CategoryName);
            if (exists)
                return Conflict("Category already exists.");

            string? imageUrl = null;
            if (dto.ImageFile != null)
            {
                imageUrl = await FileHelper.SaveImageAsync(dto.ImageFile, "categoryImages");
            }

            var created = await _categoryService.CreateCategoryAsync(dto.CategoryName, imageUrl);
            if (!created)
                return StatusCode(500, "Failed to create category.");

            return Ok("Category created successfully.");
        }

        // Get all categories with their menus
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }

        // Get all menus under a specific category
        [AllowAnonymous]
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
                RestaurantName = m.Restaurant?.RestaurantName,
                ImageUrl=m.ImageUrl
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

        // Get only category ID and Name (no menus)
        [AllowAnonymous]
        [HttpGet("names")]
        public async Task<IActionResult> GetCategoryNames()
        {
            var result = await _categoryService.GetCategoryNamesAsync();
            return Ok(result);
        }

    }
}
