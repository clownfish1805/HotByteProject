using System.ComponentModel.DataAnnotations;

namespace HotByteProject.Models
{
    public class User
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format. Must be a valid email like user@example.com.")]

        public string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(User|Restaurant|Admin)$", ErrorMessage = "Role must be User, Restaurant, or Admin.")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Contact number must be 10 digits and cannot start with 0.")]
        public string Contact { get; set; }

        public string? ImageUrl { get; set; }

    }
}
