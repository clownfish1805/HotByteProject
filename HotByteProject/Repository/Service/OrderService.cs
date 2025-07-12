using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using System;

namespace HotByteProject.Repository.Service
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> PlaceOrderAsync(int userId, OrderDTO orderDto)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Menu)
                    .ThenInclude(m => m.Restaurant)
                .Where(ci => ci.UserId == userId && !ci.Menu.IsDeleted)
                .ToListAsync();

            if (!cartItems.Any())
                throw new Exception("Your cart is empty.");

            // Check for invalid or unavailable menu items
            var invalidItems = cartItems
                .Where(ci => ci.Menu == null || ci.Menu.Restaurant == null ||
                             string.Equals(ci.Menu.AvailabilityTime, "unavailable", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (invalidItems.Any())
            {
                var itemNames = string.Join(", ", invalidItems.Select(i => i.Menu?.ItemName ?? "Unknown"));
                throw new Exception($"Cannot place order. Invalid or unavailable items in cart: {itemNames}");
            }

            // ✅ Enforce single-restaurant rule
            var distinctRestaurantIds = cartItems
                .Select(ci => ci.Menu.RestaurantId)
                .Distinct()
                .ToList();

            if (distinctRestaurantIds.Count > 1)
            {
                throw new Exception("You can only order from one restaurant at a time. Please clear the cart first.");
            }

            // ✅ Extract restaurant name
            var restaurantName = cartItems
                .Select(ci => ci.Menu.Restaurant?.RestaurantName)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(restaurantName))
                throw new Exception("Menu item(s) are not linked to a valid restaurant.");

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                DeliveryAddress = orderDto.DeliveryAddress,
                TotalAmount = 0, // will calculate later
                RestaurantName = restaurantName
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // orderId generated

            decimal total = 0;

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    MenuId = cartItem.MenuId,
                    Quantity = cartItem.Quantity
                };

                _context.OrderItems.Add(orderItem);
                total += cartItem.Menu.Price * cartItem.Quantity;
            }

            order.TotalAmount = total;
            await _context.SaveChangesAsync();

            // ✅ Clear cart after placing order
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Load OrderItems back into order for controller response mapping
            order.OrderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == order.OrderId)
                .ToListAsync();

            return order;
        }


        public async Task<bool> DeleteOrderAsync(int orderId, int? restaurantId = null)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Menu)
                .ThenInclude(m => m.Restaurant)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return false;

            // If restaurantId is specified (from Restaurant), ensure order belongs to that restaurant
            if (restaurantId.HasValue)
            {
                bool isRelated = order.OrderItems.Any(oi => oi.Menu.RestaurantId == restaurantId.Value);
                if (!isRelated)
                    return false;
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Menu)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByRestaurantAsync(int restaurantId)
        {
            //return await _context.Orders
            //    .Include(o => o.OrderItems)
            //    .ThenInclude(oi => oi.Menu)
            //    .Include(o => o.User)
            //    .Where(o => o.OrderItems.Any(oi => oi.Menu.RestaurantId == restaurantId))
            //    .ToListAsync();
            return await _context.Orders
       .Include(o => o.User) // ✅ Include User
       .Include(o => o.OrderItems)
           .ThenInclude(oi => oi.Menu)
       .Where(o => o.OrderItems.Any(oi => oi.Menu.RestaurantId == restaurantId))
       .ToListAsync();
        }

        public async Task<List<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Menu)
                .ThenInclude(m => m.Restaurant)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDTO
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.OrderItems.Sum(i => i.Menu.Price * i.Quantity),
               DeliveryAddress = o.DeliveryAddress,
                Items = o.OrderItems.Select(oi => new OrderItemDTO
                {
                   
                    MenuId = oi.MenuId,
                    Quantity = oi.Quantity
              
                }).ToList()

            }).ToList();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
