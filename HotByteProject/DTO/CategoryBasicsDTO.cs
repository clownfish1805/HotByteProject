// DTO/CategoryBasicDTO.cs
namespace HotByteProject.DTO
{
    public class CategoryBasicDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public string? ImageUrl { get; set; }
        // For file upload (optional, used only in [FromForm])
        public IFormFile? ImageFile { get; set; }


    }
}
