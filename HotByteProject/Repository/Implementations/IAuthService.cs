using HotByteProject.DTO;

namespace HotByteProject.Services.Implementations
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(RegisterDTO model);
        Task<AuthResponseDTO?> LoginAsync(LoginDTO model);

    }
}
