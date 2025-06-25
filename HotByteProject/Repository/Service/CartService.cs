using HotByteProject.Context;
using HotByteProject.DTO;
using HotByteProject.Models;
using HotByteProject.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using AutoMapper;


namespace HotByteProject.Repository.Service
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CartService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CartItemResponseDTO>> GetCartItemsAsync(int userId)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Menu)
                .ThenInclude(m => m.Restaurant)
                .Where(c => c.UserId == userId && !c.Menu.IsDeleted)
                .ToListAsync();

            return _mapper.Map<List<CartItemResponseDTO>>(cartItems);
        }


        public async Task<CartItemResponseDTO?> AddToCartAsync(int userId, int menuId, int quantity)
        {
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.MenuId == menuId && !m.IsDeleted);

            if (menu == null)
                return null;

            if (menu.AvailabilityTime.ToLower() == "unavailable")
                return null;

            var existingCartItems = await _context.CartItems
                .Include(c => c.Menu)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (existingCartItems.Any())
            {
                var existingRestaurantId = existingCartItems.First().Menu.RestaurantId;

                if (existingRestaurantId != menu.RestaurantId)
                {
                    // ❌ Conflict — mixed restaurant order not allowed
                    throw new InvalidOperationException("You can only order from one restaurant at a time. Please clear your cart first.");
                }
            }



            var cartItem = new CartItem
            {
                UserId = userId,
                MenuId = menuId,
                Quantity = quantity
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            return new CartItemResponseDTO
            {
                CartItemId = cartItem.CartItemId,
                MenuId = menuId,
                ItemName = menu.ItemName,
                Price = menu.Price,
                Quantity = quantity
            };
        }

        public async Task<bool> UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return false;

            item.Quantity = quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
