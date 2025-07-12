using HotByteProject.DTO;
using HotByteProject.Models;

public interface IMenuService
{
    Task<IEnumerable<MenuDetailsDTO>> GetAllMenusAsync();
    Task<IEnumerable<MenuDetailsDTO>> GetMenusByRestaurantAsync(int restaurantId);
    Task<Menu?> GetMenuByIdAsync(int id);
    Task<MenuDetailsDTO?> GetMenuByIdWithRestaurantAsync(int id);

    Task<List<MenuDetailsDTO>> GetAllMenus();
    
    Task<MenuDTO> AddMenuAsync(MenuDTO menuDto); // ImageUrl will be passed in dto
    Task<bool> UpdateMenuAsync(int id, MenuCreateUpdateDTO dto, int restaurantId);

    Task<bool> DeleteMenuByNameAsync(string itemName, int? restaurantId);
    //Task<bool?> UpdateMenuAsync(int v1, MenuDTO dto, int v2);
}
