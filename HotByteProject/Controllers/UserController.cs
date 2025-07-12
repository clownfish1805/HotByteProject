using HotByteProject.DTO;
using HotByteProject.Repository.Implementations;
using HotByteProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotByteProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirstValue("UserId"), out var id) ? id : 0;

        [HttpGet]
        public async Task<IActionResult> GetUserDetails()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized();

            var dto = await _userService.GetUserDtoByIdAsync(userId);
            return dto != null ? Ok(dto) : NotFound("User not found.");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDTO dto)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized();

            var success = await _userService.UpdateUserAsync(userId, dto);
            return success ? Ok("User profile updated.") : NotFound("User not found.");
        }
    }
}
