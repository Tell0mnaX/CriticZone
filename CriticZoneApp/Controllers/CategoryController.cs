using Microsoft.AspNetCore.Mvc;
using CriticZoneApp.Models;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    public readonly AppDbContext _Context;
    private readonly AuthService _authService;

    public CategoryController(AppDbContext context, AuthService authService)
    {
        _Context = context;
        _authService = authService;
    }

    [HttpGet("{name}/reviews")]
    public async Task<IActionResult> GetReviewsByCategory(string name)
    {
        var category = await _Context.Categories
            .Include(c => c.Reviews)
                .ThenInclude(r => r.User) // facultatif : pour afficher le nom de l’auteur
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

        if (category == null)
            return NotFound($"Catégorie '{name}' introuvable.");

        var reviews = category.Reviews.Select(r => new
        {
            r.Id,
            r.Title,
            r.Rating,
            r.CreatedAt,
            Author = r.User?.Username
        });

        return Ok(reviews);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _Context.Categories
        .Select(c => new 
        {
            c.Id,
            c.Name
        })
        .OrderBy(c => c.Name)
        .ToListAsync();

        return Ok(categories);
    }
}