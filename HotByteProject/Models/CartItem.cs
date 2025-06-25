namespace HotByteProject.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int UserId { get; set; }
        public int MenuId { get; set; }
        public int Quantity { get; set; }
        public User? User { get; set; }
        public Menu? Menu { get; set; }

    }
}
