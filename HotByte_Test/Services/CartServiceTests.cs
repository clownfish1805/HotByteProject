using AutoMapper;
using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Mapper; // <-- Your AutoMapper profile namespace
using HotByteProject.Models;
using HotByteProject.Repository.Service;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace HotByteProject.Tests
{
    [TestFixture]
    public class CartServiceTests
    {
        private AppDbContext _context;
        private CartService _cartService;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "CartTestDb")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // ✅ Initialize AutoMapper with your real profile
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>(); // Make sure namespace is correct
            });
            _mapper = mapperConfig.CreateMapper();

            // ✅ Seed required data
            _context.Categories.Add(new Category
            {
                CategoryId = 1,
                CategoryName = "Fast Food"
            });

            _context.Restaurants.Add(new Restaurant
            {
                RestaurantId = 1,
                RestaurantName = "Test Restaurant",
                ContactNumber = "1234567890",
                Location = "Test City",
                UserId = 1
            });

            _context.SaveChanges();

            _context.Menus.Add(new Menu
            {
                MenuId = 1,
                ItemName = "Pizza",
                Price = 100,
                AvailabilityTime = "All Day",
                Description = "Cheesy pizza",
                DietaryInfo = "Vegetarian",
                NutritionalInfo = "400 kcal",
                Status = "Available",
                TasteInfo = "Savory",
                CategoryId = 1,
                RestaurantId = 1,
                IsDeleted = false
            });

            _context.SaveChanges();

            _cartService = new CartService(_context, _mapper);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task AddToCartAsync_ShouldAddItem()
        {
            var result = await _cartService.AddToCartAsync(1, 1, 2);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.MenuId);
            Assert.AreEqual(2, result.Quantity);
            Assert.AreEqual("Pizza", result.ItemName);
        }

        [Test]
        public async Task GetCartItemsAsync_ShouldReturnItems()
        {
            await _cartService.AddToCartAsync(1, 1, 2);

            var items = await _cartService.GetCartItemsAsync(1);

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual("Pizza", items.First().ItemName);
        }

        [Test]
        public async Task UpdateQuantityAsync_ShouldUpdateQuantity()
        {
            var added = await _cartService.AddToCartAsync(1, 1, 2);
            var updated = await _cartService.UpdateQuantityAsync(added.CartItemId, 5);

            var item = await _context.CartItems.FindAsync(added.CartItemId);

            Assert.IsTrue(updated);
            Assert.AreEqual(5, item.Quantity);
        }

        [Test]
        public async Task RemoveFromCartAsync_ShouldRemoveItem()
        {
            var added = await _cartService.AddToCartAsync(1, 1, 2);
            var result = await _cartService.RemoveFromCartAsync(added.CartItemId);

            var exists = await _context.CartItems.FindAsync(added.CartItemId);

            Assert.IsTrue(result);
            Assert.IsNull(exists);
        }

        [Test]
        public async Task ClearCartAsync_ShouldRemoveAllItems()
        {
            await _cartService.AddToCartAsync(1, 1, 1);
            await _cartService.AddToCartAsync(1, 1, 2);

            var result = await _cartService.ClearCartAsync(1);
            var items = await _cartService.GetCartItemsAsync(1);

            Assert.IsTrue(result);
            Assert.IsEmpty(items);
        }
    }
}
