using HotByteProject.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotByteProject.Context;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;

namespace HotByteProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly AppDbContext _context;

        public MenuController(IMenuService menuService, AppDbContext context)
        {
            _menuService = menuService;
            _context = context;
        }

        [Authorize(Roles = "Admin, Restaurant, User")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Menus.Where(m => m.AvailabilityTime.ToLower() != "unavailable"))
                    .ThenInclude(m => m.Restaurant)
                    .ToListAsync();

                var result = categories.Select(c => new CategoryWithMenusDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Menus = c.Menus.Select(m => new MenuDetailsDTO
                    {
                        MenuId = m.MenuId,
                        ItemName = m.ItemName,
                        Description = m.Description,
                        Category = c.CategoryName,
                        Price = m.Price,
                        DietaryInfo = m.DietaryInfo,
                        TasteInfo = m.TasteInfo,
                        AvailabilityTime = m.AvailabilityTime,
                        NutritionalInfo = m.NutritionalInfo,
                        RestaurantId = m.RestaurantId,
                        RestaurantName = m.Restaurant.RestaurantName,
                        Status = m.Status
                    }).ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching menus: {ex.Message}");
            }
        }

        [HttpGet("by-restaurant/{restaurantId}")]
        public async Task<IActionResult> GetByRestaurant(int restaurantId)
        {
            try
            {
                var menus = await _menuService.GetMenusByRestaurantAsync(restaurantId);
                return Ok(menus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching restaurant menus: {ex.Message}");
            }
        }


        [Authorize(Roles = "Admin, Restaurant, User")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchMenuByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("Item name is required.");

                var menus = await _context.Menus
                    .Include(m => m.Restaurant)
                    .Include(m => m.Category)
                    .Where(m => m.ItemName.ToLower().Contains(name.ToLower()) &&
                                m.AvailabilityTime.ToLower() != "unavailable")
                    .Select(m => new MenuDetailsDTO
                    {
                        MenuId = m.MenuId,
                        ItemName = m.ItemName,
                        Description = m.Description,
                        Category = m.Category.CategoryName,
                        Price = m.Price,
                        DietaryInfo = m.DietaryInfo,
                        TasteInfo = m.TasteInfo,
                        AvailabilityTime = m.AvailabilityTime,
                        NutritionalInfo = m.NutritionalInfo,
                        RestaurantId = m.RestaurantId,
                        RestaurantName = m.Restaurant.RestaurantName,
                        Status = m.Status
                    })
                    .ToListAsync();

                if (!menus.Any())
                    return NotFound("No matching menu items found.");

                return Ok(menus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching menu items: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin, Restaurant, User")]
        [HttpGet("veg/nonveg")]
        public async Task<IActionResult> SearchByDietaryInfo([FromQuery] string dietary)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dietary))
                    return BadRequest("Dietary info is required (e.g., Vegetarian, Non-Vegetarian).");

                var menus = await _context.Menus
                    .Include(m => m.Restaurant)
                    .Include(m => m.Category)
                    .Where(m => m.DietaryInfo.ToLower() == dietary.ToLower() &&
                                m.AvailabilityTime.ToLower() != "unavailable")
                    .Select(m => new MenuDetailsDTO
                    {
                        MenuId = m.MenuId,
                        ItemName = m.ItemName,
                        Description = m.Description,
                        Category = m.Category.CategoryName,
                        Price = m.Price,
                        DietaryInfo = m.DietaryInfo,
                        TasteInfo = m.TasteInfo,
                        AvailabilityTime = m.AvailabilityTime,
                        NutritionalInfo = m.NutritionalInfo,
                        RestaurantId = m.RestaurantId,
                        RestaurantName = m.Restaurant.RestaurantName,
                        Status = m.Status
                    })
                    .ToListAsync();

                if (!menus.Any())
                    return NotFound("No menu items found for the specified dietary preference.");

                return Ok(menus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error filtering menu by dietary info: {ex.Message}");
            }
        }

        [Authorize(Roles = "Restaurant,Admin")]
        [HttpPost]
        public async Task<ActionResult<MenuDetailsDTO>> AddMenuAsync(MenuDTO menuDto)
        {
            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == menuDto.CategoryName.ToLower());

                if (category == null)
                    return BadRequest("Category not found. Please create the category first.");

                //Duplicate check
                var exists = await _context.Menus.AnyAsync(m =>
                    m.ItemName.ToLower() == menuDto.ItemName.ToLower() &&
                    m.RestaurantId == menuDto.RestaurantId);

                if (exists)
                    return Conflict("Menu item with the same name already exists for this restaurant.");

                var menu = new Menu
                {
                    ItemName = menuDto.ItemName,
                    Description = menuDto.Description,
                    CategoryId = category.CategoryId,
                    Price = menuDto.Price,
                    DietaryInfo = menuDto.DietaryInfo,
                    TasteInfo = menuDto.TasteInfo,
                    NutritionalInfo = menuDto.NutritionalInfo,
                    AvailabilityTime = menuDto.AvailabilityTime,
                    RestaurantId = menuDto.RestaurantId,
                    Status = menuDto.Status
                };

                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();

                var menuWithDetails = await _context.Menus
                    .Include(m => m.Category)
                    .Include(m => m.Restaurant)
                    .FirstOrDefaultAsync(m => m.MenuId == menu.MenuId);

                return new MenuDetailsDTO
                {
                    MenuId = menuWithDetails.MenuId,
                    ItemName = menuWithDetails.ItemName,
                    Description = menuWithDetails.Description,
                    Category = menuWithDetails.Category?.CategoryName ?? "N/A",
                    Price = menuWithDetails.Price,
                    DietaryInfo = menuWithDetails.DietaryInfo,
                    TasteInfo = menuWithDetails.TasteInfo,
                    NutritionalInfo = menuWithDetails.NutritionalInfo,
                    AvailabilityTime = menuWithDetails.AvailabilityTime,
                    RestaurantId = menuWithDetails.RestaurantId,
                    RestaurantName = menuWithDetails.Restaurant?.RestaurantName ?? "N/A",
                    Status = menuWithDetails.Status
                };
            }

            catch (Exception ex)
            {
                throw new Exception($"Error adding menu: {ex.Message} - {ex.InnerException?.Message}");
            }

        }


        [Authorize(Roles = "Restaurant,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenu(int id, [FromBody] MenuDTO dto)
        {
            try
            {
                var restaurantIdClaim = User.FindFirst("RestaurantID")?.Value;
                if (restaurantIdClaim == null)
                    return Unauthorized("RestaurantId not found in token.");

                int restaurantId = int.Parse(restaurantIdClaim);

                var success = await _menuService.UpdateMenuAsync(id, dto, restaurantId);
                return !success
                    ? NotFound("Menu not found or not owned by you.")
                    : Ok("Menu updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating menu: {ex.Message}");
            }
        }
        [Authorize(Roles = "Restaurant,Admin")]
        [HttpDelete("by-name/{itemName}")]
        public async Task<IActionResult> DeleteMenuByName(string itemName)
        {
            try
            {
                var restaurantIdClaim = User.FindFirst("RestaurantId")?.Value;
                if (restaurantIdClaim == null)
                    return Unauthorized("RestaurantId not found in token.");

                int restaurantId = int.Parse(restaurantIdClaim);

                var success = await _menuService.DeleteMenuByNameAsync(itemName, restaurantId);
                return !success ? NotFound("Menu not found or access denied.") : Ok("Menu soft-deleted");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error soft-deleting menu by name: {ex.Message}");
            }
        }

    }
}
