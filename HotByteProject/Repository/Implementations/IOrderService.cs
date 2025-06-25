using HotByteProject.DTO;
using HotByteProject.Models;

namespace HotByteProject.Services.Implementations
{
    public interface IOrderService
    {
        //Task<Order> PlaceOrderAsync(OrderDTO orderDto);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
        Task<IEnumerable<Order>> GetOrdersByRestaurantAsync(int restaurantId);
        Task<List<OrderResponseDTO>> GetAllOrdersAsync(); 

        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<Order> PlaceOrderAsync(int userId, OrderDTO dto);
        Task<bool> DeleteOrderAsync(int orderId, int? restaurantId = null);
    }
}
