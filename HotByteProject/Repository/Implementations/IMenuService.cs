using HotByteProject.DTO;
using HotByteProject.Models;

namespace HotByteProject.Services.Implementations
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuDetailsDTO>> GetAllMenusAsync();
        Task<IEnumerable<MenuDetailsDTO>> GetMenusByRestaurantAsync(int restaurantId);
        Task<Menu?> GetMenuByIdAsync(int id);
        Task<MenuDetailsDTO?> GetMenuByIdWithRestaurantAsync(int id);
        Task<Menu> AddMenuAsync(MenuDTO menuDto);

        Task<bool> UpdateMenuAsync(int id, MenuDTO menuDto, int userId);
        Task<bool> DeleteMenuByNameAsync(string itemName, int restaurantId);
    }
}
