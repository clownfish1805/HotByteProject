using HotByteProject.Context;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace HotByteProject.Tests
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private AppDbContext _context;
        private CategoryService _service;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("CategoryServiceTestDb")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new CategoryService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreateCategoryAsync_ShouldCreateCategory_WhenNameIsValid()
        {
            var result = await _service.CreateCategoryAsync("Biryani");

            Assert.IsTrue(result);
            Assert.IsNotNull(await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Biryani"));
        }

        [Test]
        public async Task CreateCategoryAsync_ShouldReturnFalse_IfCategoryAlreadyExists()
        {
            _context.Categories.Add(new Category { CategoryName = "Idli" });
            await _context.SaveChangesAsync();

            var result = await _service.CreateCategoryAsync("Idli");

            Assert.IsFalse(result);
        }


        [Test]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategoriesWithMenus()
        {
            var category = new Category
            {
                CategoryName = "Dosa",
                Menus = new List<Menu>
        {
            new Menu
            {
                ItemName = "Masala Dosa",
                Price = 50,
                AvailabilityTime = "Morning",
                Description = "Crispy stuffed dosa",
                DietaryInfo = "Vegetarian",
                NutritionalInfo = "300 kcal",
                Status = "Available",
                TasteInfo = "Savory",
                RestaurantId = 1,
                IsDeleted = false
            }
        }
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = await _service.GetAllCategoriesAsync();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Dosa", result[0].CategoryName);
            Assert.AreEqual("Masala Dosa", result[0].Menus.First().ItemName);
        }


        [Test]
        public async Task GetCategoryByNameAsync_ShouldReturnCategory_WhenNameMatches()
        {
            _context.Categories.Add(new Category { CategoryName = "Snacks" });
            await _context.SaveChangesAsync();

            var result = await _service.GetCategoryByNameAsync("snacks");

            Assert.IsNotNull(result);
            Assert.AreEqual("Snacks", result.CategoryName);
        }

        [Test]
        public async Task DeleteCategoryByNameAsync_ShouldDeleteCategoryAndMenus()
        {
            var category = new Category
            {
                CategoryName = "Desserts",
                IsDeleted = false,
                Menus = new List<Menu>
        {
            new Menu
            {
                ItemName = "Gulab Jamun",
                Price = 30,
                AvailabilityTime = "Evening",
                Description = "Sweet dessert",
                DietaryInfo = "Vegetarian",
                NutritionalInfo = "300 kcal",
                Status = "Available",
                TasteInfo = "Sweet",
                IsDeleted = false,
                RestaurantId = 1
            }
        }
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteCategoryByNameAsync("Desserts");

            // Assert
            var deletedCategory = await _context.Categories
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(c => c.CategoryName == "Desserts");
            var deletedMenu = await _context.Menus
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(m => m.ItemName == "Gulab Jamun");



            Assert.IsTrue(result);

            Assert.IsNotNull(deletedCategory, "deletedCategory is null");
            Assert.IsTrue(deletedCategory.IsDeleted);

            Assert.IsNotNull(deletedMenu, "deletedMenu is null");
            Assert.IsTrue(deletedMenu.IsDeleted);
        }


    }
}
