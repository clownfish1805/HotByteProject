namespace HotByteProject.DTO
{
    public class RestaurantDTO
    {
        public int UserId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}
