using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotByteProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        //private int GetUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        private int GetUserId()
        {
            var claim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(claim))
                throw new UnauthorizedAccessException("User ID claim is missing.");

            return int.Parse(claim);
        }


        [Authorize(Roles = "User")]
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDTO dto)
        {
            try
            {
                var userId = GetUserId();

                var order = await _orderService.PlaceOrderAsync(userId, dto);
                if (order == null)
                    return BadRequest("Order could not be placed.");

                var orderDto = new OrderResponseDTO
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    DeliveryAddress = order.DeliveryAddress,
                    Items = order.OrderItems.Select(oi => new OrderItemDTO
                    {
                        MenuId = oi.MenuId,
                        Quantity = oi.Quantity
                    }).ToList()
                };

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error placing order: {ex.Message}");
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("myorders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var userId = GetUserId();
                var orders = await _orderService.GetOrdersByUserAsync(userId);

                var result = orders.Select(order => new OrderResponseDTO
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    DeliveryAddress = order.DeliveryAddress,
                    Items = order.OrderItems.Select(oi => new OrderItemDTO
                    {
                        MenuId = oi.MenuId,
                        Quantity = oi.Quantity
                    }).ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving orders: {ex.Message}");
            }
        }

        [Authorize(Roles = "Restaurant,Admin")]
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetOrdersByRestaurant(int restaurantId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByRestaurantAsync(restaurantId);

                var result = orders.Select(order => new OrderResponseDTO
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    DeliveryAddress = order.DeliveryAddress,
                    UserName = order.User?.Name ?? "Unknown", // ✅ Add this

                    Items = order.OrderItems.Select(oi => new OrderItemDTO
                    {
                        MenuId = oi.MenuId,
                        Quantity = oi.Quantity
                    }).ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving restaurant orders: {ex.Message}");
            }
        }

        //[Authorize(Roles = "Admin")]
        //[HttpGet("all")]
        //public async Task<IActionResult> GetAllOrdersAsync()
        //{
        //    try
        //    {
        //        var orders = await _orderService.GetAllOrdersAsync();
        //        return Ok(orders);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error retrieving all orders: {ex.Message}");
        //    }
        //}

        [Authorize(Roles = "Restaurant,Admin")]
        [HttpPatch("status/{orderId}")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOrderStatusDTO dto)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(orderId, dto.Status);
                if (!success)
                    return NotFound("Order not found.");

                return Ok("Order status updated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating order status: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,Restaurant")]
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            try
            {
                int? restaurantId = null;

                if (User.IsInRole("Restaurant"))
                    restaurantId = int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

                var deleted = await _orderService.DeleteOrderAsync(orderId, restaurantId);
                if (!deleted)
                    return NotFound("Order not found or not authorized to delete.");

                return Ok("Order deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting order: {ex.Message}");
            }
        }
    }
}
