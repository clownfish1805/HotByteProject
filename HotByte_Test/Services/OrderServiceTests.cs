using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Repository.Service;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotByteProject.Tests.Services
{
    [TestFixture]
    public class OrderServiceTests
    {
        private AppDbContext _context;
        private OrderService _orderService;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            _context = new AppDbContext(options);
            _orderService = new OrderService(_context);
        }

        [Test]
        public async Task PlaceOrderAsync_WithValidCartItems_ShouldCreateOrder()
        {
            // Arrange
            var userId = 1;
            var restaurant = new Restaurant
            {
                RestaurantId = 1,
                RestaurantName = "Test Resto",
                ContactNumber = "9999999999",     
                Location = "Bangalore"     
            };
            var menu = new Menu
            {
                MenuId = 1,
                ItemName = "Dosa",
                Price = 50,
                Restaurant = restaurant,
                RestaurantId = restaurant.UserId,

                AvailabilityTime = "8 AM - 11 AM",
                Description = "South Indian breakfast dish",
                DietaryInfo = "Vegetarian",
                NutritionalInfo = "300 kcal per serving",
                TasteInfo = "Savory and crispy",
                Status= "Available"
            };

            var cartItem = new CartItem { UserId = userId, Menu = menu, MenuId = menu.MenuId, Quantity = 2 };

            await _context.Restaurants.AddAsync(restaurant);
            await _context.Menus.AddAsync(menu);
            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            var orderDto = new OrderDTO { DeliveryAddress = "Dandeli" };

            // Act
            var result = await _orderService.PlaceOrderAsync(userId, orderDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result.UserId);
            Assert.AreEqual("Pending", result.Status);
            Assert.AreEqual("Test Resto", result.RestaurantName);
            Assert.AreEqual(100, result.TotalAmount);
        }

        [Test]
        public async Task DeleteOrderAsync_WithValidOrder_ShouldReturnTrue()
        {
            // Arrange
            var restaurant = new Restaurant
            {
                RestaurantId = 2,
                RestaurantName = "DeleteTest",
                ContactNumber = "8888888888",
                Location = "Chennai"
            };

            var menu = new Menu
            {
                MenuId = 2,
                ItemName = "Idli",
                Price = 30,
                Restaurant = restaurant,
                RestaurantId = restaurant.RestaurantId,
                AvailabilityTime = "7 AM - 10 AM",
                Description = "Steamed rice cakes",
                DietaryInfo = "Vegetarian",
                NutritionalInfo = "200 kcal per serving",
                TasteInfo = "Soft and mild",
                Status="Available"
            };

            var order = new Order
            {
                OrderId = 10,
                UserId = 1,
                Status = "Pending",
                RestaurantName = "DeleteTest",
                OrderDate = DateTime.UtcNow,
                DeliveryAddress = "Mysore"
            };

            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                Menu = menu,
                MenuId = menu.MenuId,
                Quantity = 1
            };
            await _context.Restaurants.AddAsync(restaurant);
            await _context.Menus.AddAsync(menu);
            await _context.Orders.AddAsync(order);
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.DeleteOrderAsync(order.OrderId, restaurant.RestaurantId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(await _context.Orders.FindAsync(order.OrderId));
        }

        [Test]
        public async Task UpdateOrderStatusAsync_WithValidOrder_ShouldUpdateStatus()
        {
            // Arrange
            var order = new Order { OrderId = 99, UserId = 1, Status = "Pending", OrderDate = DateTime.UtcNow, RestaurantName = "UpdateTest", DeliveryAddress = "Test Address" };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(order.OrderId, "Completed");

            // Assert
            Assert.IsTrue(result);
            var updatedOrder = await _context.Orders.FindAsync(order.OrderId);
            Assert.AreEqual("Completed", updatedOrder.Status);
        }

        [Test]
        public async Task GetOrdersByUserAsync_ShouldReturnUserOrders()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 20,
                UserId = 5,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                RestaurantName = "UserTest",
                DeliveryAddress = "Some Address" // ✅ Add this
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.GetOrdersByUserAsync(5);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(5, result.First().UserId);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
