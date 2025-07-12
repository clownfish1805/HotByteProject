using AutoMapper;
using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using Microsoft.EntityFrameworkCore;

namespace HotByteProject.Repository.Service
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MenuService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MenuDetailsDTO>> GetAllMenusAsync()
        {
            var menus = await _context.Menus
                .Include(m => m.Restaurant)
                .Include(m => m.Category)
                .ToListAsync();

            return _mapper.Map<List<MenuDetailsDTO>>(menus);
        }

        public async Task<MenuDetailsDTO?> GetMenuByIdWithRestaurantAsync(int id)
        {
            var menu = await _context.Menus
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.MenuId == id);

            return menu == null ? null : _mapper.Map<MenuDetailsDTO>(menu);
        }

        public async Task<IEnumerable<MenuDetailsDTO>> GetMenusByRestaurantAsync(int restaurantId)
        {
            var menus = await _context.Menus
                .Include(m => m.Restaurant)
                .Include(m => m.Category)
                .Where(m => m.RestaurantId == restaurantId)
                .ToListAsync();

            return _mapper.Map<List<MenuDetailsDTO>>(menus);
        }

        public async Task<Menu?> GetMenuByIdAsync(int id)
        {
            return await _context.Menus.FindAsync(id);
        }

        public async Task<List<MenuDetailsDTO>> GetAllMenus()
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
                    NutritionalInfo = m.NutritionalInfo,
                    RestaurantId = m.RestaurantId,
                    RestaurantName = m.Restaurant.RestaurantName,
                    Status = m.Status,
                    ImageUrl = m.ImageUrl
                })
                .ToListAsync();
        }


        public async Task<MenuDTO?> AddMenuAsync(MenuDTO dto)
        {
            var category = await _context.Categories
    .FirstOrDefaultAsync(c => c.CategoryName.ToLower().Trim() == dto.CategoryName.ToLower().Trim() && !c.IsDeleted);


            // ❌ If category not found, throw a clear exception
            if (category == null)
            {
                var available = await _context.Categories
                    .Where(c => !c.IsDeleted)
                    .Select(c => c.CategoryName)
                    .ToListAsync();

                throw new Exception($"Invalid category name '{dto.CategoryName}'. Available: {string.Join(", ", available)}");
            }

            // ✅ Create and populate the Menu object
            var menu = new Menu
            {
                ItemName = dto.ItemName,
                Description = dto.Description,
                CategoryId = category.CategoryId, // use resolved CategoryId
                Price = dto.Price,
                DietaryInfo = dto.DietaryInfo,
                TasteInfo = dto.TasteInfo,
                AvailabilityTime = dto.AvailabilityTime,
                NutritionalInfo = dto.NutritionalInfo,
                RestaurantId = dto.RestaurantId,
                Status = dto.Status,
                ImageUrl = dto.ImageUrl,
                IsDeleted = false
            };

            // 💾 Save to database
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return dto;
        }


        public async Task<bool> UpdateMenuAsync(int id, MenuCreateUpdateDTO dto, int restaurantId)
        {
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.MenuId == id && m.RestaurantId == restaurantId && !m.IsDeleted);

            if (menu == null)
                return false;

            // ✅ Normalize input and match category
            var inputCategory = dto.CategoryName?.Trim().ToLower();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == inputCategory);

            if (category == null)
            {
                // Optional: show available categories for debugging
                var allCategories = await _context.Categories.Select(c => c.CategoryName).ToListAsync();
                var debugList = string.Join(", ", allCategories);
                throw new Exception($"Invalid category name '{dto.CategoryName}'. Available: {debugList}");
            }

            // Update fields
            menu.ItemName = dto.ItemName;
            menu.Description = dto.Description;
            menu.CategoryId = category.CategoryId;
            menu.Price = dto.Price;
            menu.DietaryInfo = dto.DietaryInfo;
            menu.TasteInfo = dto.TasteInfo;
            menu.NutritionalInfo = dto.NutritionalInfo;
            menu.AvailabilityTime = dto.AvailabilityTime;
            menu.Status = dto.Status;

            // ✅ Handle new image if provided
            if (dto.ImageFile != null)
            {
                var fileName = $"{Guid.NewGuid()}_{dto.ImageFile.FileName}";
                var filePath = Path.Combine("wwwroot/images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                menu.ImageUrl = $"/images/{fileName}";
            }

            await _context.SaveChangesAsync();
            return true;
        }


        //public async Task<bool> DeleteMenuByNameAsync(string itemName, int restaurantId)
        //{
        //    var menu = await _context.Menus
        //        .FirstOrDefaultAsync(m =>
        //            m.ItemName.ToLower() == itemName.ToLower() &&
        //            m.RestaurantId == restaurantId &&
        //            !m.IsDeleted);

        //    if (menu == null)
        //        return false;

        //    menu.IsDeleted = true;
        //    await _context.SaveChangesAsync();
        //    return true;
        //}

        public async Task<bool> DeleteMenuByNameAsync(string itemName, int? restaurantId = null)
        {
            var query = _context.Menus.Where(m => m.ItemName == itemName && !m.IsDeleted);

            if (restaurantId.HasValue)
            {
                query = query.Where(m => m.RestaurantId == restaurantId.Value);
            }

            var menu = await query.FirstOrDefaultAsync();
            if (menu == null) return false;

            menu.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
