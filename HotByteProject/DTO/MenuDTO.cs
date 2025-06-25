namespace HotByteProject.DTO
{
    public class MenuDTO
    {
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public string DietaryInfo { get; set; } = string.Empty; // Veg / Nonveg
        public string TasteInfo { get; set; } = string.Empty; // Sweet / Spicy / etc
        public string NutritionalInfo { get; set; }
        public string AvailabilityTime { get; set; } = string.Empty; // All Day / Lunch / etc
        public int RestaurantId { get; set; }
        public string Status { get; set; }

    }
}
