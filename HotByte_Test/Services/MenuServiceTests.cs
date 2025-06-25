using AutoMapper;
using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Repository.Service;
using HotByteProject.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotByteProject.Tests
{
    [TestFixture]
    public class MenuServiceTests
    {
        private AppDbContext _context;
        private IMenuService _menuService;
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "MenuServiceTestDb")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Menu, MenuDetailsDTO>()
                    .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant.RestaurantName))
                    .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.CategoryName));
            });

            _mapper = config.CreateMapper();
            _menuService = new MenuService(_context, _mapper);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private Menu CreateValidMenu(string name, int restaurantId, int? menuId = null)
        {
            return new Menu
            {
                MenuId = menuId ?? 0,
                ItemName = name,
                Description = "Description",
                CategoryId = 1,
                RestaurantId = restaurantId,
                Price = 50,
                DietaryInfo = "Veg",
                TasteInfo = "Savory",
                NutritionalInfo = "100 kcal",
                AvailabilityTime = "Morning",
                Status = "Available",
                IsDeleted = false
            };
        }

        [Test]
        public async Task AddMenuAsync_ShouldAddMenu_WhenCategoryExists()
        {
            var category = new Category { CategoryName = "Snacks" };
            _context.Categories.Add(category);

            var restaurant = new Restaurant
            {
                RestaurantName = "Testaurant",
                ContactNumber = "9876543210",
                Location = "City Center",
                UserId = 99
            };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var dto = new MenuDTO
            {
                ItemName = "Samosa",
                Description = "Spicy snack",
                CategoryName = "Snacks",
                Price = 25,
                DietaryInfo = "Vegetarian",
                TasteInfo = "Spicy",
                NutritionalInfo = "200 kcal",
                AvailabilityTime = "Morning",
                RestaurantId = restaurant.RestaurantId,
                Status = "Available"
            };

            var result = await _menuService.AddMenuAsync(dto);

            Assert.NotNull(result);
            Assert.AreEqual("Samosa", result.ItemName);
        }

        [Test]
        public void AddMenuAsync_ShouldThrowException_WhenCategoryNotFound()
        {
            var dto = new MenuDTO
            {
                ItemName = "Samosa",
                Description = "Spicy snack",
                CategoryName = "Invalid",
                Price = 25,
                DietaryInfo = "Vegetarian",
                TasteInfo = "Spicy",
                NutritionalInfo = "200 kcal",
                AvailabilityTime = "Morning",
                Status = "Available",
                RestaurantId = 1
            };

            Assert.ThrowsAsync<Exception>(async () => await _menuService.AddMenuAsync(dto));
        }

        [Test]
        public async Task DeleteMenuByNameAsync_ShouldDeleteMenu_IfRestaurantOwnsIt()
        {
            // Arrange
            var category = new Category { CategoryName = "Tiffin" };
            _context.Categories.Add(category);

            var restaurant = new Restaurant
            {
                RestaurantName = "SoftMeal",
                ContactNumber = "9876543210",
                Location = "Downtown",
                UserId = 77
            };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var menu = new Menu
            {
                ItemName = "Dosa",
                Description = "Crispy dosa",
                CategoryId = category.CategoryId,
                RestaurantId = restaurant.RestaurantId,
                Price = 50,
                DietaryInfo = "Vegetarian",
                TasteInfo = "Savory",
                NutritionalInfo = "200 kcal",
                AvailabilityTime = "Morning",
                Status = "Available",
                IsDeleted = false
            };
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            // Act
            var result = await _menuService.DeleteMenuByNameAsync("dosa", restaurant.RestaurantId);
            var updatedMenu = await _context.Menus
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(m => m.ItemName == "Dosa");


            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(updatedMenu);
            Assert.IsTrue(updatedMenu.IsDeleted);
        }


        [Test]
        public async Task DeleteMenuByNameAsync_ShouldReturnFalse_IfMenuNotFoundOrNotOwned()
        {
            var result = await _menuService.DeleteMenuByNameAsync("Pizza", 1);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task GetMenusByRestaurantAsync_ShouldReturnMenus()
        {
            // Arrange: add category
            var category = new Category { CategoryName = "Lunch" };
            _context.Categories.Add(category);

            // Add restaurant
            var restaurant = new Restaurant
            {
                RestaurantName = "SpiceHub",
                ContactNumber = "9876543210",
                Location = "Center City",
                UserId = 101
            };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            // Add menu using correct RestaurantId and CategoryId
            var menu = new Menu
            {
                ItemName = "Burger",
                Description = "Tasty veg burger",
                CategoryId = category.CategoryId,
                RestaurantId = restaurant.RestaurantId,
                Price = 80,
                DietaryInfo = "Veg",
                TasteInfo = "Savory",
                NutritionalInfo = "300 kcal",
                AvailabilityTime = "Afternoon",
                Status = "Available"
            };
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            // Act
            var result = await _menuService.GetMenusByRestaurantAsync(restaurant.RestaurantId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Burger", result.First().ItemName);
        }

        [Test]
        public async Task GetAllMenusAsync_ShouldReturnAllMenus()
        {
            // Arrange: Add a category
            var category = new Category { CategoryName = "Breakfast" };
            _context.Categories.Add(category);

            // Add a restaurant
            var restaurant = new Restaurant
            {
                RestaurantName = "Testaurant",
                ContactNumber = "9999999999",
                Location = "Main Street",
                UserId = 123
            };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            // Add menu
            var menu = new Menu
            {
                ItemName = "Idli",
                Description = "Steamed rice cake",
                CategoryId = category.CategoryId,
                RestaurantId = restaurant.RestaurantId,
                Price = 30,
                DietaryInfo = "Vegetarian",
                TasteInfo = "Mild",
                NutritionalInfo = "Low Calorie",
                AvailabilityTime = "Morning",
                Status = "Available"
            };
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            // Act
            var result = await _menuService.GetAllMenusAsync();

            // Assert
            Assert.IsNotEmpty(result);
            Assert.That(result.Any(m => m.ItemName == "Idli"));
        }


        [Test]
        public async Task UpdateMenuAsync_ShouldUpdateMenu_WhenValid()
        {
            var category = new Category { CategoryName = "Breakfast" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var menu = CreateValidMenu("Vada", 3, 1);
            menu.CategoryId = category.CategoryId;
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            var dto = new MenuDTO
            {
                ItemName = "Medu Vada",
                Description = "Crispy",
                CategoryName = "Breakfast",
                Price = 20,
                DietaryInfo = "Veg",
                TasteInfo = "Savory",
                NutritionalInfo = "150 kcal",
                AvailabilityTime = "Morning",
                Status = "Available",
                RestaurantId = 3
            };

            var result = await _menuService.UpdateMenuAsync(1, dto, 3);
            var updated = await _context.Menus.FirstAsync(m => m.MenuId == 1);

            Assert.IsTrue(result);
            Assert.AreEqual("Medu Vada", updated.ItemName);
        }

        [Test]
        public async Task UpdateMenuAsync_ShouldReturnFalse_IfNotOwned()
        {
            var menu = CreateValidMenu("Pongal", 1, 100);
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            var dto = new MenuDTO
            {
                ItemName = "Sweet Pongal",
                CategoryName = "Breakfast",
                RestaurantId = 2
            };

            var result = await _menuService.UpdateMenuAsync(100, dto, 999);
            Assert.IsFalse(result);
        }
    }
}
