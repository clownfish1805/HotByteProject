using System.ComponentModel.DataAnnotations;

namespace HotByteProject.DTO
{
    public class RegisterDTO
    {
        // Common (first shown to all)
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "User" or "Restaurant" or "Admin"

        // Shown only for User
        public string? UserName { get; set; }
        public string? UserAddress { get; set; }
        [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Contact number must be 10 digits and cannot start with 0.")]
        public string? UserContact { get; set; }

        // Shown only for Restaurant
        public string? RestaurantName { get; set; }
        public string? RestaurantAddress { get; set; }
        [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Contact number must be 10 digits and cannot start with 0.")]
        public string? RestaurantContact { get; set; }

        public string? ImageUrl { get; set; }

    }
}
