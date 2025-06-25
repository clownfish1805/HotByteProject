using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Repository.Implementations;
using Microsoft.EntityFrameworkCore;

namespace HotByteProject.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryWithMenusDTO>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.Menus)
                .ThenInclude(m => m.Restaurant)
                .ToListAsync();

            return categories.Select(c => new CategoryWithMenusDTO
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Menus = c.Menus.Select(m => new MenuDetailsDTO
                {
                    MenuId = m.MenuId,
                    ItemName = m.ItemName,
                    Description = m.Description,
                    Category = c.CategoryName,
                    Price = m.Price,
                    DietaryInfo = m.DietaryInfo,
                    TasteInfo = m.TasteInfo,
                    AvailabilityTime = m.AvailabilityTime,
                    NutritionalInfo = m.NutritionalInfo,
                    RestaurantId = m.RestaurantId,
                    RestaurantName = m.Restaurant?.RestaurantName
                }).ToList()
            }).ToList();
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _context.Categories
                .Include(c => c.Menus)
                .ThenInclude(m => m.Restaurant)
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == name.ToLower());
        }

        public async Task<bool> CreateCategoryAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            bool exists = await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == name.ToLower());
            if (exists) return false;

            _context.Categories.Add(new Category { CategoryName = name });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryByNameAsync(string name)
        {
            var category = await _context.Categories
    .Include(c => c.Menus)
    .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == name.ToLower() && !c.IsDeleted);


            if (category == null) return false;

            category.IsDeleted = true;
            foreach (var menu in category.Menus)
            {
                menu.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
