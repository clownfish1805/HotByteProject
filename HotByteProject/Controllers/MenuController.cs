using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotByteProject.Context;
using AutoMapper;

namespace HotByteProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MenuController(IMenuService menuService, AppDbContext context, IMapper mapper)
        {
            _menuService = menuService;
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMenus()
        {
            var menus = await _menuService.GetAllMenus();
            return Ok(menus);
        }


        [HttpPost("create")]
        [Authorize(Roles = "Restaurant,Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateMenu([FromForm] MenuCreateUpdateDTO dto)
        {

            // 🧪 DEBUG: Log incoming data
            Console.WriteLine("🧪 DTO.CategoryName = " + dto.CategoryName);
            Console.WriteLine("🧪 DTO.ItemName = " + dto.ItemName);
            Console.WriteLine("🧪 DTO.RestaurantId = " + dto.RestaurantId);
            Console.WriteLine("🧪 DTO.Price = " + dto.Price);

            try
            {
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("❌ ModelState Invalid");
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"⚠️ {key}: {error.ErrorMessage}");
                        }
                    }
                    return BadRequest(ModelState);
                }


             
                // ✅ Save image if uploaded
                string? imageUrl = null;
                if (dto.ImageFile != null)
                {
                    var fileName = $"{Guid.NewGuid()}_{dto.ImageFile.FileName}";
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageFile.CopyToAsync(stream);
                    }

                    imageUrl = $"/images/{fileName}";
                }

                // ✅ Map DTO and pass to service
                var menuDto = _mapper.Map<MenuDTO>(dto);
                menuDto.ImageUrl = imageUrl;

                var created = await _menuService.AddMenuAsync(menuDto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating menu: {ex.Message}");
            }
        }



        [AllowAnonymous]
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
                        Status = m.Status,
                        ImageUrl = m.ImageUrl
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

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchMenuByName([FromQuery] string name)
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
                    Status = m.Status,
                    ImageUrl = m.ImageUrl
                })
                .ToListAsync();

            return menus.Any() ? Ok(menus) : NotFound("No matching menu items found.");
        }

        [AllowAnonymous]
        [HttpGet("veg-nonveg")]
        public async Task<IActionResult> SearchByDietaryInfo([FromQuery] string dietary)
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
                    Status = m.Status,
                    ImageUrl = m.ImageUrl
                })
                .ToListAsync();

            return menus.Any() ? Ok(menus) : NotFound("No items found for the given dietary filter.");
        }

        [Authorize(Roles = "Restaurant,Admin")]
        [HttpPost("update/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMenu(int id, [FromForm] MenuCreateUpdateDTO dto)
        {
            try
            {
                var restaurantIdClaim = User.FindFirst("RestaurantId")?.Value;
                if (restaurantIdClaim == null)
                    return Unauthorized("RestaurantId not found in token.");

                int restaurantId = int.Parse(restaurantIdClaim);

                var success = await _menuService.UpdateMenuAsync(id, dto, restaurantId);

                return success
                    ? Ok("Menu updated successfully.")
                    : NotFound("Menu not found or not owned by you.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating menu: {ex.Message}");
            }
        }


        //[Authorize(Roles = "Restaurant,Admin")]
        //[HttpDelete("by-name/{itemName}")]
        //public async Task<IActionResult> DeleteMenuByName(string itemName)
        //{
        //    try
        //    {
        //        var restaurantIdClaim = User.FindFirst("RestaurantId")?.Value;
        //        if (restaurantIdClaim == null)
        //            return Unauthorized("RestaurantId not found in token.");

        //        int restaurantId = int.Parse(restaurantIdClaim);
        //        var success = await _menuService.DeleteMenuByNameAsync(itemName, restaurantId);

        //        return success ? Ok("Menu soft-deleted") : NotFound("Menu not found or access denied.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error deleting menu: {ex.Message}");
        //    }
        //}

        [Authorize(Roles = "Restaurant,Admin")]
        [HttpDelete("by-name/{itemName}")]
        public async Task<IActionResult> DeleteMenuByName(string itemName)
        {
            try
            {
                var role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                int? restaurantId = null;

                if (role == "Restaurant")
                {
                    var restaurantIdClaim = User.FindFirst("RestaurantId")?.Value;
                    if (restaurantIdClaim == null)
                        return Unauthorized("RestaurantId not found in token.");

                    restaurantId = int.Parse(restaurantIdClaim);
                }

                // Pass null for restaurantId if Admin
                var success = await _menuService.DeleteMenuByNameAsync(itemName, restaurantId);

                return success ? Ok("Menu soft-deleted") : NotFound("Menu not found or access denied.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting menu: {ex.Message}");
            }
        }

    }
}
