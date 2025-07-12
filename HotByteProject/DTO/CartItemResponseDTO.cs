namespace HotByteProject.DTO
{
    public class CartItemResponseDTO
    {
        public int CartItemId { get; set; }
        public int MenuId { get; set; }
        public string RestaurantName { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public string ImageUrl { get; set; } // ✅ Include this line

    }
}
