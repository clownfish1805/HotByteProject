using HotByteProject.DTO;
using HotByteProject.Models;

namespace HotByteProject.Services.Implementations
{
    public interface IAdminService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync();
        Task<bool> DeleteRestaurantAsync(int restaurantId);

        Task<IEnumerable<MenuDetailsDTO>> GetAllMenusAsync();
        Task<List<AdminOrderResponseDTO>> GetAllOrdersAsync();

        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> DeleteAdminAsync(int userId);
    }
}
