using System.ComponentModel.DataAnnotations;

namespace HotByteProject.DTO
{
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        
        //[StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string UserAddress { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Contact number must be 10 digits and cannot start with 0.")]
        public string UserContact { get; set; }
    }
}
