using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using System;

namespace HotByteProject.Repository.Service
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.Where(u => u.Role == "User").ToListAsync();
        }

        public async Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync()
        {
            return await _context.Restaurants.Include(r => r.User).ToListAsync();
        }
        public async Task<bool> DeleteRestaurantAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null) return false;

            var user = await _context.Users.FindAsync(restaurant.UserId);

            if (user != null)
                _context.Users.Remove(user);

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<MenuDetailsDTO>> GetAllMenusAsync()
        {
            return await _context.Menus
                .Include(m => m.Restaurant)
                .Include(m => m.Category)
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
                    RestaurantId = m.RestaurantId,
                    RestaurantName = m.Restaurant.RestaurantName
                })
                .ToListAsync();
        }


        public async Task<List<AdminOrderResponseDTO>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Menu)
                .ToListAsync();

            return orders.Select(o => new AdminOrderResponseDTO
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.OrderItems.Sum(i => i.Menu.Price * i.Quantity),
                Items = o.OrderItems.Select(oi => new OrderItemOutputDTO
                {
                    MenuId = oi.MenuId,
                    ItemName = oi.Menu.ItemName,
                    Price = oi.Menu.Price,
                    Quantity = oi.Quantity
                }).ToList()
            }).ToList();
        }



        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> DeleteAdminAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.Role == "Admin");
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
