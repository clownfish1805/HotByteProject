using HotByteProject.DTO;
using HotByteProject.Models;

namespace HotByteProject.Services.Implementations
{
    public interface ICartService
    {
        Task<List<CartItemResponseDTO>> GetCartItemsAsync(int userId);
        Task<CartItemResponseDTO?> AddToCartAsync(int userId, int menuId, int quantity);

        Task<bool> UpdateQuantityAsync(int cartItemId, int quantity);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(int userId);
    }
}
