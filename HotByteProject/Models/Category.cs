using System.ComponentModel.DataAnnotations;

namespace HotByteProject.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsDeleted { get; set; } = false;

        public string? ImageUrl { get; set; }

        public ICollection<Menu> Menus { get; set; } = new List<Menu>();

    }
}
