using AutoMapper;
using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;
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
            var menus = await _context.Menus.Include(m => m.Restaurant).ToListAsync();
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
                .Where(m => m.RestaurantId == restaurantId)
                .ToListAsync();

            return _mapper.Map<List<MenuDetailsDTO>>(menus);
        }

        public async Task<Menu?> GetMenuByIdAsync(int id)
        {
            return await _context.Menus.FindAsync(id);
        }

        public async Task<Menu> AddMenuAsync(MenuDTO menuDto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == menuDto.CategoryName.ToLower());

            if (category == null)
                throw new Exception("Category not found. Please create the category first.");

            var menu = new Menu
            {
                ItemName = menuDto.ItemName,
                Description = menuDto.Description,
                CategoryId = category.CategoryId,
                Price = menuDto.Price,
                DietaryInfo = menuDto.DietaryInfo,
                TasteInfo = menuDto.TasteInfo,
                NutritionalInfo = menuDto.NutritionalInfo,
                AvailabilityTime = menuDto.AvailabilityTime,
                RestaurantId = menuDto.RestaurantId,
                Status = menuDto.Status

            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return menu; 
        }



        public async Task<bool> UpdateMenuAsync(int id, MenuDTO dto, int restaurantId)
        {
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.MenuId == id && m.RestaurantId == restaurantId);

            if (menu == null)
                return false;

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == dto.CategoryName.ToLower());

            if (category == null)
                throw new Exception("Invalid category name.");

            menu.ItemName = dto.ItemName;
            menu.Description = dto.Description;
            menu.CategoryId = category.CategoryId; 
            menu.Price = dto.Price;
            menu.DietaryInfo = dto.DietaryInfo;
            menu.TasteInfo = dto.TasteInfo;
            menu.AvailabilityTime = dto.AvailabilityTime;

            await _context.SaveChangesAsync();
            return true;
        }



        public async Task<bool> DeleteMenuByNameAsync(string itemName, int restaurantId)
        {
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m =>
                    m.ItemName.ToLower() == itemName.ToLower()
                    && m.RestaurantId == restaurantId
                    && !m.IsDeleted);

            if (menu == null)
                return false;

            menu.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
