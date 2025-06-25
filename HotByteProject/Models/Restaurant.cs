using System.ComponentModel.DataAnnotations;

namespace HotByteProject.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string RestaurantName { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression("^[1-9][0-9]{9}$", ErrorMessage = "Contact number must not start with 0 and must be exactly 10 digits.")]
        public string ContactNumber { get; set; }

        public User? User { get; set; }

        public ICollection<Menu>? Menus { get; set; }

    }
}
