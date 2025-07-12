namespace HotByteProject.Models
{
    public class Menu
    {
        public int MenuId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public decimal Price { get; set; }
        public string DietaryInfo { get; set; }
        public string TasteInfo { get; set; }

        public string NutritionalInfo { get; set; }
        public string AvailabilityTime { get; set; } 
        public int RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }

        public bool IsDeleted { get; set; } = false;
        public string Status { get; set; }

        public string? ImageUrl { get; set; }


    }
}
