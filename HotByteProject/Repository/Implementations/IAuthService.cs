using HotByteProject.DTO;

namespace HotByteProject.Services.Implementations
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(RegisterDTO model);
        Task<string?> LoginAsync(LoginDTO model);

    }
}
