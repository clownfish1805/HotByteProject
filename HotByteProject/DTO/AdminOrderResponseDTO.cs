namespace HotByteProject.DTO
{
    public class AdminOrderResponseDTO
    {
        public string Name { get; set; }
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        public List<OrderItemOutputDTO> Items { get; set; } = new();
    }
}
