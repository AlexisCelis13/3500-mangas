using Microsoft.AspNetCore.Mvc;
using MiMangaBot.Services;

namespace MiMangaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FirebaseService _firebaseService;

    public AuthController(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var uid = await _firebaseService.CreateUserAsync(request.Email, request.Password);
            return Ok(new { uid });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("user/{uid}")]
    public async Task<IActionResult> GetUser(string uid)
    {
        try
        {
            var user = await _firebaseService.GetUserAsync(uid);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("user/{uid}")]
    public async Task<IActionResult> DeleteUser(string uid)
    {
        try
        {
            await _firebaseService.DeleteUserAsync(uid);
            return Ok(new { message = "Usuario eliminado correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 