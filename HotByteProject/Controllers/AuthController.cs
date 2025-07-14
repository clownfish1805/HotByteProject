using HotByteProject.DTO;
using HotByteProject.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using log4net;

namespace HotByteProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthController));

        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;

        public AuthController(IAuthService authService, IAdminService adminService)
        {
            _authService = authService;
            _adminService = adminService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            _logger.Info($"Registration attempt for: {dto.Email}");

            try
            {
                var token = await _authService.RegisterAsync(dto);
                if (token == null)
                {
                    _logger.Warn($"Registration failed - email already exists: {dto.Email}");
                    return BadRequest("Email already exists.");
                }

                _logger.Info($"User registered successfully: {dto.Email}");
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception during registration for {dto.Email}", ex);
                return StatusCode(500, $"Error during registration: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            _logger.Info($"Login attempt for: {dto.Email}");

            try
            {
                var token = await _authService.LoginAsync(dto);
                if (token == null)
                {
                    _logger.Warn($"Login failed - invalid credentials: {dto.Email}");
                    return Unauthorized("Invalid credentials.");
                }

                _logger.Info($"User logged in successfully: {dto.Email}");
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception during login for {dto.Email}", ex);
                return StatusCode(500, $"Error during login: {ex.Message}");
            }
        }
      
     

    }
}
