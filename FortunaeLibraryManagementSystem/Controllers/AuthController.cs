using FortunaeLibraryManagementSystem.Service.DTO;
using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FortunaeLibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                if (result)
                    return Ok("User registered successfully");
                return BadRequest("User registration failed");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("test-auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            var headers = HttpContext.Request.Headers;
            return Ok(new { AuthorizationHeader = headers["Authorization"].ToString() });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                var token = await _authService.LoginAsync(loginDto.Username, loginDto.Password);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromQuery] Guid id)
        {
            try
            {
                var result = await _authService.DeleteUserAsync(id);
                if (result)
                    return Ok("User deleted successfully");
                return BadRequest("User deletion failed");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
        }
        [HttpGet("user")]
        public async Task<IActionResult> GetUser([FromQuery] Guid id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromQuery] Guid userId, [FromBody] UpdateProfileDTO profileDto)
        {
            try
            {
                var result = await _authService.UpdateProfileAsync(userId, profileDto);
                if (result)
                    return Ok("Profile updated successfully");
                return BadRequest("Profile update failed");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string email, [FromBody] string newPassword)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(email, newPassword);
                if (result)
                    return Ok("Password reset successfully");
                return BadRequest("Password reset failed");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
        }


    }
}
