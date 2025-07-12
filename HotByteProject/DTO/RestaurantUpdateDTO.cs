namespace HotByteProject.DTO
{
    public class RestaurantUpdateDTO
    {
        public string RestaurantName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }
    }
    }
