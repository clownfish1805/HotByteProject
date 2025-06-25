using HotByteProject.DTO;
using HotByteProject.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotByteProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int GetUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var items = await _cartService.GetCartItemsAsync(userId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching cart: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartDTO dto)
        {
            try
            {
                int userId = GetUserId();
                if (userId == 0)
                    return Unauthorized("UserId missing in token");

                var result = await _cartService.AddToCartAsync(userId, dto.MenuId, dto.Quantity);
                if (result == null)
                    return NotFound("Menu item not found or unavailable");

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding to cart: {ex.Message}");
            }
        }

        [HttpPut("{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] int quantity)
        {
            try
            {
                var success = await _cartService.UpdateQuantityAsync(cartItemId, quantity);
                if (!success)
                    return NotFound("Cart item not found.");

                return Ok("Quantity updated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating quantity: {ex.Message}");
            }
        }

        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            try
            {
                var success = await _cartService.RemoveFromCartAsync(cartItemId);
                if (!success)
                    return NotFound("Cart item not found.");

                return Ok("Item removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error removing item: {ex.Message}");
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                await _cartService.ClearCartAsync(userId);
                return Ok("Cart cleared.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error clearing cart: {ex.Message}");
            }
        }
    }
}
