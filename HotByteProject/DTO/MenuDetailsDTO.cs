namespace HotByteProject.DTO
{
    public class MenuDetailsDTO
    {
        public int MenuId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string DietaryInfo { get; set; } = string.Empty;
        public string TasteInfo { get; set; } = string.Empty;
        public string AvailabilityTime { get; set; } = string.Empty;
        public string NutritionalInfo { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public string Status { get; set; }
        public string? ImageUrl { get; set; }


    }
}
