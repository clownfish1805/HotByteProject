using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Helpers;
using HotByteProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotByteProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestaurantController(AppDbContext context)
        {
            _context = context;
        }

        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var restaurants = await _context.Restaurants
                    .Where(r => !r.IsDeleted)
                    .Select(r => new
                    {
                        r.UserId,
                        r.RestaurantId,
                        r.RestaurantName,
                        r.Location,
                        r.ContactNumber,
                        r.ImageUrl
                    })
                    .ToListAsync();

                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving restaurants: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("Restaurant name is required.");

                var restaurants = await _context.Restaurants
                    .Where(r => r.RestaurantName.ToLower().Contains(name.ToLower()))
                    .Select(r => new
                    {
                   
                        r.RestaurantId,
                        r.RestaurantName,
                        r.Location,
                        r.ContactNumber,
                        r.ImageUrl

                    })
                    .ToListAsync();

                if (!restaurants.Any())
                    return NotFound("No matching restaurants found.");

                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching restaurants: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            var restaurant = await _context.Restaurants
                .Where(r => r.RestaurantId == id)
                .Select(r => new {
                    r.RestaurantId,
                    r.RestaurantName,
                    r.Location,
                    r.ContactNumber,
                    r.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (restaurant == null)
                return NotFound();

            return Ok(restaurant);
        }


        [Authorize(Roles = "Restaurant")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateRestaurant([FromForm] RestaurantUpdateDTO dto) // <- FromForm for file
        {
            try
            {
                var restaurantIdClaim = User.FindFirst("RestaurantId")?.Value;
                if (restaurantIdClaim == null)
                    return Unauthorized("RestaurantId missing in token.");

                int restaurantId = int.Parse(restaurantIdClaim);

                var restaurant = await _context.Restaurants.FindAsync(restaurantId);
                if (restaurant == null)
                    return NotFound("Restaurant not found.");

                // Update fields
                restaurant.RestaurantName = dto.RestaurantName;
                restaurant.Location = dto.Location;
                restaurant.ContactNumber = dto.ContactNumber;

                if (dto.ImageFile != null)
                {
                    restaurant.ImageUrl = await FileHelper.SaveImageAsync(dto.ImageFile, "restaurantImages");
                }

                await _context.SaveChangesAsync();
                return Ok("Updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating restaurant: {ex.Message}");
            }
        }


        [Authorize(Roles = "Restaurant")]
        [HttpDelete]
        public async Task<IActionResult> DeleteRestaurant()
        {
            try
            {
                var restaurantIdClaim = User.FindFirst("RestaurantId")?.Value;
                if (restaurantIdClaim == null)
                    return Unauthorized("RestaurantId missing from token.");

                int restaurantId = int.Parse(restaurantIdClaim);

                var restaurant = await _context.Restaurants
                    .Include(r => r.User) // Include the related User
                    .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);

                if (restaurant == null)
                    return NotFound("Restaurant not found.");

                var user = restaurant.User;

                // Remove restaurant
                _context.Restaurants.Remove(restaurant);

                // Also remove user if role is Restaurant
                if (user != null && user.Role == "Restaurant")
                {
                    _context.Users.Remove(user);
                }

                await _context.SaveChangesAsync();

                return Ok("Restaurant and associated user deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting restaurant: {ex.Message}");
            }
        }

    }
}
