using HotByteProject.DTO;

namespace HotByteProject.Services
{
    public interface IUserService
    {
        Task<UpdateUserDTO?> GetUserDtoByIdAsync(int userId);
        Task<bool> UpdateUserAsync(int userId, UpdateUserDTO dto);
    }
}
