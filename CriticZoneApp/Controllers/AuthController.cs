using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CriticZoneApp.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public AuthController(AppDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already exists.");

        _authService.CreatePasswordHash(request.Password, out var hash, out var salt);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = hash,
            PasswordSalt = salt,
            RegisteredAt = DateTime.UtcNow,
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.CreateToken(user);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null) return Unauthorized("User not found.");

        if (!_authService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized("Invalid password.");

        var token = _authService.CreateToken(user);
        return Ok(new { token });
    }
}
