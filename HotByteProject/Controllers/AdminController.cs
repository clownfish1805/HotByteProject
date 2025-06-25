using System.Security.Claims;
using HotByteProject.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotByteProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching users: {ex.Message}");
            }
        }

        [HttpGet("restaurants")]
        public async Task<IActionResult> GetRestaurants()
        {
            try
            {
                var restaurants = await _adminService.GetAllRestaurantsAsync();
                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching restaurants: {ex.Message}");
            }
        }

        [HttpDelete("restaurants/{restaurantId}")]
        public async Task<IActionResult> DeleteRestaurant(int restaurantId)
        {
            try
            {
                var success = await _adminService.DeleteRestaurantAsync(restaurantId);
                if (!success) return NotFound("Restaurant not found.");
                return Ok("Restaurant deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting restaurant: {ex.Message}");
            }
        }

        [HttpGet("menus")]
        public async Task<IActionResult> GetAllMenus()
        {
            try
            {
                var menus = await _adminService.GetAllMenusAsync();
                return Ok(menus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching menus: {ex.Message}");
            }
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _adminService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching orders: {ex.Message}");
            }
        }
    }
}
