using HotByteProject.Context;
using HotByteProject.Models;
using HotByteProject.Repository.Service;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HotByteProject.Tests.Services
{
    public class AdminServiceTests
    {
        private AppDbContext _context;
        private AdminService _adminService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "HotByteTestDb_" + System.Guid.NewGuid())
                .Options;

            _context = new AppDbContext(options);
            _adminService = new AdminService(_context);

            // Seed sample data
            SeedData();
        }

        private void SeedData()
        {
            var user = new User
            {
                UserId = 1,
                Name = "Test User",
                Address = "123 Street",
                Email = "user1@example.com",
                Role = "User",
                PasswordHash = "hashed"
            };

            var admin = new User
            {
                UserId = 2,
                Name = "Admin",
                Address = "Admin HQ",
                Email = "admin@example.com",
                Role = "Admin",
                PasswordHash = "hashed"
            };

            var restaurantUser = new User
            {
                UserId = 3,
                Name = "RestOwner",
                Address = "Rest Street",
                Email = "rest@example.com",
                Role = "Restaurant",
                PasswordHash = "hashed"
            };

            var restaurant = new Restaurant
            {
                RestaurantId = 1,
                RestaurantName = "Testaurant",
                ContactNumber = "9999999999",
                Location = "Main Road",
                UserId = 3
            };

            var category = new Category
            {
                CategoryId = 1,
                CategoryName = "Idli"
            };

            var menu = new Menu
            {
                MenuId = 1,
                ItemName = "Plain Idli",
                Description = "Soft steamed idli",
                CategoryId = 1,
                RestaurantId = 1,
                Price = 40,
                DietaryInfo = "Vegetarian",
                TasteInfo = "Savory",
                AvailabilityTime = "Morning",
                NutritionalInfo = "Low Calorie",
                Restaurant = restaurant,
                Category = category,
                Status = "Available"
            };

            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = "Completed",
                DeliveryAddress = "123 Delivery Street",
                OrderDate = System.DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { MenuId = 1, Quantity = 2, Menu = menu }
                }
            };

            _context.Users.AddRange(user, admin, restaurantUser);
            _context.Restaurants.Add(restaurant);
            _context.Categories.Add(category);
            _context.Menus.Add(menu);
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldReturnUserRoleOnly()
        {
            var users = await _adminService.GetAllUsersAsync();
            Assert.That(users.Any(u => u.Role == "User"));
            Assert.That(users.All(u => u.Role == "User"));
        }

        [Test]
        public async Task GetAllRestaurantsAsync_ShouldReturnRestaurants()
        {
            var restaurants = await _adminService.GetAllRestaurantsAsync();
            Assert.That(restaurants.Count(), Is.EqualTo(1));
            Assert.That(restaurants.First().RestaurantName, Is.EqualTo("Testaurant"));
        }

        [Test]
        public async Task DeleteRestaurantAsync_ShouldDeleteRestaurantAndUser()
        {
            var success = await _adminService.DeleteRestaurantAsync(1);
            Assert.IsTrue(success);

            var restaurant = await _context.Restaurants.FindAsync(1);
            var user = await _context.Users.FindAsync(3);

            Assert.IsNull(restaurant);
            Assert.IsNull(user);
        }

        [Test]
        public async Task GetAllMenusAsync_ShouldReturnMenus()
        {
            var menus = await _adminService.GetAllMenusAsync();
            Assert.That(menus.Count(), Is.EqualTo(1));
            Assert.That(menus.First().ItemName, Is.EqualTo("Plain Idli"));
        }

        [Test]
        public async Task GetAllOrdersAsync_ShouldReturnOrdersWithTotal()
        {
            var orders = await _adminService.GetAllOrdersAsync();
            Assert.That(orders.Count, Is.EqualTo(1));
            Assert.That(orders.First().TotalAmount, Is.EqualTo(80)); // 2 * 40
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnCorrectUser()
        {
            var user = await _adminService.GetUserByIdAsync(1);
            Assert.IsNotNull(user);
            Assert.That(user.Email, Is.EqualTo("user1@example.com"));
        }

        [Test]
        public async Task DeleteAdminAsync_ShouldDeleteAdminOnly()
        {
            var success = await _adminService.DeleteAdminAsync(2);
            Assert.IsTrue(success);

            var user = await _context.Users.FindAsync(2);
            Assert.IsNull(user);
        }

        [Test]
        public async Task DeleteAdminAsync_ShouldFailIfNotAdmin()
        {
            var success = await _adminService.DeleteAdminAsync(1); // user role
            Assert.IsFalse(success);

            var user = await _context.Users.FindAsync(1);
            Assert.IsNotNull(user);
        }
    }
}
