namespace HotByteProject.DTO
{
    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; }

        public string UserName { get; set; } // ✅ Add this

        public List<OrderItemDTO> Items { get; set; }
    }

}
