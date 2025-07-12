namespace HotByteProject.DTO
{
    public class CategoryWithMenusDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;


        public List<MenuDetailsDTO> Menus { get; set; } = new();

    }
}
