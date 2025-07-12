namespace HotByteProject.DTO
{
    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? RestaurantId { get; set; }
        public int? UserId { get; set; }


    }
}
