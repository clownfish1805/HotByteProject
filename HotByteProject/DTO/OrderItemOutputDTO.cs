namespace HotByteProject.DTO
{
    public class OrderItemOutputDTO
    {
        public int MenuId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
