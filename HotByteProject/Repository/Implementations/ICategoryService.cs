using HotByteProject.DTO;
using HotByteProject.Models;

namespace HotByteProject.Repository.Implementations
{
    public interface ICategoryService
    {
        Task<List<CategoryWithMenusDTO>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByNameAsync(string name);
        //Task<bool> CreateCategoryAsync(string name);
        Task<bool> DeleteCategoryByNameAsync(string name);
        Task<List<CategoryBasicDTO>> GetCategoryNamesAsync();
        Task<bool> CreateCategoryAsync(string name, string? imageUrl = null);
        Task<bool> CategoryExistsAsync(string name);


    }
}
