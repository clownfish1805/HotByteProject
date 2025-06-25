using HotByteProject.Context;
using HotByteProject.DTO;
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

        
        [Authorize(Roles = "Admin,User,Restaurant")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var restaurants = await _context.Restaurants
                    .Select(r => new
                    {
                        r.UserId,
                        r.RestaurantName,
                        r.Location,
                        r.ContactNumber
                    })
                    .ToListAsync();

                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving restaurants: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,User,Restaurant")]
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
                        r.ContactNumber
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


        [Authorize(Roles = "Restaurant")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateRestaurant([FromBody] RestaurantUpdateDTO dto)
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

                restaurant.RestaurantName = dto.RestaurantName;
                restaurant.Location = dto.Location;
                restaurant.ContactNumber = dto.ContactNumber;

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

                var restaurant = await _context.Restaurants.FindAsync(restaurantId);
                if (restaurant == null)
                    return NotFound("Restaurant not found.");

                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();

                return Ok("Restaurant deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting restaurant: {ex.Message}");
            }
        }
    }
}
