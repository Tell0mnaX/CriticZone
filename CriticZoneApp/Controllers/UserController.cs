using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using CriticZoneApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase{

    public readonly AppDbContext _Context;
    public UserController(AppDbContext context){_Context = context;}

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers(){
        var users = await _Context.Users.ToListAsync();
        return Ok(users);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var user = await _Context.Users
            .Include(u => u.Reviews)
            .Include(u => u.Comments)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if(user == null)
            return NotFound("Cet utilisateur n'existe pas...");
        
        var profile = new UserProfileDto
        {
            Username = user.Username,
            ReviewCount = user.Reviews.Count(),
            RegisteredAt = user.RegisteredAt,
            CommentCount = user.Comments.Count()
        };

        return Ok(profile);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var user = await _Context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound("Utilisateur non trouvé.");

        user.Bio = dto.Bio;
        user.PhotoUrl = dto.PhotoUrl;

        await _Context.SaveChangesAsync();

        return Ok("Profil mis à jour !");
    }
    
}